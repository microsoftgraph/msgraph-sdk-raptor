using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MsGraphSDKSnippetsCompiler.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MsGraphSDKSnippetsCompiler
{
    public class MicrosoftGraphJavaCompiler : IMicrosoftGraphSnippetsCompiler
    {
        private readonly string _markdownFileName;
        private readonly string _previewLibraryPath;
        private readonly string _javaLibVersion;
        private readonly string _javaCoreVersion;
        private static readonly string[] testFileSubDirectories = new string[] { "src", "main", "java", "com", "microsoft", "graph", "raptor" };

        private static readonly string gradleBuildFileName = "build.gradle";
        private static readonly string previewGradleBuildFileTemplate = @"plugins {
    id 'java'
    id 'application'
}
repositories {
    jcenter()
    flatDir {
        dirs '--path--/msgraph-sdk-java-core/build/libs'
        dirs '--path--/msgraph-sdk-java/build/libs'
    }
}
dependencies {
    --deps--
    implementation name: 'msgraph-sdk-java'
    implementation name: 'msgraph-sdk-java-core'
}
application {
    mainClassName = 'com.microsoft.graph.raptor.App'
}";
        private static readonly string v1GradleBuildFileTemplate = @"plugins {
    id 'java'
    id 'application'
}
repositories {
    jcenter()
}
dependencies {
    --deps--
    implementation 'com.microsoft.graph:microsoft-graph-core:--coreversion--'
    implementation 'com.microsoft.graph:microsoft-graph:--libversion--'
}
application {
    mainClassName = 'com.microsoft.graph.raptor.App'
}";
        private static readonly string betaGradleBuildFileTemplate = @"plugins {
    id 'java'
    id 'application'
}
repositories {
    jcenter()
    jcenter{
        	url 'https://oss.jfrog.org/artifactory/oss-snapshot-local'
	}
}
dependencies {
    --deps--
    implementation 'com.microsoft.graph:microsoft-graph-core:--coreversion--'
    implementation 'com.microsoft.graph:microsoft-graph-beta:--libversion--'
}
application {
    mainClassName = 'com.microsoft.graph.raptor.App'
}";
        private const string depsCurrent = @"implementation 'com.google.guava:guava:23.0'";
        private const string depsvNext = @"implementation 'com.google.guava:guava:23.0'
    implementation 'com.google.code.gson:gson:2.8.6'
    implementation 'com.squareup.okhttp3:okhttp:4.9.0'";
        private static readonly string gradleSettingsFileName = "settings.gradle";
        private static readonly string gradleSettingsFileTemplate = @"rootProject.name = 'msgraph-sdk-java-raptor'";

        private static Versions? currentlyConfiguredVersion;
        private static readonly Lazy<int> currentExcutionFolder = new Lazy<int>(() => new Random().Next(0, int.MaxValue));

        public MicrosoftGraphJavaCompiler(string markdownFileName, string previewLibPath, string javaLibVersion, string javaCoreVersion)
        {
            _markdownFileName = markdownFileName;
            _previewLibraryPath = previewLibPath;
            _javaLibVersion = javaLibVersion;
            _javaCoreVersion = javaCoreVersion;
        }
        public CompilationResultsModel CompileSnippet(string codeSnippet, Versions version)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "msgraph-sdk-raptor");
            Directory.CreateDirectory(tempPath);
            var rootPath = Path.Combine(tempPath, "java" + currentExcutionFolder.Value, Guid.NewGuid().ToString());
            var sourceFileDirectory = Path.Combine(new string[] { rootPath }.Union(testFileSubDirectories).ToArray());

            InitializeProjectStructure(version, rootPath).GetAwaiter().GetResult();

            File.WriteAllText(Path.Combine(sourceFileDirectory, "App.java"), codeSnippet); //could be async
            using var javacProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "gradle",
                    Arguments = "build",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = rootPath,
                },
            };
            javacProcess.Start();
            var hasExited = javacProcess.WaitForExit(20000);
            if (!hasExited)
                javacProcess.Kill(true);
            var stdOutput = javacProcess.StandardOutput.ReadToEnd(); //could be async
            var stdErr = javacProcess.StandardError.ReadToEnd(); //could be async

            var config = AppSettings.Config();
            if (bool.Parse(config.GetSection("DeleteJavaTempDirectory").Value))
            {
                Directory.Delete(rootPath, true);
            }

            return new CompilationResultsModel(
                hasExited && stdOutput.Contains("BUILD SUCCESSFUL"),
                GetDiagnosticsFromStdErr(stdOutput, stdErr, hasExited),
                _markdownFileName
            );
        }

        private const string errorsSuffix = "FAILURE";
        private static readonly Regex notesFilterRegex = new Regex(@"^Note:\s[^\n]*$", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex doubleLineReturnCleanupRegex = new Regex(@"\n{2,}", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex errorCountCleanupRegex = new Regex(@"\d+ error", RegexOptions.Compiled);
        private static readonly Regex errorMessageCaptureRegex = new Regex(@":(?<linenumber>\d+):(?<message>[^\/\\]+)", RegexOptions.Compiled | RegexOptions.Multiline);
        private static List<Diagnostic> GetDiagnosticsFromStdErr(string stdOutput, string stdErr, bool hasExited)
        {
            var result = new List<Diagnostic>();
            if(stdErr.Contains(errorsSuffix))
            {
                var diagnosticsToParse = doubleLineReturnCleanupRegex.Replace(
                                                errorCountCleanupRegex.Replace(
                                                    notesFilterRegex.Replace(// we don't need informational notes
                                                        stdErr[0..stdErr.IndexOf(errorsSuffix)], // we want the traces before FAILURE
                                                        string.Empty),
                                                    string.Empty),
                                                string.Empty);
                result.AddRange(errorMessageCaptureRegex
                                            .Matches(diagnosticsToParse)
                                            .Select(x => new { message = x.Groups["message"].Value, linenumber = int.Parse(x.Groups["linenumber"].Value) })
                                            .Select(x => Diagnostic.Create(new DiagnosticDescriptor("JAVA1001", 
                                                                                "Error during Java compilation",
                                                                                x.message,
                                                                                "JAVA1001: 'Java.Language'",
                                                                                DiagnosticSeverity.Error,
                                                                                true),
                                                                            Location.Create("App.java", 
                                                                                new TextSpan(0, 5),
                                                                                new LinePositionSpan(
                                                                                    new LinePosition(x.linenumber, 0),
                                                                                    new LinePosition(x.linenumber, 2))))));
            }
            if (!hasExited)
            {
                result.Add(Diagnostic.Create(new DiagnosticDescriptor("JAVA1000",
                                                                        "Sample didn't finish compiling",
                                                                        "The compilation for that sample timed out",
                                                                        "JAVA1000: 'Gradle.Build'",
                                                                        DiagnosticSeverity.Error,
                                                                        true),
                                            null));
                result.Add(Diagnostic.Create(new DiagnosticDescriptor("JAVA1000",
                                                                        "Sample didn't finish compiling",
                                                                        stdErr,
                                                                        "JAVA1000: 'Gradle.StdErr'",
                                                                        DiagnosticSeverity.Error,
                                                                        true),
                                            null));
                result.Add(Diagnostic.Create(new DiagnosticDescriptor("JAVA1000",
                                                                        "Sample didn't finish compiling",
                                                                        stdOutput,
                                                                        "JAVA1000: 'Gradle.StdOut'",
                                                                        DiagnosticSeverity.Error,
                                                                        true),
                                            null));
            }
            return result;
        }

        private async Task InitializeProjectStructure(Versions version, string rootPath)
        {
            Directory.CreateDirectory(rootPath);
            var buildGradleFileContent = version == Versions.V1 ? v1GradleBuildFileTemplate : betaGradleBuildFileTemplate;
            if (!string.IsNullOrEmpty(_previewLibraryPath))
                buildGradleFileContent = previewGradleBuildFileTemplate.Replace("--path--", _previewLibraryPath);
            await File.WriteAllTextAsync(Path.Combine(rootPath, gradleBuildFileName), buildGradleFileContent
                                                                            .Replace("--deps--", string.IsNullOrEmpty(_previewLibraryPath) ? depsCurrent : depsvNext )
                                                                            .Replace("--coreversion--", _javaCoreVersion)
                                                                            .Replace("--libversion--", _javaLibVersion));
            var gradleSettingsFilePath = Path.Combine(rootPath, gradleSettingsFileName);
            if (!File.Exists(gradleSettingsFilePath))
                await File.WriteAllTextAsync(gradleSettingsFilePath, gradleSettingsFileTemplate);

            CreateDirectoryStructure(rootPath, testFileSubDirectories);
        }

        private static void CreateDirectoryStructure(string rootPath, string[] subdirectoriesNames)
        {
            var dirsAsList = subdirectoriesNames.ToList();
            dirsAsList.ForEach(name =>
            {
                Directory.CreateDirectory(Path.Combine(new string[] { rootPath }.Union(dirsAsList.Take(dirsAsList.IndexOf(name) + 1)).ToArray()));
            });
        }
    }
}

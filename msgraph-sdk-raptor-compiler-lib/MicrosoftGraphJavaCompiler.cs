using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MsGraphSDKSnippetsCompiler.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MsGraphSDKSnippetsCompiler
{
    public class MicrosoftGraphJavaCompiler : IMicrosoftGraphSnippetsCompiler
    {
        private readonly string _markdownFileName;
        private static readonly string[] testFileSubDirectories = new string[] { "src", "main", "java", "com", "microsoft", "graph", "raptor" };

        private static readonly string gradleBuildFileName = "build.gradle";
        private static readonly string v1GradleBuildFileTemplate = @"plugins {
    id 'java'
    id 'application'
}
repositories {
    jcenter()
}
dependencies {
    implementation 'com.google.guava:guava:23.0'
    implementation 'com.microsoft.graph:microsoft-graph-core:1.0.5'
    implementation 'com.microsoft.graph:microsoft-graph:2.3.2'
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
    implementation 'com.google.guava:guava:23.0'
    implementation 'com.microsoft.graph:microsoft-graph-core:1.0.5'
    implementation 'com.microsoft.graph:microsoft-graph-beta:0.1.0-SNAPSHOT'
}
application {
    mainClassName = 'com.microsoft.graph.raptor.App'
}";
        private static readonly string gradleSettingsFileName = "settings.gradle";
        private static readonly string gradleSettingsFileTemplate = @"rootProject.name = 'msgraph-sdk-java-raptor'";

        private static Versions? currentlyConfiguredVersion;
        private static object versionLock = new { };

        private static void setCurrentlyConfiguredVersion (Versions version)
        {// we don't want to overwrite the build.gradle for each test, this prevents gradle from caching things and slows down build time
            lock(versionLock) {
                currentlyConfiguredVersion = version;
            }
        }

        public MicrosoftGraphJavaCompiler(string markdownFileName)
        {
            _markdownFileName = markdownFileName;
        }
        public CompilationResultsModel CompileSnippet(string codeSnippet, Versions version)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "msgraph-sdk-raptor");
            Directory.CreateDirectory(tempPath);
            var rootPath = Path.Combine(tempPath, "java");
            var sourceFileDirectory = Path.Combine(new string[] { rootPath }.Union(testFileSubDirectories).ToArray());
            if (!currentlyConfiguredVersion.HasValue || currentlyConfiguredVersion.Value != version)
            {
                InitializeProjectStructure(version, rootPath).GetAwaiter().GetResult();
                setCurrentlyConfiguredVersion(version);
            }
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
            var hasExited = javacProcess.WaitForExit(10000);
            if (!hasExited)
                javacProcess.Kill(true);
            var stdOutput = javacProcess.StandardOutput.ReadToEnd(); //could be async
            var stdErr = javacProcess.StandardError.ReadToEnd(); //could be async
            return new CompilationResultsModel
            {
                MarkdownFileName = _markdownFileName,
                IsSuccess = hasExited && stdOutput.Contains("BUILD SUCCESSFUL"),
                Diagnostics = GetDiagnosticsFromStdErr(stdOutput, stdErr, hasExited)
            };
        }

        private const string errorsSuffix = "FAILURE";
        private static Regex notesFilterRegex = new Regex(@"^Note:\s[^\n]*$", RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex doubleLineReturnCleanupRegex = new Regex(@"\n{2,}", RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex errorCountCleanupRegex = new Regex(@"\d+ error", RegexOptions.Compiled);
        private static Regex errorMessageCaptureRegex = new Regex(@":(?<linenumber>\d+):(?<message>[^\/\\]+)", RegexOptions.Compiled | RegexOptions.Multiline);
        private List<Diagnostic> GetDiagnosticsFromStdErr(string stdOutput, string stdErr, bool hasExited)
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
            await File.WriteAllTextAsync(Path.Combine(rootPath, gradleBuildFileName), version == Versions.V1 ? v1GradleBuildFileTemplate : betaGradleBuildFileTemplate);
            var gradleSettingsFilePath = Path.Combine(rootPath, gradleSettingsFileName);
            if (!File.Exists(gradleSettingsFilePath))
                await File.WriteAllTextAsync(gradleSettingsFilePath, gradleSettingsFileTemplate);

            CreateDirectoryStructure(rootPath, testFileSubDirectories);
        }

        private void CreateDirectoryStructure(string rootPath, string[] subdirectoriesNames)
        {
            var dirsAsList = subdirectoriesNames.ToList();
            dirsAsList.ForEach(name =>
            {
                Directory.CreateDirectory(Path.Combine(new string[] { rootPath }.Union(dirsAsList.Take(dirsAsList.IndexOf(name) + 1)).ToArray()));
            });
        }
    }
}

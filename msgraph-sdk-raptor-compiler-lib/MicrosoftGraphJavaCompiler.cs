﻿using Microsoft.CodeAnalysis;
using MsGraphSDKSnippetsCompiler.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
                Diagnostics = new List<Diagnostic>() //TODO parse output diagnostics, maybe switch to a generic class?
            };
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

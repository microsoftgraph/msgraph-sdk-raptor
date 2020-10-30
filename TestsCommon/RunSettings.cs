// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System;
using System.IO;

namespace TestsCommon
{
    /// <summary>
    /// Converts a runsettings file into an object after validating settings
    /// </summary>
    public class RunSettings
    {
        public Versions Version { get; set; }
        public string DllPath { get; set; }
        public bool KnownFailuresRequested { get; set; }
        public Languages Language { get; set; }

        public RunSettings() { }

        public RunSettings(TestParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var versionString = parameters.Get("Version");
            var dllPath = parameters.Get("DllPath");
            var knownFailuresRequested = parameters.Get("KnownFailuresRequested");
            Language = parameters.Get("Language").ToUpperInvariant() switch
            {
                "CSHARP" => Languages.CSharp,
                "C#" => Languages.CSharp,
                "JAVA" => Languages.Java,
                "JAVASCRIPT" => Languages.JavaScript,
                "JS" => Languages.JavaScript,
                "OBJC" => Languages.ObjC,
                "OBJECTIVEC" => Languages.ObjC,
                "OBJECTIVE-C" => Languages.ObjC,
                _ => Languages.CSharp
            };

            if (Language == Languages.CSharp && !File.Exists(dllPath))
            {
                throw new ArgumentException("File specified with DllPath in Test.runsettings doesn't exist!");
            }

            DllPath = dllPath;
            Version = VersionString.GetVersion(versionString);
            KnownFailuresRequested = bool.Parse(knownFailuresRequested);
        }
    }
}

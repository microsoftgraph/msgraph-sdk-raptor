// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using MsGraphSDKSnippetsCompiler.Models;
using System;
using System.IO;

namespace TestsCommon
{
    /// <summary>
    /// Init-once access to snippets directory
    /// </summary>
    public static class GraphDocsDirectory
    {
        /// <summary>
        /// Represents where the snippets are stored. Expected to refer to a single directory for each assembly.
        /// </summary>
        private static string SnippetsDirectory = null;

        /// <summary>
        /// Represents the root directory where you checkout microsoft-graph-docs repo
        /// </summary>
        const string RootGitDirectory = @"C:\github";

        /// <summary>
        /// Sets snippets directory only once and refers to the string if it is already set
        /// Assumes that default "git clone <remote-reference>" command is used, in other words,
        /// the repo is always in microsoft-graph-docs folder under RootDirectory defined above
        /// </summary>
        /// <param name="version">Docs version (e.g. V1 or Beta)</param>
        /// <returns>
        /// C# snippets directory
        /// </returns>
        public static string GetCsharpSnippetsDirectory(Versions version)
        {
            if (SnippetsDirectory is object)
            {
                return SnippetsDirectory;
            }

            string versionString = version switch
            {
                Versions.V1 => "v1.0",
                Versions.Beta => "beta",
                _ => throw new ArgumentException("Unexpected version, we can't resolve this to a source code path."),
            };

            var msGraphDocsRepoLocation = Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY") ?? RootGitDirectory;
            SnippetsDirectory = Path.Join(msGraphDocsRepoLocation, $@"microsoft-graph-docs\api-reference\{versionString}\includes\snippets\csharp");

            return SnippetsDirectory;
        }
    }
}

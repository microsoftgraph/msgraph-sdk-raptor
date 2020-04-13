using MsGraphSDKSnippetsCompiler.Models;
using System;
using System.IO;

namespace TestsCommon
{
    public static class GraphDocsDirectory
    {
        private static string SnippetsDirectory = null;
        const string LocalMsGraphDocsRepo = @"C:\github";

        public static string GetDirectory(Versions version)
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

            var msGraphDocsRepoLocation = Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY") ?? LocalMsGraphDocsRepo;
            SnippetsDirectory = Path.Join(msGraphDocsRepoLocation, $@"microsoft-graph-docs\api-reference\{versionString}\includes\snippets\csharp");

            return SnippetsDirectory;
        }
    }
}

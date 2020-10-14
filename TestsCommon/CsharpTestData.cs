using MsGraphSDKSnippetsCompiler.Models;

namespace TestsCommon
{
    public class CsharpTestData
    {
        /// <summary>
        /// Docs version e.g. V1 or Beta
        /// </summary>
        public Versions Version { get; set; }

        /// <summary>
        /// Whether the test case is failing due to a known issue
        /// </summary>
        public bool IsKnownIssue { get; set; }

        /// <summary>
        /// Message to represent known issue
        /// </summary>
        public string KnownIssueMessage { get; set; }

        /// <summary>
        /// Documentation link where snippet is shown
        /// </summary>
        public string DocsLink { get; set; }

        /// <summary>
        /// Snippet file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Optional dll path to load Microsoft.Graph from a local resource instead of published nuget
        /// </summary>
        public string DllPath { get; set; }
    }
}

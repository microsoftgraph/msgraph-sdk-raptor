using MsGraphSDKSnippetsCompiler.Models;

namespace TestsCommon
{
    public record LanguageTestData
    {
        /// <summary>
        /// Docs version e.g. V1 or Beta
        /// </summary>
        public Versions Version { get; init; }

        /// <summary>
        /// Whether the test case is failing due to a known issue
        /// </summary>
        public bool IsKnownIssue { get; init; }

        /// <summary>
        /// Message to represent known issue
        /// </summary>
        public string KnownIssueMessage { get; init; }

        /// <summary>
        /// Documentation link where snippet is shown
        /// </summary>
        public string DocsLink { get; init; }

        /// <summary>
        /// Snippet file name
        /// </summary>
        public string FileName { get; init; }

        /// <summary>
        /// Optional dll path to load Microsoft.Graph from a local resource instead of published nuget
        /// </summary>
        public string DllPath { get; init; }
        /// <summary>
        /// Optional. Version to use for the java core library. Ignored when using JavaPreviewLibPath
        /// </summary>
        public string JavaCoreVersion { get; init; }
        /// <summary>
        /// Optional. Version to use for the java service library. Ignored when using JavaPreviewLibPath
        /// </summary>
        public string JavaLibVersion { get; init; }
        /// <summary>
        /// Optional. Folder container the java core and java service library repositories so the unit testing uses that local version instead.
        /// </summary>
        public string JavaPreviewLibPath { get; init; }
    }
}

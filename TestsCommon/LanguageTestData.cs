using MsGraphSDKSnippetsCompiler.Models;

namespace TestsCommon
{
    /// <summary>
    /// Test Data
    /// </summary>
    /// <param name="Version">Docs version e.g. V1 or Beta</param>
    /// <param name="IsKnownIssue">Whether the test case is failing due to a known issue</param>
    /// <param name="KnownIssueMessage">Message to represent known issue</param>
    /// <param name="DocsLink">Documentation link where snippet is shown</param>
    /// <param name="FileName">Snippet file name</param>
    /// <param name="DllPath">Optional dll path to load Microsoft.Graph from a local resource instead of published nuget</param>
    /// <param name="JavaCoreVersion">Optional. Version to use for the java core library. Ignored when using JavaPreviewLibPath</param>
    /// <param name="JavaLibVersion">Optional. Version to use for the java service library. Ignored when using JavaPreviewLibPath</param>
    /// <param name="JavaPreviewLibPath">Optional. Folder container the java core and java service library repositories so the unit testing uses that local version instead.</param>
    public record LanguageTestData(
        Versions Version,
        bool IsKnownIssue,
        string KnownIssueMessage,
        string DocsLink,
        string FileName,
        string DllPath,
        string JavaCoreVersion,
        string JavaLibVersion,
        string JavaPreviewLibPath,
        string TestName,
        string Owner);

    public record ExecutionTestData(
        LanguageTestData LanguageTestData,
        string FileContent);
}

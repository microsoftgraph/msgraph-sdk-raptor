using MsGraphSDKSnippetsCompiler.Models;

namespace MsGraphSDKSnippetsCompiler
{
    public interface IMicrosoftGraphSnippetsCompiler
    {
        CompilationResultsModel CompileSnippet(string codeSnippet, Versions version);
        ExecutionResultsModel ExecuteSnippet(string codeSnippet, Versions version);
    }
}

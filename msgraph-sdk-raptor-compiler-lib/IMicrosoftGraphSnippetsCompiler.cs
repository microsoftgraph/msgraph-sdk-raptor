using System.Threading.Tasks;
using MsGraphSDKSnippetsCompiler.Models;

namespace MsGraphSDKSnippetsCompiler
{
    public interface IMicrosoftGraphSnippetsCompiler
    {
        CompilationResultsModel CompileSnippet(string codeSnippet, Versions version);
        Task<ExecutionResultsModel> ExecuteSnippet(string codeSnippet, Versions version);
    }
}

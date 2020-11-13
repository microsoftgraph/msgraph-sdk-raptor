using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MsGraphSDKSnippetsCompiler.Models
{
    public record CompilationResultsModel(bool IsSuccess, IEnumerable<Diagnostic> Diagnostics, string MarkdownFileName);
}
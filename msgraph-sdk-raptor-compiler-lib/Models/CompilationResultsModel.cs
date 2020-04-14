using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MsGraphSDKSnippetsCompiler.Models
{
    public class CompilationResultsModel
    {
        public bool IsSuccess { get; set; }
        public IEnumerable<Diagnostic> Diagnostics { get; set; }
        public string MarkdownFileName { get; set; }
    }
}
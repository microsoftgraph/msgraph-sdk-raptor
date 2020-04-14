using MsGraphSDKSnippetsCompiler.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MsGraphSDKSnippetsCompiler.Models
{
    public class CompilationCycleResultsModel
    {
        public List<CompilationResultsModel> CompilationResultsModelList { get; set; }
        public int TotalCompiledSnippets { get; set; }
        public int TotalSnippetsWithError { get; set; }
        public int TotalErrors { get; set; }
        public Languages Language { get; set; }
        public Versions Version { get; set; }
        public decimal ExecutionTime { get; set; }
    }
}
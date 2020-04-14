using System;
using Microsoft.CodeAnalysis;
using MsGraphSDKSnippetsCompiler.Data;
using MsGraphSDKSnippetsCompiler.Models;
using MsGraphSDKSnippetsCompiler.Services;

namespace MsGraphSDKSnippetsCompiler
{
    public class CompilationResultsSQLDatabaseLogger : ICompilationResultsLogger
    {
        private readonly string _connectionString;

        public CompilationResultsSQLDatabaseLogger(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Log the compilation data in a database 
        /// </summary>
        /// <param name="compilationCycleResultsModel">Model with all the necessary data after a compilation cycle</param>
        public void Log(CompilationCycleResultsModel compilationCycleResultsModel)
        {
            //Log Compile Cycle
            CompileCycle compileCycle = new CompileCycle
            {
                CompileCycleID = Guid.NewGuid(),
                TotalCompiledSnippets = compilationCycleResultsModel.TotalCompiledSnippets,
                TotalSnippetsWithError = compilationCycleResultsModel.TotalSnippetsWithError,
                TotalErrors = compilationCycleResultsModel.TotalErrors,
                Language = compilationCycleResultsModel.Language,
                Version = compilationCycleResultsModel.Version,
                ExecutionTime = compilationCycleResultsModel.ExecutionTime,
                CompileDate = DateTime.Now
            };

            ICompileCycle compileCycleData = new CompileCycleData(new RaptorDbContext(_connectionString));
            compileCycleData.Add(compileCycle);

            //Log CompileCycle Results in database
            foreach (CompilationResultsModel compilationResultsModel in compilationCycleResultsModel.CompilationResultsModelList)
            {
                CompileResult compileResult = new CompileResult
                {
                    CompileResultsID = Guid.NewGuid(),
                    CompileCycleID = compileCycle.CompileCycleID,
                    IsSuccess = compilationResultsModel.IsSuccess,
                    FileName = compilationResultsModel.MarkdownFileName,
                    Snippet = compilationResultsModel.Snippet
                };

                ICompileResult compileResultData = new CompileResultData(new RaptorDbContext(_connectionString));
                compileResultData.Add(compileResult);

                if (compilationResultsModel.Diagnostics != null)
                {
                    foreach (Diagnostic diagnostics in compilationResultsModel.Diagnostics)
                    {
                        CompileResultsError compileResultsError = new CompileResultsError
                        {
                            CompileResultsErrorID = Guid.NewGuid(),
                            CompileResultsID = compileResult.CompileResultsID,
                            ErrorCode = diagnostics.Id,
                            IsWarning = diagnostics.IsWarningAsError,
                            WarningLevel = diagnostics.WarningLevel,
                            ErrorMessage = diagnostics.GetMessage()
                        };

                        ICompileResultsError compileResultsErrorData = new CompileResultsErrorData(new RaptorDbContext(_connectionString));
                        compileResultsErrorData.Add(compileResultsError);
                    }
                }
            }
        }
    }
}
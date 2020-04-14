using System;
using System.IO;
using Microsoft.CodeAnalysis;
using MsGraphSDKSnippetsCompiler.Models;

namespace MsGraphSDKSnippetsCompiler
{
    public class CompilationResultsTextFileLogger : ICompilationResultsLogger
    {
        public CompilationResultsTextFileLogger()
        {

        }

        /// <summary>
        /// Log the compilation data in a text file 
        /// </summary>
        /// <param name="compilationCycleResultsModel">Model with all the necessary data after a compilation cycle</param>
        public void Log(CompilationCycleResultsModel compilationCycleResultsModel)
        {
            string logPath = "CompileLog.md";
            if (File.Exists(logPath))
            {
                // Create a log file to write on
                using (StreamWriter streamWriter = File.CreateText(logPath))
                {
                    //Log Compile Cycle
                    streamWriter.WriteLine("|++++++++++++++++++++++++|");
                    streamWriter.Write($"#### Total CompiledSnippets:  {compilationCycleResultsModel.TotalCompiledSnippets}");
                    streamWriter.Write($"#### Total Snippets With Errors: {compilationCycleResultsModel.TotalSnippetsWithError}");
                    streamWriter.Write($"#### Total Errors: {compilationCycleResultsModel.TotalErrors}");
                    streamWriter.Write($"#### Language: {compilationCycleResultsModel.Language}");
                    streamWriter.Write($"#### Version: {compilationCycleResultsModel.Version}");
                    streamWriter.Write($"#### Execution Time: {compilationCycleResultsModel.ExecutionTime} secs");
                    streamWriter.Write($"#### Compilation Date: {DateTime.Now}");
                    streamWriter.WriteLine("|++++++++++++++++++++++++|\n");

                    //Log CompileCycle Results in txt
                    foreach (CompilationResultsModel compilationResultsModel in compilationCycleResultsModel.CompilationResultsModelList)
                    {
                        streamWriter.WriteLine($"#### Markdown FileName: {compilationResultsModel.MarkdownFileName}");
                        streamWriter.WriteLine($"#### IsSuccess: {compilationResultsModel.IsSuccess}");
                        streamWriter.WriteLine($"``` \n {compilationResultsModel.Snippet} \n ```");
                        streamWriter.WriteLine("|++++++++++++++++++++++++|");

                        if (compilationResultsModel.Diagnostics != null)
                        {
                            foreach (Diagnostic diagnostics in compilationResultsModel.Diagnostics)
                            {
                                streamWriter.WriteLine($"\n\t#### Error Code: {diagnostics.Id}");
                                streamWriter.WriteLine($"\t#### IsWarningAsError {diagnostics.IsWarningAsError}");
                                streamWriter.WriteLine($"\t#### WarningLevel {diagnostics.WarningLevel}");
                                streamWriter.WriteLine($"\t#### Error Message {diagnostics.GetMessage()}\n");
                            }
                        }
                    }
                }
            }
        }
    }
}
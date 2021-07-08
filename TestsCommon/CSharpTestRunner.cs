// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using MsGraphSDKSnippetsCompiler;
using MsGraphSDKSnippetsCompiler.Models;

using NUnit.Framework;

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestsCommon
{
    /// <summary>
    /// TestRunner for C# compilation tests
    /// </summary>
    public static class CSharpTestRunner
    {
        /// <summary>
        /// template to compile snippets in
        /// </summary>
        private const string SDKShellTemplate = @"using System;
using Microsoft.Graph;
using MsGraphSDKSnippetsCompiler;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

// Disambiguate colliding namespaces
using DayOfWeek = Microsoft.Graph.DayOfWeek;
using TimeOfDay = Microsoft.Graph.TimeOfDay;
using KeyValuePair = Microsoft.Graph.KeyValuePair;

public class GraphSDKTest
{
    public async Task Main(IAuthenticationProvider authProvider, IHttpProvider httpProvider)
    {
        try
        {
            //insert-code-here
        }
        catch(Exception e)
        {
            throw;
        }
    }

    public HttpRequestMessage GetRequestMessage(IAuthenticationProvider authProvider)
    {
        return null; //return-request-message
    }
}";

        /// <summary>
        /// matches csharp snippet from C# snippets markdown output
        /// </summary>
        private const string CSharpSnippetPattern = @"```csharp(.*)```";

        /// <summary>
        /// compiled version of the C# markdown regular expression
        /// uses Singleline so that (.*) matches new line characters as well
        /// </summary>
        private static readonly Regex CSharpSnippetRegex = new Regex(CSharpSnippetPattern, RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// matches result variable name from code snippets
        /// </summary>
        private const string ResultVariablePattern = "var ([@_a-zA-Z][_a-zA-Z0-9]+) = await graphClient";

        /// <summary>
        /// compiled version of the regex matching result variable name from code snippets
        /// </summary>
        internal static readonly Regex ResultVariableRegex = new Regex(ResultVariablePattern, RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// 1. Fetches snippet from docs repo
        /// 2. Asserts that there is one and only one snippet in the file
        /// 3. Wraps snippet with compilable template
        /// 4. Attempts to compile and reports errors if there is any
        /// </summary>
        /// <param name="testData">Test data containing information such as snippet file name</param>
        public static void Compile(LanguageTestData testData)
        {
            if (testData == null)
            {
                throw new ArgumentNullException(nameof(testData));
            }

            var fullPath = Path.Join(GraphDocsDirectory.GetSnippetsDirectory(testData.Version, Languages.CSharp), testData.FileName);
            Assert.IsTrue(File.Exists(fullPath), "Snippet file referenced in documentation is not found!");

            var fileContent = File.ReadAllText(fullPath);
            var (codeToCompile, codeSnippetFormatted) = GetCodeToCompile(fileContent);

            // Compile Code
            var microsoftGraphCSharpCompiler = new MicrosoftGraphCSharpCompiler(testData.FileName, testData.DllPath);
            var compilationResultsModel = microsoftGraphCSharpCompiler.CompileSnippet(codeToCompile, testData.Version);

            var compilationOutputMessage = new CompilationOutputMessage(compilationResultsModel, codeToCompile, testData.DocsLink, testData.KnownIssueMessage, testData.IsKnownIssue);
            EvaluateCompilationResult(compilationResultsModel, testData, codeSnippetFormatted, compilationOutputMessage);

            Assert.Pass();
        }

        /// <summary>
        /// 1. Fetches snippet from docs repo
        /// 2. Asserts that there is one and only one snippet in the file
        /// 3. Wraps snippet with compilable template
        /// 4. Attempts to compile and reports errors if there is any
        /// 5. It uses the compiled binary to make a request to the demo tenant and reports error if there's a service exception i.e 4XX or 5xx response
        /// </summary>
        /// <param name="executionTestData">Test data containing information such as snippet file name</param>
        public async static Task Execute(ExecutionTestData executionTestData)
        {
            if (executionTestData == null)
            {
                throw new ArgumentNullException(nameof(executionTestData));
            }

            var testData = executionTestData.LanguageTestData;

            var (codeToCompile, codeSnippetFormatted) = GetCodeToExecute(executionTestData.FileContent);

            // Compile Code
            var microsoftGraphCSharpCompiler = new MicrosoftGraphCSharpCompiler(testData.FileName, testData.DllPath);
            var executionResultsModel = await microsoftGraphCSharpCompiler.ExecuteSnippet(codeToCompile, testData.Version).ConfigureAwait(false);
            var compilationOutputMessage = new CompilationOutputMessage(
                executionResultsModel.CompilationResult,
                codeToCompile,
                testData.DocsLink,
                testData.KnownIssueMessage,
                testData.IsKnownIssue);

            EvaluateCompilationResult(executionResultsModel.CompilationResult, testData, codeSnippetFormatted, compilationOutputMessage);

            if (!executionResultsModel.Success)
            {
                Assert.Fail($"{compilationOutputMessage}{Environment.NewLine}{executionResultsModel.ExceptionMessage}");
            }

            Assert.Pass(compilationOutputMessage.ToString());
        }

        private static void EvaluateCompilationResult(CompilationResultsModel compilationResult, LanguageTestData testData, string codeSnippetFormatted, CompilationOutputMessage compilationOutputMessage)
        {
            if (!compilationResult.IsSuccess)
            {
                // environment variable for sources directory is defined only for cloud runs
                var config = AppSettings.Config();
                if (bool.Parse(config.GetSection("IsLocalRun").Value)
                    && bool.Parse(config.GetSection("GenerateLinqPadOutputInLocalRun").Value))
                {
                    WriteLinqFile(testData, codeSnippetFormatted);
                }

                Assert.Fail($"{compilationOutputMessage}");
            }
        }

        /// <summary>
        /// Modifies snippet to return HttpRequestMessage object so that we can extract the generated URL
        /// </summary>
        /// <param name="codeSnippet">code snippet</param>
        /// <returns>Code snippet that returns an HttpRequestMessage</returns>

        internal static string ReturnHttpRequestMessage(string codeSnippet)
        {
            string resultVariable = null;
            try
            {
                resultVariable = GetResultVariable(codeSnippet);
            }
            catch (Exception e)
            {
                Assert.Fail("result variable is not found!" + Environment.NewLine + e.Message);
            }

            codeSnippet = codeSnippet.Replace("await graphClient", "graphClient")
                .Replace(".GetAsync();", $@".GetHttpRequestMessage();

        return {resultVariable};");

            var intendedClosingStatement = codeSnippet.LastIndexOf($"{resultVariable};", StringComparison.OrdinalIgnoreCase);
            // Semi-Colon closing the snippet is at LastIndexOf the return variable + Length of resultVariable + 1
            var closingSemiColonIndex = intendedClosingStatement + resultVariable.Length + 1;
            // Remove all text from intended closing statement to the end. 
            codeSnippet = codeSnippet.Remove(closingSemiColonIndex);
            return codeSnippet;
        }
        private static string GetResultVariable(string codeToCompile)
        {
            string resultVariable = null;
            var resultVariableMatch = ResultVariableRegex.Match(codeToCompile);
            if (resultVariableMatch.Success)
            {
                resultVariable = resultVariableMatch.Groups[1].Value;
            }
            else
            {
                Assert.Fail("Regex {0}, against code {1} Failed", ResultVariablePattern, codeToCompile);
            }

            return resultVariable;
        }

        /// <summary>
        /// Gets code to be executed
        /// </summary>
        /// <param name="fileContent">snippet file content</param>
        /// <returns>code to be executed</returns>
        private static (string, string) GetCodeToExecute(string fileContent)
        {
            var (codeToCompile, codeSnippetFormatted) = GetCodeToCompile(IdentifierReplacer.Instance.ReplaceIds(fileContent));

            // have another transformation to insert GetRequestMessage method
            codeToCompile = codeToCompile.Replace("GraphServiceClient( authProvider );", "GraphServiceClient( authProvider, httpProvider );");
            codeToCompile = codeToCompile.Replace("return null; //return-request-message", "//insert-code-here");
            codeToCompile = BaseTestRunner.ConcatBaseTemplateWithSnippet(ReturnHttpRequestMessage(codeSnippetFormatted), codeToCompile);
            return (codeToCompile, codeSnippetFormatted);
        }

        /// <summary>
        /// Gets code to be compiled
        /// </summary>
        /// <param name="fileContent">snippet file content</param>
        /// <returns>code to be compiled</returns>
        private static (string, string) GetCodeToCompile(string fileContent)
        {
            var match = CSharpSnippetRegex.Match(fileContent);
            Assert.IsTrue(match.Success, "Csharp snippet file is not in expected format!");

            var codeSnippetFormatted = match.Groups[1].Value
                .Replace("\r\n", "\r\n        ")            // add indentation to match with the template
                .Replace("\r\n        \r\n", "\r\n\r\n")    // remove indentation added to empty lines
                .Replace("\t", "    ");                     // do not use tabs

            while (codeSnippetFormatted.Contains("\r\n\r\n"))
            {
                codeSnippetFormatted = codeSnippetFormatted.Replace("\r\n\r\n", "\r\n"); // do not have empty lines for shorter error messages
            }

            var codeToCompile = BaseTestRunner.ConcatBaseTemplateWithSnippet(codeSnippetFormatted, SDKShellTemplate);

            return (codeToCompile, codeSnippetFormatted);
        }

        /// <summary>
        /// Generates .linq file in default My Queries folder so that the results are visible in LinqPad right away
        /// </summary>
        /// <param name="testData">test data</param>
        /// <param name="codeSnippetFormatted">code snippet</param>
        private static void WriteLinqFile(LanguageTestData testData, string codeSnippetFormatted)
        {
            var linqPadQueriesDefaultFolder = Path.Join(
                    Environment.GetEnvironmentVariable("USERPROFILE"),
                    "/OneDrive - Microsoft", // remove this if you are not syncing your Documents to OneDrive
                    "/Documents",
                    "/LINQPad Queries");

            var linqDirectory = Path.Join(
                    linqPadQueriesDefaultFolder,
                    "/RaptorResults",
                    (testData.Version, testData.IsKnownIssue) switch
                    {
                        (Versions.Beta, false) => "/Beta",
                        (Versions.Beta, true) => "/BetaKnown",
                        (Versions.V1, false) => "/V1",
                        (Versions.V1, true) => "/V1Known",
                        _ => throw new ArgumentException("unsupported version", nameof(testData))
                    });

            Directory.CreateDirectory(linqDirectory);

            var linqFilePath = Path.Join(linqDirectory, testData.FileName.Replace(".md", ".linq"));

            const string LinqTemplateStart = "<Query Kind=\"Statements\">";
            string LinqTemplateEnd =
@$"
  <Namespace>DayOfWeek = Microsoft.Graph.DayOfWeek</Namespace>
  <Namespace>KeyValuePair = Microsoft.Graph.KeyValuePair</Namespace>
  <Namespace>Microsoft.Graph</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>TimeOfDay = Microsoft.Graph.TimeOfDay</Namespace>
</Query>
// {testData.DocsLink}{(testData.IsKnownIssue ? (Environment.NewLine + "/* " + testData.KnownIssueMessage + " */") : string.Empty)}
IAuthenticationProvider authProvider  = null;";

            File.WriteAllText(linqFilePath,
                LinqTemplateStart
                + Environment.NewLine
                + (testData.Version) switch
                {
                    Versions.Beta => "  <NuGetReference Prerelease=\"true\">Microsoft.Graph.Beta</NuGetReference>",
                    Versions.V1 => "  <NuGetReference>Microsoft.Graph</NuGetReference>",
                    _ => throw new ArgumentException("unsupported version", nameof(testData))
                }
                + LinqTemplateEnd
                + codeSnippetFormatted.Replace("\n        ", "\n"));
        }
    }
}

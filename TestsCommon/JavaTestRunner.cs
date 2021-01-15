using MsGraphSDKSnippetsCompiler;
using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace TestsCommon
{
    public static class JavaTestRunner
    {
        private const string importsCurrent = @"import com.microsoft.graph.authentication.IAuthenticationProvider;
import com.microsoft.graph.models.extensions.IGraphServiceClient;
import com.microsoft.graph.requests.extensions.GraphServiceClient;";
        private const string importsVNext = @"import com.microsoft.graph.httpcore.*;
import com.microsoft.graph.core.IGraphServiceClient;
import com.microsoft.graph.core.GraphServiceClient;";
        /// <summary>
        /// template to compile snippets in
        /// </summary>
        private const string SDKShellTemplate = @"package com.microsoft.graph.raptor;
--imports--
import com.microsoft.graph.http.IHttpRequest;
import java.util.*;
import java.time.*;
import java.io.InputStream;
import javax.xml.datatype.DatatypeFactory;
import javax.xml.datatype.Duration;
import com.google.gson.JsonPrimitive;
import com.google.gson.JsonParser;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import okhttp3.Request;
import com.microsoft.graph.core.*;
import com.microsoft.graph.models.extensions.*;
import com.microsoft.graph.requests.extensions.*;
import com.microsoft.graph.models.generated.*;
import com.microsoft.graph.options.*;
import com.microsoft.graph.serializer.*;
public class App
{
    public static void main(String[] args) throws Exception
    {
--auth--
        //insert-code-here
    }
}";
        private const string authProviderCurrent = @"        final IAuthenticationProvider authProvider = new IAuthenticationProvider() {
            @Override
            public void authenticateRequest(IHttpRequest request) {
            }
        };";
        private const string authProvidervNext = @"        final ICoreAuthenticationProvider authProvider = new ICoreAuthenticationProvider() {
            @Override
            public Request authenticateRequest(Request request) {
                return request;
            }
        };";
        /// <summary>
        /// matches csharp snippet from C# snippets markdown output
        /// </summary>
        private const string Pattern = @"```java(.*)```";

        /// <summary>
        /// compiled version of the C# markdown regular expression
        /// uses Singleline so that (.*) matches new line characters as well
        /// </summary>
        private static readonly Regex RegExp = new Regex(Pattern, RegexOptions.Singleline | RegexOptions.Compiled);


        /// <summary>
        /// 1. Fetches snippet from docs repo
        /// 2. Asserts that there is one and only one snippet in the file
        /// 3. Wraps snippet with compilable template
        /// 4. Attempts to compile and reports errors if there is any
        /// </summary>
        /// <param name="testData">Test data containing information such as snippet file name</param>
        public static void Run(LanguageTestData testData)
        {
            if (testData == null)
            {
                throw new ArgumentNullException(nameof(testData));
            }

            var fullPath = Path.Join(GraphDocsDirectory.GetSnippetsDirectory(testData.Version, Languages.Java), testData.FileName);
            Assert.IsTrue(File.Exists(fullPath), "Snippet file referenced in documentation is not found!");

            var fileContent = File.ReadAllText(fullPath);
            var match = RegExp.Match(fileContent);
            Assert.IsTrue(match.Success, "Java snippet file is not in expected format!");

            var codeSnippetFormatted = match.Groups[1].Value
                .Replace("\r\n", "\r\n        ")            // add indentation to match with the template
                .Replace("\r\n        \r\n", "\r\n\r\n")    // remove indentation added to empty lines
                .Replace("\t", "    ")                      // do not use tabs
                .Replace("\r\n\r\n\r\n", "\r\n\r\n");       // do not have two consecutive empty lines
            var isCurrentSdk = string.IsNullOrEmpty(testData.JavaPreviewLibPath);
            var codeToCompile = BaseTestRunner.ConcatBaseTemplateWithSnippet(codeSnippetFormatted, SDKShellTemplate
                                                                            .Replace("--auth--",  isCurrentSdk ? authProviderCurrent: authProvidervNext)
                                                                            .Replace("--imports--", isCurrentSdk ? importsCurrent: importsVNext));

            // Compile Code
            var microsoftGraphCSharpCompiler = new MicrosoftGraphJavaCompiler(testData.FileName, testData.JavaPreviewLibPath, testData.JavaLibVersion, testData.JavaCoreVersion);

            var jvmRetryAttmptsLeft = 3;
            while (jvmRetryAttmptsLeft > 0)
            {
                var compilationResultsModel = microsoftGraphCSharpCompiler.CompileSnippet(codeToCompile, testData.Version);

                if (compilationResultsModel.IsSuccess)
                {
                    Assert.Pass();
                } 
                else if(compilationResultsModel.Diagnostics.Any(x => x.GetMessage().Contains("Starting a Gradle Daemon")))
                {//the JVM takes time to start making the first test to be run to be flaky, this is a workaround
                    jvmRetryAttmptsLeft--;
                    Thread.Sleep(20000);
                    continue;
                }

                var compilationOutputMessage = new CompilationOutputMessage(compilationResultsModel, codeToCompile, testData.DocsLink, testData.KnownIssueMessage, testData.IsKnownIssue);

                Assert.Fail($"{compilationOutputMessage}");
                break;
            }
        }
    }
}

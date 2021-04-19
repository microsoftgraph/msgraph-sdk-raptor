﻿using MsGraphSDKSnippetsCompiler;
using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace TestsCommon
{
    public static class JavaTestRunner
    {
        /// <summary>
        /// template to compile snippets in
        /// </summary>
        private const string SDKShellTemplate = @"package com.microsoft.graph.raptor;
import com.microsoft.graph.httpcore.*;
import com.microsoft.graph.requests.*;
import com.microsoft.graph.models.*;
import com.microsoft.graph.http.IHttpRequest;
import java.util.LinkedList;
import java.time.OffsetDateTime;
import java.io.InputStream;
import java.net.URL;
import java.util.UUID;
import java.util.Base64;
import java.util.EnumSet;
import javax.xml.datatype.DatatypeFactory;
import javax.xml.datatype.Duration;
import com.google.gson.JsonPrimitive;
import com.google.gson.JsonParser;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import java.util.concurrent.CompletableFuture;
import okhttp3.Request;
import com.microsoft.graph.core.*;
import com.microsoft.graph.options.*;
import com.microsoft.graph.serializer.*;
import com.microsoft.graph.authentication.*;
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
            public CompletableFuture<String> getAuthorizationTokenAsync(final URL requestUrl) {
                return CompletableFuture.completedFuture("""");
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
                                                                            .Replace("--auth--", authProviderCurrent));

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

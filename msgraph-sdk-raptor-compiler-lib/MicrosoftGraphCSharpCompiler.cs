﻿extern alias beta;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using MsGraphSDKSnippetsCompiler.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.Loader;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Security;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Net.Http.Headers;

namespace MsGraphSDKSnippetsCompiler
{
    /// <summary>
    ///     Microsoft Graph SDK CSharp snippets compiler class
    /// </summary>
    public class MicrosoftGraphCSharpCompiler : IMicrosoftGraphSnippetsCompiler
    {
        private readonly string _markdownFileName;
        private readonly string _dllPath;
        private readonly RaptorConfig _config;
        private readonly IPublicClientApplication _publicClientApplication;
        private readonly IConfidentialClientApplication _confidentialClientApp;

        /// for hiding bearer token
        private const string AuthHeaderPattern = "Authorization: Bearer .*";
        private const string AuthHeaderReplacement = "Authorization: Bearer <token>";
        private static readonly Regex AuthHeaderRegex = new Regex(AuthHeaderPattern, RegexOptions.Compiled);

        private static readonly ConcurrentDictionary<string, IAccount> TokenCache = new();

        private const string DefaultAuthScope = "https://graph.microsoft.com/.default";

        public MicrosoftGraphCSharpCompiler(string markdownFileName,
            string dllPath,
            RaptorConfig config,
            IPublicClientApplication publicClientApplication,
            IConfidentialClientApplication confidentialClientApp)
        {
            _markdownFileName = markdownFileName;
            _dllPath = dllPath;
            _config = config;
            _publicClientApplication = publicClientApplication;
            _confidentialClientApp = confidentialClientApp;
        }
        public MicrosoftGraphCSharpCompiler(string markdownFileName,
            string dllPath)
        {
            _markdownFileName = markdownFileName;
            _dllPath = dllPath;
        }

        /// <summary>
        ///     Returns CompilationResultsModel which has the results status and the compilation diagnostics.
        /// </summary>
        /// <param name="codeSnippet">The code snippet to be compiled.</param>
        /// <param name="version"></param>
        /// <returns>CompilationResultsModel</returns>
        private (CompilationResultsModel, Assembly) CompileSnippetAndGetAssembly(string codeSnippet, Versions version)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeSnippet);

            string assemblyName = Path.GetRandomFileName();
            string commonAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            string graphAssemblyPathV1 = Path.GetDirectoryName(typeof(GraphServiceClient).Assembly.Location);
            string graphAssemblyPathBeta = Path.GetDirectoryName(typeof(beta.Microsoft.Graph.GraphServiceClient).Assembly.Location);

            List<MetadataReference> metadataReferences = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "System.Private.CoreLib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "System.Console.dll")),
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(IAuthenticationProvider).Assembly.Location), "Microsoft.Graph.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(AuthenticationProvider).Assembly.Location), "msgraph-sdk-raptor-compiler-lib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(Task).Assembly.Location), "System.Threading.Tasks.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(JToken).Assembly.Location), "Newtonsoft.Json.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(HttpClient).Assembly.Location), "System.Net.Http.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(Expression).Assembly.Location), "System.Linq.Expressions.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(Uri).Assembly.Location), "System.Private.Uri.dll"))
            };

            //Use the right Microsoft Graph Version
            if (!string.IsNullOrEmpty(_dllPath))
            {
                if (!System.IO.File.Exists(_dllPath))
                {
                    throw new ArgumentException($"Provided dll path {_dllPath} doesn't exist!");
                }

                metadataReferences.Add(MetadataReference.CreateFromFile(_dllPath));
            }
            else if (version == Versions.V1)
            {
                metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(graphAssemblyPathV1, "Microsoft.Graph.dll")));
            }
            else
            {
                metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(graphAssemblyPathBeta, "Microsoft.Graph.Beta.dll")));
            }

            var compilation = CSharpCompilation.Create(
               assemblyName,
               syntaxTrees: new[] { syntaxTree },
               references: metadataReferences,
               options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                                .WithOptimizationLevel(OptimizationLevel.Release));

            var (emitResult, assembly) = GetEmitResult(compilation);
            CompilationResultsModel results = GetCompilationResults(emitResult);

            return (results, assembly);
        }

        public CompilationResultsModel CompileSnippet(string codeSnippet, Versions version)
        {
            var (results, _) = CompileSnippetAndGetAssembly(codeSnippet, version);
            return results;
        }

        /// <summary>
        ///     Returns CompilationResultsModel which has the results status and the compilation diagnostics.
        /// </summary>
        /// <param name="codeSnippet">The code snippet to be compiled.</param>
        /// <returns>CompilationResultsModel</returns>
        public async Task<ExecutionResultsModel> ExecuteSnippet(string codeSnippet, Versions version)
        {
            var (compilationResult, assembly) = CompileSnippetAndGetAssembly(codeSnippet, version);

            string exceptionMessage = null;
            bool success = false;
            if (compilationResult.IsSuccess)
            {
                try
                {
                    var requiresDelegatedPermissions = RequiresDelegatedPermissions(codeSnippet);
                    dynamic instance = assembly.CreateInstance("GraphSDKTest");
                    IAuthenticationProvider authProvider;

                    if (requiresDelegatedPermissions)
                    {
                        // delegated permissions
                        using var httpRequestMessage = instance.GetRequestMessage(null);
                        var scopes = await GetScopes(httpRequestMessage);
                        authProvider = new DelegateAuthenticationProvider(async request =>
                        {
                            var token = await GetATokenForGraph(scopes);
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        });
                    }
                    else
                    {
                        authProvider = new ClientCredentialProvider(_confidentialClientApp, DefaultAuthScope);
                    }
                    // Pass custom http provider to provide interception and logging
                    await (instance.Main(authProvider, new CustomHttpProvider()) as Task);
                    success = true;
                }
                catch (Exception e)
                {
                    var innerExceptionMessage = e.InnerException?.Message ?? string.Empty;
                    exceptionMessage = e.Message + Environment.NewLine + innerExceptionMessage;
                    if (!_config.IsLocalRun)
                    {
                        exceptionMessage = AuthHeaderRegex.Replace(exceptionMessage, AuthHeaderReplacement);
                    }
                }
            }

            return new ExecutionResultsModel(compilationResult, success, exceptionMessage);
        }

        /// <summary>
        /// Calls DevX Api to get required permissions
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <returns></returns>
        static async Task<string[]> GetScopes(HttpRequestMessage httpRequestMessage)
        {
            var path = httpRequestMessage.RequestUri.LocalPath;
            var versionSegmentLength = "/v1.0".Length;
            if (path.StartsWith("/v1.0") || path.StartsWith("/beta"))
            {
                path = path[versionSegmentLength..];
            }

            using var httpClient = new HttpClient();

            using var scopesRequest = new HttpRequestMessage(HttpMethod.Get, $"https://graphexplorerapi.azurewebsites.net/permissions?requesturl={path}&method={httpRequestMessage.Method}");
            scopesRequest.Headers.Add("Accept-Language", "en-US");

            try
            {
                using var response = await httpClient.SendAsync(scopesRequest).ConfigureAwait(false);
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var scopes = JsonSerializer.Deserialize<Scope[]>(responseString);
                var fullReadScopes = scopes
                    .ToList()
                    .Where(x => x.value.Contains("Read") && !x.value.Contains("Write"))
                    .Select(x => $"https://graph.microsoft.com/{ x.value }")
                    .ToArray();

                return fullReadScopes.Length == 0 ? new[] { DefaultAuthScope } : fullReadScopes;
            }
            catch (Exception)
            {
                // some URLs don't return scopes from the permissions endpoint of DevX API
                return new[] { DefaultAuthScope };
            }
        }

        /// <summary>
        /// Acquires a token for given context
        /// </summary>
        /// <param name="scopes">requested scopes in the token</param>
        /// <returns>token for the given context</returns>
        private async Task<string> GetATokenForGraph(string[] scopes)
        {
            IAccount account = null;
            if (TokenCache.ContainsKey(_config.Username))
            {
                account = TokenCache[_config.Username];
            }
            using var securePassword = new SecureString();
            // convert plain password into a secure string.
            _config.Password.ToList().ForEach(c => securePassword.AppendChar(c));

            try
            {
                AuthenticationResult authResult;
                if (account == null)
                {
                    authResult = await _publicClientApplication
                        .AcquireTokenByUsernamePassword(scopes, _config.Username, securePassword)
                        .ExecuteAsync();
                }
                else
                {
                    authResult = await _publicClientApplication.AcquireTokenSilent(scopes, account).ExecuteAsync();
                }

                TokenCache[_config.Username] = authResult.Account;
                return authResult.AccessToken;
            }
            catch (Exception e)
            {
                var prefixLength = "https://graph.microsoft.com/".Length;
                var scopeShortNames = scopes.Select(s => s[prefixLength..]).ToArray();
                throw new AggregateException("scopes: " + string.Join(", ", scopeShortNames), e);
            }
        }


        /// <summary>
        ///     Gets the result of the Compilation.Emit method.
        /// </summary>
        /// <param name="compilation">Immutable representation of a single invocation of the compiler</param>
        private (EmitResult, Assembly) GetEmitResult(CSharpCompilation compilation)
        {
            Assembly assembly = null;

            using MemoryStream memoryStream = new MemoryStream();
            EmitResult emitResult = compilation.Emit(memoryStream);

            if (emitResult.Success)
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                assembly = AssemblyLoadContext.Default.LoadFromStream(memoryStream);
            }
            return (emitResult, assembly);
        }

        /// <summary>
        ///     Checks whether the EmitResult is successful and returns an instance of CompilationResultsModel.
        /// </summary>
        /// <param name="emitResult">The result of the Compilation.Emit method.</param>
        private CompilationResultsModel GetCompilationResults(EmitResult emitResult)
        {
            // We are only interested with warnings and errors hence the diagnostics filter
            var failures = emitResult.Success
                ? null
                : emitResult.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

            return new CompilationResultsModel(emitResult.Success, failures, _markdownFileName);
        }

        /// <summary>
        /// Determines whether code snippet requires delegated permissions (as opposed to application permissions)
        /// </summary>
        /// <param name="codeSnippet">code snippet</param>
        /// <returns>true if the snippet requires delegated permissions</returns>
        internal static bool RequiresDelegatedPermissions(string codeSnippet)
        {
            // TODO: https://github.com/microsoftgraph/msgraph-sdk-raptor/issues/164
            const string graphClient = "graphClient.";
            var apiPathStart = codeSnippet.IndexOf(graphClient, StringComparison.Ordinal) + graphClient.Length;
            var apiPath = codeSnippet[apiPathStart..];

            const RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.Singleline;
            var apisWithDelegatedPermissions = new[]
            {
                new Regex(@"^Me", regexOptions),
                new Regex(@"^Education.Me", regexOptions),
                new Regex(@"^Users\[",regexOptions),
                new Regex(@"^Planner",regexOptions),
                new Regex(@"^Print",regexOptions),
                new Regex(@"^IdentityProviders",regexOptions),
                new Regex(@"^Reports",regexOptions),
                new Regex(@"^IdentityGovernance",regexOptions),
                new Regex(@"^GroupSetting",regexOptions),
                new Regex(@"^Teams\[[^\]]*\]\.Schedule",regexOptions),
                new Regex(@"^Teamwork.WorkforceIntegrations",regexOptions),
                new Regex(@"^Communications.Presences\[[^\]]*\]",regexOptions)
            };

            var matchResult = apisWithDelegatedPermissions.Any(x => x.IsMatch(apiPath));
            return matchResult;
        }
    }
}

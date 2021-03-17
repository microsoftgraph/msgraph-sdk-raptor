extern alias beta;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using MsGraphSDKSnippetsCompiler.Models;
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
using Microsoft.Extensions.Configuration;

namespace MsGraphSDKSnippetsCompiler
{
    /// <summary>
    ///     Microsoft Graph SDK CSharp snippets compiler class
    /// </summary>
    public class MicrosoftGraphCSharpCompiler : IMicrosoftGraphSnippetsCompiler
    {
        private readonly string _markdownFileName;
        private readonly string _dllPath;

        /// for hiding bearer token
        private const string AuthHeaderPattern = "Authorization: Bearer .*";
        private const string AuthHeaderReplacement = "Authorization: Bearer <token>";
        private static readonly Regex AuthHeaderRegex = new Regex(AuthHeaderPattern, RegexOptions.Compiled);

        private const string DefaultAuthScope = "https://graph.microsoft.com/.default";

        public MicrosoftGraphCSharpCompiler(string markdownFileName, string dllPath)
        {
            _markdownFileName = markdownFileName;
            _dllPath = dllPath;
        }

        /// <summary>
        ///     Returns CompilationResultsModel which has the results status and the compilation diagnostics.
        /// </summary>
        /// <param name="codeSnippet">The code snippet to be compiled.</param>
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
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "System.Text.Json.dll")),
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "System.Memory.dll")),
                MetadataReference.CreateFromFile(Path.Combine(commonAssemblyPath, "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(IAuthenticationProvider).Assembly.Location), "Microsoft.Graph.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(AuthenticationProvider).Assembly.Location), "msgraph-sdk-raptor-compiler-lib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(Task).Assembly.Location), "System.Threading.Tasks.dll")),
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
                    var config = AppSettings.Config();
                    var clientId = GetNonEmptyValue(config, "ClientID");

                    dynamic instance = assembly.CreateInstance("GraphSDKTest");
                    IAuthenticationProvider authProvider;

                    if (requiresDelegatedPermissions)
                    {
                        // delegated permissions
                        using var httpRequestMessage = instance.GetRequestMessage(null);
                        var scopes = await GetScopes(httpRequestMessage);
                        var authority = GetNonEmptyValue(config, "Authority");
                        var username = GetNonEmptyValue(config, "Username");
                        var password = GetNonEmptyValue(config, "Password");
                        var token = await GetATokenForGraph(clientId, authority, username, password, scopes).ConfigureAwait(false);
                        authProvider = new DelegateAuthenticationProvider(async request =>
                        {
                            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                        });
                    }
                    else
                    {
                        // application permissions
                        var tenantId = GetNonEmptyValue(config, "TenantID");
                        var clientSecret = GetNonEmptyValue(config, "ClientSecret");
                        IConfidentialClientApplication confidentialClientApp = ConfidentialClientApplicationBuilder
                            .Create(clientId)
                            .WithTenantId(tenantId)
                            .WithClientSecret(clientSecret)
                            .Build();
                        authProvider = new ClientCredentialProvider(confidentialClientApp, DefaultAuthScope);
                    }

                    await (instance.Main(authProvider) as Task);
                    success = true;
                }
                catch (Exception e)
                {
                    var innerExceptionMessage = e.InnerException?.Message ?? string.Empty;
                    exceptionMessage = e.Message + Environment.NewLine + innerExceptionMessage;
                    if (!bool.Parse(AppSettings.Config().GetSection("IsLocalRun").Value))
                    {
                        exceptionMessage = AuthHeaderRegex.Replace(exceptionMessage, AuthHeaderReplacement);
                    }
                }
            }

            return new ExecutionResultsModel(compilationResult, success, exceptionMessage);
        }

        /// <summary>
        /// Calls DevX Api to get required premissions
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

            using var response = await httpClient.SendAsync(scopesRequest).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var scopes = JsonSerializer.Deserialize<Scope[]>(responseString);

            return scopes
                .ToList()
                .Where(x => x.value.Contains("Read") && !x.value.Contains("Write"))
                .Select(x => $"https://graph.microsoft.com/{ x.value }")
                .ToArray();
        }

        /// <summary>
        /// Acquires a token for given context
        /// </summary>
        /// <param name="clientId">Client ID of the application</param>
        /// <param name="authority">token authority, such as: https://login.microsoftonline.com/contoso.onmicrosoft.com</param>
        /// <param name="username">username of the user for which the token is requested</param>
        /// <param name="password">password of the user for which the token is requested</param>
        /// <param name="scopes">requested scopes in the token</param>
        /// <returns>token for the given context</returns>
        static async Task<string> GetATokenForGraph(string clientId, string authority, string username, string password, string[] scopes)
        {
            var app = PublicClientApplicationBuilder.Create(clientId).WithAuthority(authority).Build();

            using var securePassword = new SecureString();

            // convert plain password into a secure string.
            password.ToList().ForEach(c => securePassword.AppendChar(c));

            try
            {
                var result = await app.AcquireTokenByUsernamePassword(scopes, username, securePassword).ExecuteAsync();
                return result.AccessToken;
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
        /// <param name="compilation">Immutable respresentation of a single invocation of the compiler</param>
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
        ///     Checks whether the EmitResult is successfull and returns an instance of CompilationResultsModel.
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
        private static bool RequiresDelegatedPermissions(string codeSnippet)
        {
            // TODO: https://github.com/microsoftgraph/msgraph-sdk-raptor/issues/164
            return codeSnippet.Contains("graphClient.Me") ||
                codeSnippet.Contains("graphClient.Education.Me") ||
                codeSnippet.Contains("graphClient.Users[\"");
        }

        /// <summary>
        /// Extracts the configuration value, throws if empty string
        /// </summary>
        /// <param name="config">configuration</param>
        /// <param name="key">lookup key</param>
        /// <returns>non-empty configuration value if found</returns>
        private static string GetNonEmptyValue(IConfigurationRoot config, string key)
        {
            var value = config.GetSection(key).Value;
            if (value == string.Empty)
            {
                throw new Exception($"Value for {key} is not found in appsettings.json");
            }

            return value;
        }
    }
}

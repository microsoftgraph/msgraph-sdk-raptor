extern alias beta;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Graph;
using MsGraphSDKSnippetsCompiler.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace MsGraphSDKSnippetsCompiler
{
    /// <summary>
    ///     Microsoft Graph SDK CSharp snippets compiler class
    /// </summary>
    public class MicrosoftGraphCSharpCompiler : IMicrosoftGraphSnippetsCompiler
    {
        private readonly string _markdownFileName;
        private readonly string _dllPath;

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
        public CompilationResultsModel CompileSnippet(string codeSnippet, Versions version)
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
            };

            //Use the right Microsoft Graph Version
            if (_dllPath != null && _dllPath != string.Empty)
            {
                if (!System.IO.File.Exists(_dllPath))
                {
                    throw new ArgumentException($"Provided dll path {_dllPath} doesn't exist!");
                }

                metadataReferences.Add(MetadataReference.CreateFromFile(_dllPath));
            }
            else if(version == Versions.V1)
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
               options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var memoryStream = new MemoryStream();
            var emitResult = compilation.Emit(memoryStream);
            CompilationResultsModel results = GetCompilationResults(emitResult);

            return results;
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
    }
}
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NUnit.Framework;
using TestsCommon;
using MsGraphSDKSnippetsCompiler.Models;

namespace MyParallel
{
    class Program
    {
        static void Main(string[] args)
        {
            var testnames = System.IO.File.ReadAllLines(@"/Users/muzengin/repos/msgraph-sdk-raptor/testnames.txt");
            ExecutionTestData[] executionTestData = new ExecutionTestData[testnames.Length];
            var i = 0;
            foreach (var testname in testnames)
            {
                var fileName = testname.Replace("-V1-compiles", "-snippets.md");
                var filePath = System.IO.Path.Combine("//Users/muzengin/repos/microsoft-graph-docs/api-reference/v1.0/includes/snippets/csharp/", fileName);
                var fileContent = System.IO.File.ReadAllText(filePath);
                executionTestData[i++] = new ExecutionTestData(
                    new LanguageTestData(
                            Versions.V1,
                            false,
                            string.Empty,
                            "",
                            fileName,
                            null,
                            null,
                            null,
                            null,
                            testname,
                            "SDK"
                        ),
                        fileContent
                    );

                Console.WriteLine(filePath);
            }

            var successResults = new ConcurrentBag<string>();
            var failResults = new ConcurrentBag<string>();

            var result = Parallel.For(0, testnames.Length, (i, state) =>
            {
                try
                {
                    var task = CSharpTestRunner.Execute(executionTestData[i]);
                    task.Wait();
                }
                catch (AggregateException e)
                {
                    if (e.InnerException.GetType() == typeof(SuccessException))
                    {
                        successResults.Add(executionTestData[i].LanguageTestData.TestName);
                        Console.WriteLine("Execution Successful!");
                    }
                    else
                    {
                        failResults.Add(executionTestData[i].LanguageTestData.TestName);
                        Console.Error.WriteLine("Execution Not successful!");
                    }
                }
                catch (Exception e)
                {
                    failResults.Add(executionTestData[i].LanguageTestData.TestName);
                }
            });

            System.IO.File.WriteAllLines("/Users/muzengin/repos/msgraph-sdk-raptor/success.txt", successResults.ToArray());
            System.IO.File.WriteAllLines("/Users/muzengin/repos/msgraph-sdk-raptor/fail.txt", failResults.ToArray());

        }
    }
}
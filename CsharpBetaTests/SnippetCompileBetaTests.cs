using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System.Collections.Generic;
using TestsCommon;

namespace CsharpBetaTests
{
    [TestFixture]
    public class SnippetCompileBetaTests
    {
        public static IEnumerable<TestCaseData> TestDataBeta => TestDataGenerator.GetTestCaseData(Versions.Beta);

        [Test]
        [TestCaseSource(typeof(SnippetCompileBetaTests), nameof(TestDataBeta))]
        public void Test(string fileName, Versions version)
        {
            TestRunner.Run(fileName, version);
        }
    }
}
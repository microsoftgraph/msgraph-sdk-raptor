using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System.Collections.Generic;
using TestsCommon;

namespace CsharpV1Tests
{
    [TestFixture]
    public class SnippetCompileV1Tests
    {
        public static IEnumerable<TestCaseData> TestDataV1 => TestDataGenerator.GetTestCaseData(Versions.V1);

        [Test]
        [TestCaseSource(typeof(SnippetCompileV1Tests), nameof(TestDataV1))]
        public void Test(string fileName, Versions version)
        {
            TestRunner.Run(fileName, version);
        }
    }
}
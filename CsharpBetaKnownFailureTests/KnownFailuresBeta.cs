// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System.Collections.Generic;
using TestsCommon;

namespace CsharpBetaKnownFailureTests
{
    [TestFixture]
    public class KnownFailuresBeta
    {
        /// <summary>
        /// Gets TestCaseData for Beta known failures
        /// TestCaseData contains snippet file name, version and test case name
        /// </summary>
        public static IEnumerable<TestCaseData> TestDataBeta => TestDataGenerator.GetTestCaseData(Versions.Beta, Languages.CSharp, knownFailuresRequested: true);

        /// <summary>
        /// Represents test runs generated from test case data
        /// </summary>
        /// <param name="fileName">snippet file name in docs repo</param>
        /// <param name="docsLink">documentation page where the snippet is shown</param>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        [Test]
        [TestCaseSource(typeof(KnownFailuresBeta), nameof(TestDataBeta))]
        public void Test(LanguageTestData testData)
        {
            CSharpTestRunner.Run(testData);
        }
    }
}
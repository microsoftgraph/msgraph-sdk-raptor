// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System.Collections.Generic;
using TestsCommon;

namespace CsharpV1KnownFailureTests
{
    [TestFixture]
    public class KnownFailuresV1
    {
        /// <summary>
        /// Gets TestCaseData for V1 known failures
        /// TestCaseData contains snippet file name, version and test case name
        /// </summary>
        public static IEnumerable<TestCaseData> TestDataV1 => TestDataGenerator.GetTestCaseData(
            new RunSettings
            {
                Version = Versions.V1,
                KnownFailuresRequested = true
            });

        /// <summary>
        /// Represents test runs generated from test case data
        /// </summary>
        /// <param name="fileName">snippet file name in docs repo</param>
        /// <param name="docsLink">documentation page where the snippet is shown</param>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        [Test]
        [TestCaseSource(typeof(KnownFailuresV1), nameof(TestDataV1))]
        public void Test(CsharpTestData testData)
        {
            CSharpTestRunner.Run(testData);
        }
    }
}
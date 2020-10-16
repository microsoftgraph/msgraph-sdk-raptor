// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.

using MsGraphSDKSnippetsCompiler.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TestsCommon;

namespace CSharpArbitraryDllTests
{
    [TestFixture]
    public class SnippetCompileArbitraryDllTests
    {
        /// <summary>
        /// Gets TestCaseData for version specified in Test.runsettings
        /// TestCaseData contains snippet file name, version and test case name
        /// </summary>
        public static IEnumerable<TestCaseData> ArbitraryDllTestData => TestDataGenerator.GetTestCaseData(new RunSettings(TestContext.Parameters));

        /// <summary>
        /// Represents test runs generated from test case data
        /// </summary>
        /// <param name="fileName">snippet file name in docs repo</param>
        /// <param name="docsLink">documentation page where the snippet is shown</param>
        /// <param name="version">Docs version (e.g. V1, Beta)</param>
        [Test]
        [TestCaseSource(typeof(SnippetCompileArbitraryDllTests), nameof(ArbitraryDllTestData))]
        public void Test(CsharpTestData testData)
        {
            CSharpTestRunner.Run(testData);
        }
    }
}
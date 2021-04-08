using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace TestsCommon.Tests
{
    public class IDTreeTests
    {
        [TestCaseSource(typeof(IDTreeTestCases), nameof(IDTreeTestCases.EqualTestCases))]
        public void Equal(IDTree a, IDTree b) => Assert.IsTrue(a.Equals(b));

        [TestCaseSource(typeof(IDTreeTestCases), nameof(IDTreeTestCases.NotEqualTestCases))]
        public void NotEqual(IDTree a, IDTree b) => Assert.IsFalse(a.Equals(b));

        [TestCaseSource(typeof(IDTreeTestCases), nameof(IDTreeTestCases.LosslessSerializationTestCases))]
        public void LosslessSerialization(IDTree a)
        {
            a.Equals(JsonSerializer.Deserialize<IDTree>(JsonSerializer.Serialize(a)));
        }

        [TestCaseSource(typeof(IDTreeTestCases), nameof(IDTreeTestCases.SerializationTestCases))]
        public void Serialization(IDTree a, string json)
        {
            var actual = JsonSerializer.Serialize(a);
            actual = Regex.Replace(actual, @"\s", string.Empty);

            var expected = Regex.Replace(json, @"\s", string.Empty);
            Assert.AreEqual(expected, actual);
        }
    }

    public static class IDTreeTestCases
    {
        private static readonly IDTree NullTree = null;
        private static readonly IDTree EmptyTree = new IDTree(null);
        private static readonly IDTree EmptyTree2 = new IDTree(null);

        private static readonly IDTree OneItemTree = new IDTree(null)
        {
            ["application"] = new IDTree("(application)")
        };

        private static readonly IDTree OneItemTree2 = new IDTree(null)
        {
            ["application"] = new IDTree("(application)")
        };

        private static readonly IDTree TwoItemTree = new IDTree(null)
        {
            ["application"] = new IDTree("(application)"),
            ["team"] = new IDTree("(team)")
        };

        private static readonly IDTree DepthTwoTree = new IDTree(null)
        {
            ["application"] = new IDTree("(application)")
            {
                ["child"] = new IDTree("(application_child)")
            }
        };

        private static readonly IDTree DepthTwoTree2 = new IDTree(null)
        {
            ["application"] = new IDTree("(application)")
            {
                ["child"] = new IDTree("(application_child)")
            }
        };

        private static readonly IDTree OneItemTreeValueDifferent = new IDTree(null)
        {
            ["application"] = new IDTree("(application2)")
        };

        private static readonly IDTree OneItemTreeKeyDifferent = new IDTree(null)
        {
            ["application2"] = new IDTree("(application)")
        };

        private static readonly IDTree ComplexTree = new IDTree(null)
        {
            ["application"] = new IDTree("(application)"),
            ["team"] = new IDTree("(team)")
            {
                ["channel"] = new IDTree("(team_channel)")
                {
                    ["conversationMember"] = new IDTree("(team_channel_conversationMember)")
                },
                ["conversationMember"] = new IDTree("(team_conversationMember)")
            },
            ["chat"] = new IDTree("(chat)")
            {
                ["conversationMember"] = new IDTree("(chat_conversationMember)")
            },
            ["callRecords.callRecord"] = new IDTree("(callRecords.callRecord)")
        };

        private static readonly string SerializedComplexTree = @"{
    ""_value"": null,
    ""application"": {
    ""_value"": ""(application)""
    },
    ""callRecords.callRecord"": {
    ""_value"": ""(callRecords.callRecord)""
    },
    ""chat"": {
        ""_value"": ""(chat)"",
        ""conversationMember"": {
            ""_value"": ""(chat_conversationMember)""
        }
    },
    ""team"": {
        ""_value"": ""(team)"",
        ""channel"": {
            ""_value"": ""(team_channel)"",
            ""conversationMember"": {
                ""_value"": ""(team_channel_conversationMember)""
            }
        },
        ""conversationMember"": {
            ""_value"": ""(team_conversationMember)""
        }
    }
}";

        public static IEnumerable<TestCaseData> EqualTestCases()
        {
            yield return new TestCaseData(EmptyTree, EmptyTree2);

            yield return new TestCaseData(OneItemTree, OneItemTree2);
            yield return new TestCaseData(OneItemTree2, OneItemTree);

            yield return new TestCaseData(DepthTwoTree, DepthTwoTree2);
            yield return new TestCaseData(DepthTwoTree2, DepthTwoTree);
        }

        public static IEnumerable<TestCaseData> NotEqualTestCases()
        {
            yield return new TestCaseData(EmptyTree, NullTree);
            yield return new TestCaseData(OneItemTree, NullTree);

            // check for same-value false equivelance
            yield return new TestCaseData(OneItemTree, OneItemTreeKeyDifferent);
            yield return new TestCaseData(OneItemTreeKeyDifferent, OneItemTree);

            // check for same-key false equivelance
            yield return new TestCaseData(OneItemTree, OneItemTreeValueDifferent);
            yield return new TestCaseData(OneItemTreeValueDifferent, OneItemTree);

            // check for subset or superset false equivelance with breadth
            yield return new TestCaseData(OneItemTree, TwoItemTree);
            yield return new TestCaseData(TwoItemTree, OneItemTree);

            // check for subset or superset false equivelance with depth
            yield return new TestCaseData(OneItemTree, DepthTwoTree);
            yield return new TestCaseData(DepthTwoTree, OneItemTree);
        }

        public static IEnumerable<TestCaseData> LosslessSerializationTestCases()
        {
            yield return new TestCaseData(EmptyTree);
            yield return new TestCaseData(OneItemTree);
            yield return new TestCaseData(TwoItemTree);
            yield return new TestCaseData(DepthTwoTree);
            yield return new TestCaseData(ComplexTree);
        }

        public static IEnumerable<TestCaseData> SerializationTestCases()
        {
            yield return new TestCaseData(ComplexTree, SerializedComplexTree);
        }
    }
}

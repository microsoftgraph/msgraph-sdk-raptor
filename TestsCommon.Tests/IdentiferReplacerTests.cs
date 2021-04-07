using NUnit.Framework;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;

namespace TestsCommon.Tests
{
    public class Tests
    {
        private IdentifierReplacer idReplacer;

        [SetUp]
        public void Setup()
        {
            var idRegex = new Regex(@"{([A-Za-z\.]+)\-id}", RegexOptions.Compiled);
            var lines = System.IO.File.ReadAllLines(@"C:\Users\Admin\OneDrive - Microsoft\Desktop\url-replacement\urlsBefore.txt");
            var tree = new IDTree(null);
            foreach (var line in lines.Where(line => line.StartsWith("https:")))
            {
                var matches = idRegex.Matches(line);
                var currentIdNode = tree;
                var parent = string.Empty;
                foreach (Match match in matches)
                {
                    var id = match.Groups[0].Value;
                    var idType = match.Groups[1].Value;

                    parent = parent == string.Empty
                        ? idType
                        : $"{parent}_{idType}";

                    if (!currentIdNode.ContainsKey(idType))
                    {
                        var subTree = new IDTree($"({parent})");
                        currentIdNode[idType] = subTree;
                    }

                    currentIdNode = currentIdNode[idType];
                }
            }

            TestContext.Out.WriteLine(JsonSerializer.Serialize(tree, new JsonSerializerOptions { WriteIndented = true }));

            idReplacer = new IdentifierReplacer(tree);
        }

        [TestCase("https://graph.microsoft.com/v1.0/applications/{application-id}/owners",
                  "https://graph.microsoft.com/v1.0/applications/(application)/owners")]
        [TestCase("https://graph.microsoft.com/v1.0/teams/{team-id}/channels/{channel-id}/members/{conversationMember-id}",
                  "https://graph.microsoft.com/v1.0/teams/(team)/channels/(team_channel)/members/(team_channel_conversationMember)")]
        public void TestIds(string snippetUrl, string expectedUrl)
        {
            var newUrl = idReplacer.ReplaceIds(snippetUrl);
            Assert.AreEqual(expectedUrl, newUrl);
        }
    }
}

using System.Text;
using NUnit.Framework;

namespace TestsCommon.Tests
{
    public class Tests
    {
        private IdentifierReplacer idReplacer;

        [SetUp]
        public void Setup()
        {
            // identifiers.json holds sample tree constructed from V1 urls
            var identifiersJson = System.IO.File.ReadAllText("identifiers.json");
            var tree = System.Text.Json.JsonSerializer.Deserialize<IDTree>(identifiersJson);

            idReplacer = new IdentifierReplacer(tree);
        }

        [TestCase("https://graph.microsoft.com/v1.0/applications/{application-id}/owners",
                  "https://graph.microsoft.com/v1.0/applications/<application>/owners")]
        [TestCase("https://graph.microsoft.com/v1.0/teams/{team-id}/channels/{channel-id}/members/{conversationMember-id}",
                  "https://graph.microsoft.com/v1.0/teams/<team>/channels/<team_channel>/members/<team_channel_conversationMember>")]
        [TestCase("https://graph.microsoft.com/v1.0/communications/callRecords/{callRecords.callRecord-id}?$expand=sessions($expand=segments)",
                  "https://graph.microsoft.com/v1.0/communications/callRecords/<callRecords.callRecord>?$expand=sessions($expand=segments)")]
        [TestCase("https://graph.microsoft.com/v1.0/chats/{chat-id}/members/{conversationMember-id}",
                  "https://graph.microsoft.com/v1.0/chats/<chat>/members/<chat_conversationMember>")]
        [TestCase("https://graph.microsoft.com/v1.0/teams/{team-id}/members/{conversationMember-id}",
                  "https://graph.microsoft.com/v1.0/teams/<team>/members/<team_conversationMember>")]
        public void TestIds(string snippetUrl, string expectedUrl)
        {
            var newUrl = idReplacer.ReplaceIds(snippetUrl);
            Assert.AreEqual(expectedUrl, newUrl);
        }

        [Test]
        public void CreatePSScript()
        {
            var identifiersJson = System.IO.File.ReadAllText("identifiers.json");
            var tree = System.Text.Json.JsonSerializer.Deserialize<IDTree>(identifiersJson);
            string currentRef = "$root";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("$root = @{}");
            CrawlTree(sb, tree, currentRef);
            sb.AppendLine("$root | ConvertTo-Json -Depth 10");
            TestContext.Out.Write(sb.ToString());
            Assert.Pass();
        }

        private void CrawlTree(StringBuilder sb, IDTree tree, string currentRef)
        {
            if (tree.Value is not null)
            {
                sb.AppendLine($"{currentRef}._value = \"{tree.Value}\"");
            }

            foreach (var key in tree.Keys)
            {
                var nextRef = $"{currentRef}[\"{key}\"]";
                sb.AppendLine($"{nextRef} = @{{}}");
                CrawlTree(sb, tree[key], nextRef);
            }
        }



    }
}

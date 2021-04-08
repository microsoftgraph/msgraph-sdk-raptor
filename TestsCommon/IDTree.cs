using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace TestsCommon
{
    /// <summary>
    /// Tree data structure to represent ID placeholders in graph URLs.
    ///
    /// Example URLs:
    ///   https://graph.microsoft.com/v1.0/teams/{team-id}/channels/{channel-id}/members/{conversationMember-id}
    ///   https://graph.microsoft.com/v1.0/communications/callRecords/{callRecords.callRecord-id}?$expand=sessions($expand=segments)
    ///   https://graph.microsoft.com/v1.0/chats/{chat-id}/members/{conversationMember-id}
    ///
    /// Corresponding tree representation:
    ///
    ///                                 root
    ///                               /  |  \
    ///                              /   |   \
    ///                             /    |    \
    ///                         chat   team   callRecords.callRecord
    ///                          /       |
    ///         conversationMember    channel
    ///                                  |
    ///                             conversationMember
    /// </summary>
    [JsonConverter(typeof(IDTreeConverter))]
    public class IDTree : Dictionary<string, IDTree>, IEquatable<IDTree>
    {
        public string Value { get; set; }
        public IDTree(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"{nameof(value)} can't be null or empty!");
            }

            Value = value;
        }

        public IDTree() { }

        public bool Equals(IDTree other)
        {
            return other != null &&
                Value == other.Value &&
                Keys.Count == other.Keys.Count &&
                Keys.All(key => other.ContainsKey(key) && this[key].Equals(other[key]));
        }

        public override bool Equals(object obj) => Equals(obj as IDTree);

        public override int GetHashCode() => base.GetHashCode();
    }

    public class IDTreeConverter : JsonConverter<IDTree>
    {
        public override IDTree Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            IDTree tree = new IDTree();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return tree;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    break;
                }

                var propertyName = reader.GetString();
                if (propertyName == "_value")
                {
                    reader.Read();
                    tree.Value = reader.GetString();
                }
                else
                {
                    IDTree subTree = JsonSerializer.Deserialize<IDTree>(ref reader, options);
                    tree[propertyName] = subTree;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, IDTree tree, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (tree.Value is not null)
            {
                writer.WritePropertyName("_value");
                writer.WriteStringValue(tree.Value);
            }

            foreach (var key in tree.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                writer.WritePropertyName(key);
                Write(writer, tree[key], options);
            }
            writer.WriteEndObject();
            writer.Flush();
        }
    }
}


using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestsCommon
{
    [JsonConverter(typeof(IDTreeConverter))]
    public class IDTree : Dictionary<string, IDTree>
    {
        public string Value { get; set; }
        public IDTree(string value) { Value = value; }
    }

    public class IDTreeConverter : JsonConverter<IDTree>
    {
        public override IDTree Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IDTree tree, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("value");
            writer.WriteStringValue(tree.Value);
            foreach (KeyValuePair<string, IDTree> item in tree)
            {
                writer.WritePropertyName(item.Key);
                Write(writer, item.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}


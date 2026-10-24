using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Json;

namespace Soenneker.Stripe.Client;

// Stripe requires bracket notation for nested objects and indexed collections.
internal sealed class StripeFormSerializationWriter : JsonSerializationWriter, ISerializationWriter
{
    public new Stream GetSerializedContent()
    {
        using Stream json = base.GetSerializedContent();
        using JsonDocument document = JsonDocument.Parse(json);
        var fields = new List<KeyValuePair<string, string>>();
        Flatten(document.RootElement, "", fields);
        using var content = new FormUrlEncodedContent(fields);
        return new MemoryStream(content.ReadAsByteArrayAsync().GetAwaiter().GetResult());
    }

    private static void Flatten(JsonElement element, string prefix, List<KeyValuePair<string, string>> fields)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                    Flatten(property.Value, prefix.Length == 0 ? property.Name : $"{prefix}[{property.Name}]", fields);
                break;
            case JsonValueKind.Array:
                int index = 0;
                foreach (JsonElement item in element.EnumerateArray())
                    Flatten(item, $"{prefix}[{index++}]", fields);
                if (index == 0)
                    fields.Add(new(prefix, ""));
                break;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                break;
            case JsonValueKind.String:
                fields.Add(new(prefix, element.GetString()!));
                break;
            default:
                fields.Add(new(prefix, element.GetRawText()));
                break;
        }
    }
}

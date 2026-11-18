using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Json;

namespace Soenneker.Stripe.Client;

/// <summary>Serializes Stripe OpenAPI models using their generated field mappings.</summary>
public static class StripeModelUtil
{
    /// <summary>Parses a Stripe JSON object with the supplied generated model factory.</summary>
    public static T Parse<T>(string json, ParsableFactory<T> factory) where T : IParsable
    {
        return Parse(JsonElement.Parse(json), factory);
    }

    private static T Parse<T>(JsonElement root, ParsableFactory<T> factory) where T : IParsable
    {
        return new JsonParseNode(root).GetObjectValue(factory)
            ?? throw new JsonException("Stripe returned an empty object.");
    }

    /// <summary>Serializes a generated model to JSON.</summary>
    public static string ToJson(IParsable model)
    {
        using var writer = new JsonSerializationWriter();
        writer.WriteObjectValue(null, model);
        using Stream stream = writer.GetSerializedContent();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static JsonDocument SerializeDocument(IParsable model)
    {
        using var writer = new JsonSerializationWriter();
        writer.WriteObjectValue(null, model);
        using Stream stream = writer.GetSerializedContent();
        return JsonDocument.Parse(stream);
    }

    /// <summary>Returns the identifier from an expandable Stripe field, whether expanded or a string.</summary>
    public static string? GetId(this IParsable? value)
    {
        if (value is null) return null;
        using JsonDocument document = SerializeDocument(value);
        JsonElement root = document.RootElement;
        return root.ValueKind == JsonValueKind.String ? root.GetString()
            : root.ValueKind == JsonValueKind.Object && root.TryGetProperty("id", out JsonElement id) ? id.GetString() : null;
    }

    /// <summary>Converts selected writable fields of a response into a generated request model.</summary>
    public static T SelectFields<T>(IParsable model, ParsableFactory<T> factory, params string[] fields) where T : IParsable
    {
        using JsonDocument document = SerializeDocument(model);
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            foreach (string field in fields)
                if (document.RootElement.TryGetProperty(field, out JsonElement value) && value.ValueKind != JsonValueKind.Null)
                {
                    writer.WritePropertyName(field);
                    value.WriteTo(writer);
                }
            writer.WriteEndObject();
        }
        return Parse(JsonElement.Parse(buffer.WrittenSpan), factory);
    }

    /// <summary>Creates a metadata map for a generated request's additional data.</summary>
    public static UntypedObject Metadata(IDictionary<string, string> values)
    {
        var fields = new Dictionary<string, UntypedNode>(values.Count);
        foreach (var pair in values) fields.Add(pair.Key, new UntypedString(pair.Value));
        return new UntypedObject(fields);
    }
}

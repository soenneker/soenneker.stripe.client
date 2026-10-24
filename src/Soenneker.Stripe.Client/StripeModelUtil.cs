using System;
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
        using JsonDocument document = JsonDocument.Parse(json);
        return new JsonParseNode(document.RootElement.Clone()).GetObjectValue(factory)
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

    /// <summary>Returns the identifier from an expandable Stripe field, whether expanded or a string.</summary>
    public static string? GetId(this IParsable? value)
    {
        if (value is null) return null;
        using JsonDocument document = JsonDocument.Parse(ToJson(value));
        JsonElement root = document.RootElement;
        return root.ValueKind == JsonValueKind.String ? root.GetString()
            : root.ValueKind == JsonValueKind.Object && root.TryGetProperty("id", out JsonElement id) ? id.GetString() : null;
    }

    /// <summary>Converts selected writable fields of a response into a generated request model.</summary>
    public static T SelectFields<T>(IParsable model, ParsableFactory<T> factory, params string[] fields) where T : IParsable
    {
        using JsonDocument document = JsonDocument.Parse(ToJson(model));
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
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
        return Parse(System.Text.Encoding.UTF8.GetString(stream.ToArray()), factory);
    }

    /// <summary>Creates a metadata map for a generated request's additional data.</summary>
    public static UntypedObject Metadata(IDictionary<string, string> values)
    {
        var fields = new Dictionary<string, UntypedNode>();
        foreach (var pair in values) fields.Add(pair.Key, new UntypedString(pair.Value));
        return new UntypedObject(fields);
    }
}

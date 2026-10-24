using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Kiota.Abstractions.Helpers;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Soenneker.Stripe.Client;

/// <summary>Reads values from generated Stripe models without losing their wire representation.</summary>
public static class StripeModelExtensions
{
    /// <summary>Returns the wire value of a generated enum.</summary>
    public static string? ToStripeValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(this T? value) where T : struct, Enum =>
        value.HasValue ? EnumHelpers.GetEnumStringValue(value.Value) : null;

    /// <summary>Returns a Stripe identifier that is already represented as a string.</summary>
    public static string? GetId(this string? value) => value;

    /// <summary>Returns a metadata value from the generated metadata container.</summary>
    public static string? GetValueOrDefault(this IAdditionalDataHolder? metadata, string key) =>
        metadata != null && metadata.AdditionalData.TryGetValue(key, out object? value) ? value?.ToString() : null;

    /// <summary>Attempts to read a metadata value.</summary>
    public static bool TryGetValue(this IAdditionalDataHolder metadata, string key, out string? value)
    {
        value = metadata.GetValueOrDefault(key);
        return value != null;
    }

    /// <summary>Copies generated metadata into a string dictionary.</summary>
    public static Dictionary<string, string> ToStripeMetadata(this IAdditionalDataHolder? metadata)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (metadata != null)
            foreach (var pair in metadata.AdditionalData) result[pair.Key] = pair.Value?.ToString() ?? "";
        return result;
    }

    /// <summary>Converts a nullable Stripe Unix timestamp to UTC.</summary>
    public static DateTimeOffset? ToStripeDateTime(this int? timestamp) => timestamp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(timestamp.Value) : null;
}

using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Soenneker.Stripe.Client;

/// <summary>Verifies Stripe webhook signatures against the original request body.</summary>
public static class StripeWebhookSignature
{
    /// <summary>Rejects malformed, expired, or invalid signatures; accepts any matching v1 signature during secret rotation.</summary>
    public static void Verify(string payload, string signatureHeader, string secret, long toleranceSeconds = 300, DateTimeOffset? now = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        string[] parts = signatureHeader.Split(',');
        string? timestamp = null;
        foreach (string part in parts)
            if (part.StartsWith("t=", StringComparison.Ordinal))
            {
                if (timestamp != null) throw new CryptographicException("Multiple Stripe signature timestamps.");
                timestamp = part[2..];
            }
        if (!long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out long seconds) || toleranceSeconds < 0)
            throw new CryptographicException("Invalid Stripe signature timestamp.");
        long current = (now ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds();
        if (seconds < current - toleranceSeconds || seconds > current + toleranceSeconds)
            throw new CryptographicException("Stripe signature timestamp is outside the allowed tolerance.");

        byte[] expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{timestamp}.{payload}"));
        foreach (string part in parts)
        {
            if (!part.StartsWith("v1=", StringComparison.Ordinal) || part.Length != 67) continue;
            try
            {
                if (CryptographicOperations.FixedTimeEquals(expected, Convert.FromHexString(part[3..]))) return;
            }
            catch (FormatException) { }
        }
        throw new CryptographicException("Stripe webhook signature did not match.");
    }
}

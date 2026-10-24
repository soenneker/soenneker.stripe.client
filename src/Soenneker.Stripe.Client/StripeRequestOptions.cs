using Microsoft.Kiota.Abstractions;

namespace Soenneker.Stripe.Client;

/// <summary>Optional headers for a Stripe API request.</summary>
public sealed class StripeRequestOptions
{
    /// <summary>Identifies a mutation so retries cannot create duplicate operations.</summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>The connected account on whose behalf the request is made.</summary>
    public string? StripeAccount { get; set; }

    /// <summary>Applies the configured headers to a generated request.</summary>
    public void Apply(RequestHeaders headers)
    {
        if (!string.IsNullOrWhiteSpace(IdempotencyKey))
            headers.Add("Idempotency-Key", IdempotencyKey);
        if (!string.IsNullOrWhiteSpace(StripeAccount))
            headers.Add("Stripe-Account", StripeAccount);
    }
}

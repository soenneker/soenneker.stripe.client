using Soenneker.Stripe.OpenApiClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Stripe.Client.Abstract;

/// <summary>
/// Provides a lazily initialized Stripe OpenAPI client backed by an owned HTTP client with request and response logging.
/// </summary>
public interface IStripeClientUtil : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the shared Stripe client for this utility instance.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    ValueTask<StripeOpenApiClient> Get(CancellationToken cancellationToken = default);

    /// <summary>Removes and disposes the Stripe and HTTP clients owned by this utility.</summary>
    new void Dispose();

    /// <summary>Asynchronously removes and disposes the Stripe and HTTP clients owned by this utility.</summary>
    new ValueTask DisposeAsync();
}

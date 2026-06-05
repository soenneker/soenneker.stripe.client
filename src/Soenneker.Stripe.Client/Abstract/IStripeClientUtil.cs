using Stripe;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Stripe.Client.Abstract;

/// <summary>
/// A .NET typesafe implementation of Stripe's StripeClient
/// </summary>
public interface IStripeClientUtil : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    ValueTask<StripeClient> Get(CancellationToken cancellationToken = default);
}
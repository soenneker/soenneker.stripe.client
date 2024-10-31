using Stripe;
using System;
using System.Threading.Tasks;

namespace Soenneker.Stripe.Client.Abstract;

/// <summary>
/// A .NET typesafe implementation of Stripe's StripeClient
/// </summary>
public interface IStripeClientUtil : IDisposable, IAsyncDisposable
{
    ValueTask<StripeClient> Get();
}
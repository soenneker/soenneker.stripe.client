using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Stripe.Client.Abstract;
using Soenneker.Utils.HttpClientCache.Registrar;

namespace Soenneker.Stripe.Client.Registrars;

/// <summary>
/// A .NET typesafe implementation of Stripe's StripeClient
/// </summary>
public static class StripeClientUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IStripeClientUtil"/> as a singleton service. <para/>
    /// </summary>
    /// <remarks>This is most likely what you want.</remarks>
    public static IServiceCollection AddStripeClientUtilAsSingleton(this IServiceCollection services)
    {
        services.AddHttpClientCacheAsSingleton().TryAddSingleton<IStripeClientUtil, StripeClientUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IStripeClientUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddStripeClientUtilAsScoped(this IServiceCollection services)
    {
        services.AddHttpClientCacheAsSingleton().TryAddScoped<IStripeClientUtil, StripeClientUtil>();

        return services;
    }
}
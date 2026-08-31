using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Stripe.Client.Abstract;
using Soenneker.Utils.HttpClientCache.Registrar;

namespace Soenneker.Stripe.Client.Registrars;

/// <summary>
/// Registers the lazily initialized Stripe SDK client.
/// </summary>
public static class StripeClientUtilRegistrar
{
    /// <summary>
    /// Adds the Stripe client utility as a singleton service. <para/>
    /// </summary>
    /// <remarks>This is most likely what you want.</remarks>
    public static IServiceCollection AddStripeClientUtilAsSingleton(this IServiceCollection services)
    {
        services.AddHttpClientCacheAsSingleton().TryAddSingleton<IStripeClientUtil, StripeClientUtil>();

        return services;
    }

    /// <summary>
    /// Adds the Stripe client utility as a scoped service. Each scope owns a separate HTTP client. <para/>
    /// </summary>
    public static IServiceCollection AddStripeClientUtilAsScoped(this IServiceCollection services)
    {
        services.AddHttpClientCacheAsSingleton().TryAddScoped<IStripeClientUtil, StripeClientUtil>();

        return services;
    }
}

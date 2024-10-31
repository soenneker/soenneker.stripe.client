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
    public static void AddStripeClientUtilAsSingleton(this IServiceCollection services)
    {
        services.AddHttpClientCache();
        services.TryAddSingleton<IStripeClientUtil, StripeClientUtil>();
    }

    /// <summary>
    /// Adds <see cref="IStripeClientUtil"/> as a scoped service. <para/>
    /// </summary>
    public static void AddStripeClientUtilAsScoped(this IServiceCollection services)
    {
        services.AddHttpClientCache();
        services.TryAddScoped<IStripeClientUtil, StripeClientUtil>();
    }
}

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Configuration;
using Soenneker.Extensions.ValueTask;
using Soenneker.Stripe.Client.Abstract;
using Soenneker.Utils.AsyncSingleton;
using Soenneker.Utils.HttpClientCache.Abstract;
using Stripe;

namespace Soenneker.Stripe.Client;

///<inheritdoc cref="IStripeClientUtil"/>
public sealed class StripeClientUtil : IStripeClientUtil
{
    private readonly IHttpClientCache _httpClientCache;

    private readonly AsyncSingleton<StripeClient> _client;

    public StripeClientUtil(ILogger<StripeClientUtil> logger, IHttpClientCache httpClientCache, IConfiguration config)
    {
        _httpClientCache = httpClientCache;

        _client = new AsyncSingleton<StripeClient>(async (cancellationToken) =>
        {
            logger.LogDebug("Initializing Stripe client...");

            HttpClient httpClient = await _httpClientCache.Get(nameof(StripeClientUtil), () => null, cancellationToken).NoSync();

            var stripeClient = new SystemNetHttpClient(httpClient);

            var secretKey = config.GetValueStrict<string>("Stripe:SecretKey");

            return new StripeClient(secretKey, null, stripeClient);
        });
    }

    public ValueTask<StripeClient> Get(CancellationToken cancellationToken = default)
    {
        return _client.Get(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _httpClientCache.Remove(nameof(StripeClientUtil)).NoSync();

        await _client.DisposeAsync().NoSync();
    }

    public void Dispose()
    {
        _httpClientCache.RemoveSync(nameof(StripeClientUtil));

        _client.Dispose();
    }
}
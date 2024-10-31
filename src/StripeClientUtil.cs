using System;
using System.Net.Http;
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
public class StripeClientUtil : IStripeClientUtil
{
    private readonly IHttpClientCache _httpClientCache;

    private readonly AsyncSingleton<StripeClient> _client;

    public StripeClientUtil(ILogger<StripeClientUtil> logger, IHttpClientCache httpClientCache, IConfiguration config)
    {
        _httpClientCache = httpClientCache;

        _client = new AsyncSingleton<StripeClient>(async () =>
        {
            logger.LogDebug("Initializing Stripe client...");

            HttpClient httpClient = await _httpClientCache.Get(nameof(StripeClientUtil)).NoSync();

            var stripeClient = new SystemNetHttpClient(httpClient);

            var secretKey = config.GetValueStrict<string>("Stripe:SecretKey");

            return new StripeClient(secretKey, null, stripeClient);
        });
    }

    public ValueTask<StripeClient> Get()
    {
        return _client.Get();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await _httpClientCache.Remove(nameof(StripeClientUtil)).NoSync();

        await _client.DisposeAsync().NoSync();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _httpClientCache.Remove(nameof(StripeClientUtil));

        _client.Dispose();
    }
}
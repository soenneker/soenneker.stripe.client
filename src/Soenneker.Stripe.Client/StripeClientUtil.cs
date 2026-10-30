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
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Soenneker.Stripe.OpenApiClient;
using Soenneker.Dtos.HttpClientOptions;
using Soenneker.HttpClients.LoggingHandler;
using System.Collections.Generic;

namespace Soenneker.Stripe.Client;

public sealed class StripeClientUtil : IStripeClientUtil
{
    private readonly IHttpClientCache _httpClientCache;
    private readonly string _cacheKey = $"{nameof(StripeClientUtil)}-{Guid.NewGuid()}";

    private readonly AsyncSingleton<StripeOpenApiClient> _client;

    private readonly ILogger<StripeClientUtil> _logger;
    private readonly IConfiguration _config;

    public StripeClientUtil(ILogger<StripeClientUtil> logger, IHttpClientCache httpClientCache, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        _httpClientCache = httpClientCache;

        _client = new AsyncSingleton<StripeOpenApiClient>(CreateClient);
    }

    private async ValueTask<StripeOpenApiClient> CreateClient(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Initializing Stripe client...");

        HttpClient httpClient = await _httpClientCache.Get(_cacheKey, (config: _config, logger: _logger), static state => new HttpClientOptions
        {
            BaseAddress = new Uri("https://api.stripe.com"),
            DefaultRequestHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {state.config.GetStringStrict("Stripe:SecretKey")}",
                // Matches the specification used by Soenneker.Stripe.OpenApiClient 4.0.140.
                ["Stripe-Version"] = "2026-09-30.endive"
            },
            DelegatingHandlerFactories =
            [
                () => new StripeQueryHandler(),
                () => new HttpClientLoggingHandler(state.logger, new HttpClientLoggingOptions
                {
                    LogLevel = LogLevel.Debug,
                    LogRequestBody = true,
                    LogResponseBody = true,
                    LogQueryString = true
                })
            ]
        }, cancellationToken).NoSync();

        return new StripeOpenApiClient(new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(),
            serializationWriterFactory: new StripeSerializationWriterFactory(), httpClient: httpClient));
    }

    public ValueTask<StripeOpenApiClient> Get(CancellationToken cancellationToken = default)
    {
        return _client.Get(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _httpClientCache.Remove(_cacheKey).NoSync();

        await _client.DisposeAsync().NoSync();
    }

    public void Dispose()
    {
        _httpClientCache.RemoveSync(_cacheKey);

        _client.Dispose();
    }
}

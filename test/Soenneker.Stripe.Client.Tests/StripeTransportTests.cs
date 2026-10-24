using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Soenneker.Stripe.OpenApiClient;
using Soenneker.Stripe.OpenApiClient.Models;

namespace Soenneker.Stripe.Client.Tests;

public sealed class StripeTransportTests
{
    [Test]
    public async Task Nested_forms_keep_metadata_empty_values_and_idempotency()
    {
        using var handler = new RecordingStripeHandler();
        using var http = new HttpClient(new StripeQueryHandler { InnerHandler = handler });
        using var adapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), serializationWriterFactory: new StripeSerializationWriterFactory(), httpClient: http);
        var api = new StripeOpenApiClient(adapter);
        var options = new PostCustomersCustomerXWwwFormUrlencodedRequest
        {
            InvoiceSettings = new() { DefaultPaymentMethod = "" },
            Metadata = new() { PostCustomersCustomerXWwwFormUrlencodedRequestMetadataAnyOf1 = new() { AdditionalData = new Dictionary<string, object> { ["organization"] = "a&b" } } }
        };
        await api.V1.Customers["cus_test"].PostAsync(options, c => new StripeRequestOptions { IdempotencyKey = "operation-1" }.Apply(c.Headers));
        string body = Uri.UnescapeDataString(handler.Body!);
        if (!body.Contains("invoice_settings[default_payment_method]=") || !body.Contains("metadata[organization]=a&b") || handler.IdempotencyKey != "operation-1")
            throw new InvalidOperationException($"Unexpected form encoding: {body}");
    }

    [Test]
    public async Task Get_requests_keep_query_parameters_and_have_no_body()
    {
        using var handler = new RecordingStripeHandler();
        using var http = new HttpClient(new StripeQueryHandler { InnerHandler = handler });
        using var adapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), serializationWriterFactory: new StripeSerializationWriterFactory(), httpClient: http);
        var api = new StripeOpenApiClient(adapter);
        await api.V1.Customers.GetAsync(new(), c => { c.QueryParameters.Limit = 100; c.QueryParameters.StartingAfter = "cus_previous"; });
        if (handler.Body != null || !handler.Uri!.Query.Contains("starting_after=cus_previous") || !handler.Uri.Query.Contains("limit=100"))
            throw new InvalidOperationException("GET parameters were not sent in the URL.");
    }

    [Test]
    public void Webhook_signatures_reject_tampering_and_expiration_and_allow_rotation()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1800000000);
        const string payload = "{\"id\":\"evt_test\"}";
        string signature = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes("secret"), Encoding.UTF8.GetBytes($"{now.ToUnixTimeSeconds()}.{payload}")));
        string header = $"t={now.ToUnixTimeSeconds()},v1={new string('0', 64)},v1={signature}";
        StripeWebhookSignature.Verify(payload, header, "secret", now: now);
        MustReject(() => StripeWebhookSignature.Verify(payload + " ", header, "secret", now: now));
        MustReject(() => StripeWebhookSignature.Verify(payload, header, "secret", now: now.AddSeconds(301)));
        MustReject(() => StripeWebhookSignature.Verify(payload, header, "secret", now: now.AddSeconds(-301)));
    }

    private static void MustReject(Action action)
    {
        try { action(); }
        catch (CryptographicException) { return; }
        throw new InvalidOperationException("An invalid webhook signature was accepted.");
    }
}

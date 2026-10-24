using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Stripe.Client.Tests;

internal sealed class RecordingStripeHandler : HttpMessageHandler
{
    public string? Body { get; private set; }
    public Uri? Uri { get; private set; }
    public string? IdempotencyKey { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Uri = request.RequestUri;
        IdempotencyKey = request.Headers.TryGetValues("Idempotency-Key", out var values) ? string.Join("", values) : null;
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"id\":\"cus_test\",\"object\":\"customer\"}", System.Text.Encoding.UTF8, "application/json") };
    }
}

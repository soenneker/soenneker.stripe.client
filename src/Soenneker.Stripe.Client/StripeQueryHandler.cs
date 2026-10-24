using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Stripe.Client;

// Stripe's specification declares GET form bodies; the API expects these fields in the query string.
internal sealed class StripeQueryHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method == HttpMethod.Get && request.Content is not null)
        {
            string query = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (query.Length != 0)
            {
                var uri = new UriBuilder(request.RequestUri!);
                uri.Query = string.IsNullOrEmpty(uri.Query) ? query : $"{uri.Query.TrimStart('?')}&{query}";
                request.RequestUri = uri.Uri;
            }
            request.Content.Dispose();
            request.Content = null;
        }
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}

[![](https://img.shields.io/nuget/v/soenneker.stripe.client.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.stripe.client/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.stripe.client/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.stripe.client/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.stripe.client.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.stripe.client/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.stripe.client/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.stripe.client/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Stripe.Client

Provides a lazily initialized `StripeClient` backed by a cached `HttpClient`, ready for Stripe.net services such as customers, subscriptions, invoices, payment intents, and payment methods.

## Installation

```bash
dotnet add package Soenneker.Stripe.Client
```

## Configuration

```json
{
  "Stripe": {
    "SecretKey": "sk_test_..."
  }
}
```

## Usage

```csharp
using Soenneker.Stripe.Client.Abstract;
using Soenneker.Stripe.Client.Registrars;
using Stripe;

services.AddStripeClientUtilAsSingleton();

StripeClient client = await stripeClientUtil.Get(cancellationToken);
var customers = new CustomerService(client);

StripeList<Customer> page = await customers.ListAsync(
    new CustomerListOptions { Limit = 10 },
    cancellationToken: cancellationToken);
```

Use the singleton registration for one application-wide Stripe account. Scoped registration creates an independently owned Stripe and HTTP client per scope; disposing the utility releases both.

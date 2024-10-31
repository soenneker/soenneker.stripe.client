using Soenneker.Stripe.Client.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.Stripe.Client.Tests;

[Collection("Collection")]
public class StripeClientUtilTests : FixturedUnitTest
{
    private readonly IStripeClientUtil _util;

    public StripeClientUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IStripeClientUtil>();
    }
}

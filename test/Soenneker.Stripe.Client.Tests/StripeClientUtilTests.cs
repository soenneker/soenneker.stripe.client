using Soenneker.Stripe.Client.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Stripe.Client.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class StripeClientUtilTests : HostedUnitTest
{
    private readonly IStripeClientUtil _util;

    public StripeClientUtilTests(Host host) : base(host)
    {
        _util = Resolve<IStripeClientUtil>(true);
    }

    [Test]
    public void Default()
    {

    }
}

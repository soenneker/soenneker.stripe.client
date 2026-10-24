using Microsoft.Kiota.Abstractions.Serialization;

namespace Soenneker.Stripe.Client;

internal sealed class StripeSerializationWriterFactory : ISerializationWriterFactory
{
    public string ValidContentType => "application/x-www-form-urlencoded";

    public ISerializationWriter GetSerializationWriter(string contentType) =>
        contentType.Split(';')[0] == ValidContentType
            ? new StripeFormSerializationWriter()
            : SerializationWriterFactoryRegistry.DefaultInstance.GetSerializationWriter(contentType);
}

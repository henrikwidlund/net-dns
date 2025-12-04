using System.Threading.Tasks;

using Makaretu.Dns;

namespace DnsTests;

public class ResourceRegistryTest
{
    [Test]
    public async Task Exists() => await Assert.That(ResourceRegistry.Records).Count().IsNotEqualTo(0);

    [Test]
    public async Task Create()
    {
        var rr = ResourceRegistry.Create(DnsType.NS);
        await Assert.That(rr).IsTypeOf<NSRecord>();

        rr = ResourceRegistry.Create((DnsType)1234);
        await Assert.That(rr).IsTypeOf<UnknownRecord>();
    }
}
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class ResourceRegistryTest
{
    [Test]
    public void Exists() => ResourceRegistry.Records.Count.ShouldNotBe(0);

    [Test]
    public void Create()
    {
        var rr = ResourceRegistry.Create(DnsType.NS);
        rr.ShouldBeOfType<NSRecord>();

        rr = ResourceRegistry.Create((DnsType)1234);
        rr.ShouldBeOfType<UnknownRecord>();
    }
}
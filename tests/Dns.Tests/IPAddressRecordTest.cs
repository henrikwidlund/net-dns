using System.Net;
using System.Threading.Tasks;

using Makaretu.Dns;

namespace DnsTests;

public class IPAddressRecordTest
{
    [Test]
    public async Task Create()
    {
        var rr = IPAddressRecord.Create("foo", IPAddress.Loopback);
        await Assert.That(rr.Name).IsEquatableOrEqualTo("foo");
        await Assert.That(rr.Type).IsEqualTo(DnsType.A);
        await Assert.That(rr.Address).IsEqualTo(IPAddress.Loopback);

        rr = IPAddressRecord.Create("foo", IPAddress.IPv6Loopback);
        await Assert.That(rr.Name).IsEquatableOrEqualTo("foo");
        await Assert.That(rr.Type).IsEqualTo(DnsType.AAAA);
        await Assert.That(rr.Address).IsEqualTo(IPAddress.IPv6Loopback);
    }
}

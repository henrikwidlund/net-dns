using System.Net;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class AddressRecordTest
{
    [Fact]
    public void Create()
    {
        var rr = AddressRecord.Create("foo", IPAddress.Loopback);
        rr.Name.ShouldBe("foo");
        rr.Type.ShouldBe(DnsType.A);
        rr.Address.ShouldBe(IPAddress.Loopback);

        rr = AddressRecord.Create("foo", IPAddress.IPv6Loopback);
        rr.Name.ShouldBe("foo");
        rr.Type.ShouldBe(DnsType.AAAA);
        rr.Address.ShouldBe(IPAddress.IPv6Loopback);
    }
}
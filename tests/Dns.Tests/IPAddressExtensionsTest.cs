using System.Net;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class IPAddressExtensionsTest
{
    [Fact]
    public void ArpaName()
    {
        IPAddress.Parse("8.8.4.4").GetArpaName().ShouldBe("4.4.8.8.in-addr.arpa");
        IPAddress.Parse("2001:db8::567:89ab").GetArpaName().ShouldBe("b.a.9.8.7.6.5.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.8.b.d.0.1.0.0.2.ip6.arpa");
    }
}
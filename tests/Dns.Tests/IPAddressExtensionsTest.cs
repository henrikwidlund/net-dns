using System.Net;
using System.Threading.Tasks;

using Makaretu.Dns;

namespace DnsTests;

public class IPAddressExtensionsTest
{
    [Test]
    public async Task ArpaName()
    {
        await Assert.That(IPAddress.Parse("8.8.4.4").GetArpaName()).IsEqualTo("4.4.8.8.in-addr.arpa");
        await Assert.That(IPAddress.Parse("2001:db8::567:89ab").GetArpaName()).IsEqualTo("b.a.9.8.7.6.5.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.8.b.d.0.1.0.0.2.ip6.arpa");
    }
}
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Makaretu.Dns;

namespace Makaretu.Mdns;

public class ServiceProfileTest
{
    [Test]
    public async Task Defaults()
    {
        var service = new ServiceProfile();
        await Assert.That(service.Resources).IsNotNull();
    }

    [Test]
    public async Task QualifiedNames()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024, [IPAddress.Loopback]);

        await Assert.That(service.QualifiedServiceName.ToString()).IsEqualTo("_sdtest._udp.local");
        await Assert.That(service.FullyQualifiedName.ToString()).IsEqualTo("x._sdtest._udp.local");
    }

    [Test]
    public async Task ResourceRecords()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024, [IPAddress.Loopback]);

        await Assert.That(service.Resources).Any(static x => x is SRVRecord);
        await Assert.That(service.Resources).Any(static x => x is TXTRecord);
        await Assert.That(service.Resources).Any(static x => x is ARecord);
    }

    [Test]
    public async Task Addresses_Default()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024);
        await Assert.That(service.Resources).Any(static r => r.Type is DnsType.A or DnsType.AAAA);
    }

    [Test]
    public async Task Addresses_IPv4()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024, [IPAddress.Loopback]);
        await Assert.That(service.Resources).Any(static r => r.Type == DnsType.A);
    }

    [Test]
    public async Task Addresses_IPv6()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024, [IPAddress.IPv6Loopback]);
        await Assert.That(service.Resources).Any(static r => r.Type == DnsType.AAAA);
    }

    [Test]
    public async Task TXTRecords()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024);
        var txt = service.Resources.OfType<TXTRecord>().First();
        txt.Strings.AddRange(["a=1", "b=2"]);

        await Assert.That(txt.Strings).Contains("txtvers=1").And.Contains("a=1").And.Contains("b=2");
    }

    [Test]
    public async Task AddProperty()
    {
        var service = new ServiceProfile
        {
            InstanceName = "x",
            ServiceName = "_sdtest._udp"
        };
        service.AddProperty("a", "1");

        var txt = service.Resources.OfType<TXTRecord>().First();

        await Assert.That(txt.Name).IsEqualTo(service.FullyQualifiedName);
        await Assert.That(txt.Strings).Contains("a=1");
    }

    [Test]
    public async Task TTLs()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024);

        await Assert.That(service.Resources.OfType<TXTRecord>().First().TTL).IsEqualTo(TimeSpan.FromMinutes(75));
        await Assert.That(service.Resources.OfType<AddressRecord>().First().TTL).IsEqualTo(TimeSpan.FromSeconds(120));
    }

    [Test]
    public async Task Subtypes()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024);
        await Assert.That(service.Subtypes).HasCount().Zero();
    }

    [Test]
    public async Task HostName()
    {
        var service = new ServiceProfile("fred", "_foo._tcp", 1024);
        await Assert.That(service.HostName.ToString()).IsEqualTo("fred.foo.local");

        service = new ServiceProfile("fred", "_foo_bar._tcp", 1024);
        await Assert.That(service.HostName.ToString()).IsEqualTo("fred.foo-bar.local");
    }
}
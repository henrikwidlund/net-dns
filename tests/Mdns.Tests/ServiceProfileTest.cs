using System;
using System.Linq;
using System.Net;

using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace Makaretu.Mdns;

public class ServiveProfileTest
{
    [Fact]
    public void Defaults()
    {
        var service = new ServiceProfile();
        service.Resources.ShouldNotBeNull();
    }

    [Fact]
    public void QualifiedNames()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024, [IPAddress.Loopback]);

        service.QualifiedServiceName.ShouldBe("_sdtest._udp.local");
        service.FullyQualifiedName.ShouldBe("x._sdtest._udp.local");
    }

    [Fact]
    public void ResourceRecords()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024, [IPAddress.Loopback]);

        service.Resources.OfType<SRVRecord>().Any().ShouldBeTrue();
        service.Resources.OfType<TXTRecord>().Any().ShouldBeTrue();
        service.Resources.OfType<ARecord>().Any().ShouldBeTrue();
    }

    [Fact]
    public void Addresses_Default()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024);
        service.Resources.Exists(static r => r.Type is DnsType.A or DnsType.AAAA).ShouldBeTrue();
    }

    [Fact]
    public void Addresses_IPv4()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024, [IPAddress.Loopback]);
        service.Resources.Exists(static r => r.Type == DnsType.A).ShouldBeTrue();
    }

    [Fact]
    public void Addresses_IPv6()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024, [IPAddress.IPv6Loopback]);
        service.Resources.Exists(static r => r.Type == DnsType.AAAA).ShouldBeTrue();
    }

    [Fact]
    public void TXTRecords()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024);
        var txt = service.Resources.OfType<TXTRecord>().First();
        txt.Strings.AddRange(["a=1", "b=2"]);
        
        txt.Strings.ShouldContain("txtvers=1");
        txt.Strings.ShouldContain("a=1");
        txt.Strings.ShouldContain("b=2");
    }

    [Fact]
    public void AddProperty()
    {
        var service = new ServiceProfile
        {
            InstanceName = "x",
            ServiceName = "_sdtest._udp"
        };
        service.AddProperty("a", "1");

        var txt = service.Resources.OfType<TXTRecord>().First();
        
        txt.Name.ShouldBe(service.FullyQualifiedName);
        txt.Strings.ShouldContain("a=1");
    }

    [Fact]
    public void TTLs()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024);
        
        service.Resources.OfType<TXTRecord>().First().TTL.ShouldBe(TimeSpan.FromMinutes(75));
        service.Resources.OfType<AddressRecord>().First().TTL.ShouldBe(TimeSpan.FromSeconds(120));
    }

    [Fact]
    public void Subtypes()
    {
        var service = new ServiceProfile("x", "_sdtest._udp", 1024);
        service.Subtypes.Count.ShouldBe(0);
    }

    [Fact]
    public void HostName()
    {
        var service = new ServiceProfile("fred", "_foo._tcp", 1024);
        service.HostName.ShouldBe("fred.foo.local");

        service = new ServiceProfile("fred", "_foo_bar._tcp", 1024);
        service.HostName.ShouldBe("fred.foo-bar.local");
    }
}
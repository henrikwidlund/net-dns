using System.Net;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class AAAARecordTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new AAAARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb")
        };
        var b = (AAAARecord)new ResourceRecord().Read(a.ToByteArray());
        
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Address.ShouldBe(b.Address);
    }

    [Fact]
    public void Roundtrip_ScopeId()
    {
        var a = new AAAARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("fe80::7573:b0a8:46b0:bfea%17")
        };
        var b = (AAAARecord)new ResourceRecord().Read(a.ToByteArray());
        
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        IPAddress.Parse("fe80::7573:b0a8:46b0:bfea").ShouldBe(b.Address);
    }

    [Fact]
    public void Roundtrip_Master()
    {
        var a = new AAAARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb")
        };
        var b = (AAAARecord)new ResourceRecord().Read(a.ToString());
        
        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Address.ShouldBe(b.Address);
    }

    [Fact]
    public void Equality()
    {
        var a = new AAAARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb")
        };
        var b = new AAAARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25ce")
        };
        
        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }
}
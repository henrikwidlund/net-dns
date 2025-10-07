using System.Net;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class ARecordTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new ARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("127.0.0.1")
        };

        var b = (ARecord)new ResourceRecord().Read(a.ToByteArray());

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Address.ShouldBe(b.Address);
    }

    [Fact]
    public void Roundtrip_Master()
    {
        var a = new ARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("127.0.0.1")
        };
        var b = (ARecord)new ResourceRecord().Read(a.ToString());

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
        var a = new ARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("127.0.0.1")
        };

        var b = new ARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("127.0.0.2")
        };

        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }
}
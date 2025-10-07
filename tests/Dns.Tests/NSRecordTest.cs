using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class NSRecordTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new NSRecord
        {
            Name = "emanon.org",
            Authority = "mydomain.name"
        };

        var b = (NSRecord)new ResourceRecord().Read(a.ToByteArray());

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Authority.ShouldBe(b.Authority);
    }

    [Fact]
    public void Roundtrip_Master()
    {
        var a = new NSRecord
        {
            Name = "emanon.org",
            Authority = "mydomain.name"
        };

        var b = (NSRecord)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Authority.ShouldBe(b.Authority);
    }

    [Fact]
    public void Equality()
    {
        var a = new NSRecord
        {
            Name = "emanon.org",
            Authority = "mydomain.name"
        };

        var b = new NSRecord
        {
            Name = "emanon.org",
            Authority = "mydomainx.name"
        };

        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }
}
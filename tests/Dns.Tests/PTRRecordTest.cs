using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class PTRRecordTest
{
    [Test]
    public void Roundtrip()
    {
        var a = new PTRRecord
        {
            Name = "emanon.org",
            DomainName = "somewhere.else.org"
        };
        
        var b = (PTRRecord)new ResourceRecord().Read(a.ToByteArray());
        
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.DomainName.ShouldBe(b.DomainName);
    }

    [Test]
    public void Roundtrip_Master()
    {
        var a = new PTRRecord
        {
            Name = "emanon.org",
            DomainName = "somewhere.else.org"
        };
        
        var b = (PTRRecord)new ResourceRecord().Read(a.ToString());
        
        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.DomainName.ShouldBe(b.DomainName);
    }

    [Test]
    public void Equality()
    {
        var a = new PTRRecord
        {
            Name = "emanon.org",
            DomainName = "somewhere.else.org"
        };
        
        var b = new PTRRecord
        {
            Name = "emanon.org",
            DomainName = "somewhere.org"
        };

        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }
}
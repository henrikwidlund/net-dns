using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class AFSDBRecordTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new AFSDBRecord
        {
            Name = "emanon.org",
            Subtype = 1,
            Target = "afs.emanon.org"
        };

        var b = (AFSDBRecord)new ResourceRecord().Read(a.ToByteArray());

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Subtype.ShouldBe(b.Subtype);
        a.Target.ShouldBe(b.Target);
    }

    [Fact]
    public void Roundtrip_Master()
    {
        var a = new AFSDBRecord
        {
            Name = "emanon.org",
            Subtype = 1,
            Target = "afs.emanon.org"
        };

        var b = (AFSDBRecord)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Subtype.ShouldBe(b.Subtype);
        a.Target.ShouldBe(b.Target);
    }

    [Fact]
    public void Equality()
    {
        var a = new AFSDBRecord
        {
            Name = "emanon.org",
            Subtype = 1,
            Target = "afs.emanon.org"
        };

        var b = new AFSDBRecord
        {
            Name = "emanon.org",
            Subtype = 2,
            Target = "afs.emanon.org"
        };

        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }
}
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class DNAMERecordTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new DNAMERecord
        {
            Name = "emanon.org",
            Target = "somewhere.else.org"
        };

        var b = (DNAMERecord)new ResourceRecord().Read(a.ToByteArray());

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Target.ShouldBe(b.Target);
    }

    [Fact]
    public void Roundtrip_Master()
    {
        var a = new DNAMERecord
        {
            Name = "emanon.org",
            Target = "somewhere.else.org"
        };

        var b = (DNAMERecord)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Target.ShouldBe(b.Target);
    }

    [Fact]
    public void Equality()
    {
        var a = new DNAMERecord
        {
            Name = "emanon.org",
            Target = "somewhere.else.org"
        };

        var b = new DNAMERecord
        {
            Name = "emanon.org",
            Target = "somewhere.org"
        };

        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }
}
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class CNAMERecordTest
{
    [Test]
    public void Roundtrip()
    {
        var a = new CNAMERecord
        {
            Name = "emanon.org",
            Target = "somewhere.else.org"
        };

        var b = (CNAMERecord)new ResourceRecord().Read(a.ToByteArray());

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Target.ShouldBe(b.Target);
    }

    [Test]
    public void Roundtrip_Master()
    {
        var a = new CNAMERecord
        {
            Name = "emanon.org",
            Target = "somewhere.else.org"
        };

        var b = (CNAMERecord)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Target.ShouldBe(b.Target);
    }

    [Test]
    public void Equality()
    {
        var a = new CNAMERecord
        {
            Name = "emanon.org",
            Target = "somewhere.else.org"
        };

        var b = new CNAMERecord
        {
            Name = "emanon.org",
            Target = "somewhere.org"
        };

        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
    }
}
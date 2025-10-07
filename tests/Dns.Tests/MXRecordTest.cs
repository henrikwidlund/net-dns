using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class MXRecordTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new MXRecord
        {
            Name = "emanon.org",
            Preference = 10,
            Exchange = "mail.emanon.org"
        };

        var b = (MXRecord)new ResourceRecord().Read(a.ToByteArray());

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Preference.ShouldBe(b.Preference);
        a.Exchange.ShouldBe(b.Exchange);
    }

    [Fact]
    public void Roundtrip_Master()
    {
        var a = new MXRecord
        {
            Name = "emanon.org",
            Preference = 10,
            Exchange = "mail.emanon.org"
        };

        var b = (MXRecord)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Preference.ShouldBe(b.Preference);
        a.Exchange.ShouldBe(b.Exchange);
    }

    [Fact]
    public void Equality()
    {
        var a = new MXRecord
        {
            Name = "emanon.org",
            Preference = 10,
            Exchange = "mail.emanon.org"
        };
        
        var b = new MXRecord
        {
            Name = "emanon.org",
            Preference = 11,
            Exchange = "mailx.emanon.org"
        };
        
        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }
}
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class MXRecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new MXRecord
        {
            Name = "emanon.org",
            Preference = 10,
            Exchange = "mail.emanon.org"
        };

        var b = (MXRecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Preference).IsEqualTo(b.Preference);
        await Assert.That(a.Exchange).IsEqualTo(b.Exchange);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new MXRecord
        {
            Name = "emanon.org",
            Preference = 10,
            Exchange = "mail.emanon.org"
        };

        var b = (MXRecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Preference).IsEqualTo(b.Preference);
        await Assert.That(a.Exchange).IsEqualTo(b.Exchange);
    }

    [Test]
    public async Task Equality()
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
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }
}
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class RPRecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new RPRecord
        {
            Name = "emanon.org",
            Mailbox = "nowon.emanon.org"
        };

        var b = (RPRecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Mailbox).IsEqualTo(b.Mailbox);
        await Assert.That(a.TextName).IsEqualTo(b.TextName);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new RPRecord
        {
            Name = "emanon.org",
            Mailbox = "nowon.emanon.org"
        };

        var b = (RPRecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Mailbox).IsEqualTo(b.Mailbox);
        await Assert.That(a.TextName).IsEqualTo(b.TextName);
    }

    [Test]
    public async Task Equality()
    {
        var a = new RPRecord
        {
            Name = "emanon.org",
            Mailbox = "nowon.emanon.org"
        };

        var b = new RPRecord
        {
            Name = "emanon.org",
            Mailbox = "someone.emanon.org"
        };

        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }
}
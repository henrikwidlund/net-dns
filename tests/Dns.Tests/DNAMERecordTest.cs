using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class DNAMERecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new DNAMERecord
        {
            Name = "emanon.org",
            Target = "somewhere.else.org"
        };

        var b = (DNAMERecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Target).IsEqualTo(b.Target);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new DNAMERecord
        {
            Name = "emanon.org",
            Target = "somewhere.else.org"
        };

        var b = (DNAMERecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Target).IsEqualTo(b.Target);
    }

    [Test]
    public async Task Equality()
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
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }
}
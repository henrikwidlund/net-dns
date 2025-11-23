using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class AFSDBRecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new AFSDBRecord
        {
            Name = "emanon.org",
            Subtype = 1,
            Target = "afs.emanon.org"
        };

        var b = (AFSDBRecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Subtype).IsEqualTo(b.Subtype);
        await Assert.That(a.Target).IsEqualTo(b.Target);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new AFSDBRecord
        {
            Name = "emanon.org",
            Subtype = 1,
            Target = "afs.emanon.org"
        };

        var b = (AFSDBRecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Subtype).IsEqualTo(b.Subtype);
        await Assert.That(a.Target).IsEqualTo(b.Target);
    }

    [Test]
    public async Task Equality()
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
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }
}
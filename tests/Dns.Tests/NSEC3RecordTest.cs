using System;
using System.Threading.Tasks;
using Makaretu.Dns;
using SimpleBase;

namespace DnsTests;

public class NSEC3RecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new NSEC3Record
        {
            Name = "2t7b4g4vsa5smi47k61mv5bv1a22bojr.example",
            TTL = TimeSpan.FromDays(1),
            HashAlgorithm = DigestType.Sha1,
            Flags = NSEC3s.OptOut,
            Iterations = 12,
            Salt = [0xaa, 0xbb, 0xcc, 0xdd],
            NextHashedOwnerName = Base32.ExtendedHex.Decode("2vptu5timamqttgl4luu9kg21e0aor3s"),
            Types = { DnsType.A, DnsType.RRSIG }
        };

        var b = (NSEC3Record)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.HashAlgorithm).IsEqualTo(b.HashAlgorithm);
        await Assert.That(a.Flags).IsEqualTo(b.Flags);
        await Assert.That(a.Iterations).IsEqualTo(b.Iterations);
        await Assert.That(a.Salt).IsEquivalentTo(b.Salt!);
        await Assert.That(a.NextHashedOwnerName).IsEquivalentTo(b.NextHashedOwnerName!);
        await Assert.That(a.Types).IsEquivalentTo(b.Types);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new NSEC3Record
        {
            Name = "2t7b4g4vsa5smi47k61mv5bv1a22bojr.example",
            TTL = TimeSpan.FromDays(1),
            HashAlgorithm = DigestType.Sha1,
            Flags = NSEC3s.OptOut,
            Iterations = 12,
            Salt = [0xaa, 0xbb, 0xcc, 0xdd],
            NextHashedOwnerName = Base32.ExtendedHex.Decode("2vptu5timamqttgl4luu9kg21e0aor3s"),
            Types = { DnsType.A, DnsType.RRSIG }
        };

        var b = (NSEC3Record)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.HashAlgorithm).IsEqualTo(b.HashAlgorithm);
        await Assert.That(a.Flags).IsEqualTo(b.Flags);
        await Assert.That(a.Iterations).IsEqualTo(b.Iterations);
        await Assert.That(a.Salt).IsEquivalentTo(b.Salt!);
        await Assert.That(a.NextHashedOwnerName).IsEquivalentTo(b.NextHashedOwnerName!);
        await Assert.That(a.Types).IsEquivalentTo(b.Types);
    }
}
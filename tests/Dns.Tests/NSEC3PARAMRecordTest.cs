using System;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class NSEC3PARAMRecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new NSEC3PARAMRecord
        {
            Name = "example",
            TTL = TimeSpan.FromDays(1),
            HashAlgorithm = DigestType.Sha1,
            Flags = 1,
            Iterations = 12,
            Salt = [0xaa, 0xbb, 0xcc, 0xdd]
        };

        var b = (NSEC3PARAMRecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.HashAlgorithm).IsEqualTo(b.HashAlgorithm);
        await Assert.That(a.Flags).IsEqualTo(b.Flags);
        await Assert.That(a.Iterations).IsEqualTo(b.Iterations);
        await Assert.That(a.Salt).IsEquivalentTo(b.Salt!);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new NSEC3PARAMRecord
        {
            Name = "example",
            TTL = TimeSpan.FromDays(1),
            HashAlgorithm = DigestType.Sha1,
            Flags = 1,
            Iterations = 12,
            Salt = [0xaa, 0xbb, 0xcc, 0xdd]
        };

        var b = (NSEC3PARAMRecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.HashAlgorithm).IsEqualTo(b.HashAlgorithm);
        await Assert.That(a.Flags).IsEqualTo(b.Flags);
        await Assert.That(a.Iterations).IsEqualTo(b.Iterations);
        await Assert.That(a.Salt).IsEquivalentTo(b.Salt!);
    }

    [Test]
    public async Task Roundtrip_Master_NullSalt()
    {
        var a = new NSEC3PARAMRecord
        {
            Name = "example",
            TTL = TimeSpan.FromDays(1),
            HashAlgorithm = DigestType.Sha1,
            Flags = 1,
            Iterations = 12
        };

        var b = (NSEC3PARAMRecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.HashAlgorithm).IsEqualTo(b.HashAlgorithm);
        await Assert.That(a.Flags).IsEqualTo(b.Flags);
        await Assert.That(a.Iterations).IsEqualTo(b.Iterations);
        await Assert.That(b.Salt).IsNull();
    }

    [Test]
    public async Task Roundtrip_Master_EmptySalt()
    {
        var a = new NSEC3PARAMRecord
        {
            Name = "example",
            TTL = TimeSpan.FromDays(1),
            HashAlgorithm = DigestType.Sha1,
            Flags = 1,
            Iterations = 12,
            Salt = []
        };

        var b = (NSEC3PARAMRecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.HashAlgorithm).IsEqualTo(b.HashAlgorithm);
        await Assert.That(a.Flags).IsEqualTo(b.Flags);
        await Assert.That(a.Iterations).IsEqualTo(b.Iterations);
        await Assert.That(b.Salt).IsNull();
    }
}
using System;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class RRSIGRecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var now = new DateTime(2018, 8, 13, 23, 59, 59, DateTimeKind.Utc);
        var a = new RRSIGRecord
        {
            Name = "host.example.com",
            TTL = TimeSpan.FromDays(1),
            TypeCovered = DnsType.A,
            Algorithm = SecurityAlgorithm.RSASHA1,
            Labels = 3,
            OriginalTTL = TimeSpan.FromDays(2),
            SignatureExpiration = now.AddMinutes(15),
            SignatureInception = now,
            KeyTag = 2642,
            SignerName = "example.com",
            Signature = [1, 2, 3]
        };

        var b = (RRSIGRecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.TypeCovered).IsEqualTo(b.TypeCovered);
        await Assert.That(a.Algorithm).IsEqualTo(b.Algorithm);
        await Assert.That(a.Labels).IsEqualTo(b.Labels);
        await Assert.That(a.OriginalTTL).IsEqualTo(b.OriginalTTL);
        await Assert.That(a.SignatureExpiration.ToUniversalTime()).IsEqualTo(b.SignatureExpiration);
        await Assert.That(a.SignatureInception.ToUniversalTime()).IsEqualTo(b.SignatureInception);
        await Assert.That(a.KeyTag).IsEqualTo(b.KeyTag);
        await Assert.That(a.SignerName).IsEqualTo(b.SignerName);
        await Assert.That(a.Signature).IsEquivalentTo(b.Signature!);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var now = new DateTime(2018, 8, 13, 23, 59, 59, DateTimeKind.Utc);
        var a = new RRSIGRecord
        {
            Name = "host.example.com",
            TTL = TimeSpan.FromDays(1),
            TypeCovered = DnsType.A,
            Algorithm = SecurityAlgorithm.RSASHA1,
            Labels = 3,
            OriginalTTL = TimeSpan.FromDays(2),
            SignatureExpiration = now.AddMinutes(15),
            SignatureInception = now,
            KeyTag = 2642,
            SignerName = "example.com",
            Signature = [1, 2, 3]
        };

        var b = (RRSIGRecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.TypeCovered).IsEqualTo(b.TypeCovered);
        await Assert.That(a.Algorithm).IsEqualTo(b.Algorithm);
        await Assert.That(a.Labels).IsEqualTo(b.Labels);
        await Assert.That(a.OriginalTTL).IsEqualTo(b.OriginalTTL);
        await Assert.That(a.SignatureExpiration.ToUniversalTime()).IsEqualTo(b.SignatureExpiration);
        await Assert.That(a.SignatureInception.ToUniversalTime()).IsEqualTo(b.SignatureInception);
        await Assert.That(a.KeyTag).IsEqualTo(b.KeyTag);
        await Assert.That(a.SignerName).IsEqualTo(b.SignerName);
        await Assert.That(a.Signature).IsEquivalentTo(b.Signature!);
    }
}
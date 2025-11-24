using System;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class TKEYRecordTest
{
    [Test]
    public async Task Defaults()
    {
        var tsig = new TKEYRecord();

        await Assert.That(tsig.Type).IsEqualTo(DnsType.TKEY);
        await Assert.That(tsig.Class).IsEqualTo(DnsClass.ANY);
        await Assert.That(tsig.TTL).IsEqualTo(TimeSpan.Zero);
    }

    [Test]
    public async Task Roundtrip()
    {
        var now = new DateTime(2018, 8, 13, 23, 59, 59, DateTimeKind.Utc);
        var a = new TKEYRecord
        {
            Name = "host.example.com",
            Algorithm = TSIGRecord.HMACSHA1,
            Inception = now,
            Expiration = now.AddSeconds(15),
            Mode = KeyExchangeMode.DiffieHellman,
            Key = [1, 2, 3, 4],
            Error = MessageStatus.BadTime,
            OtherData = [5, 6]
        };

        var b = (TKEYRecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Algorithm).IsEqualTo(b.Algorithm);
        await Assert.That(a.Inception).IsEqualTo(b.Inception);
        await Assert.That(a.Expiration).IsEqualTo(b.Expiration);
        await Assert.That(a.Mode).IsEqualTo(b.Mode);
        await Assert.That(a.Key).IsEquivalentTo(b.Key!);
        await Assert.That(a.Error).IsEqualTo(b.Error);
        await Assert.That(a.OtherData).IsEquivalentTo(b.OtherData!);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var now = new DateTime(2018, 8, 13, 23, 59, 59, DateTimeKind.Utc);
        var a = new TKEYRecord
        {
            Name = "host.example.com",
            Algorithm = TSIGRecord.HMACSHA1,
            Inception = now,
            Expiration = now.AddSeconds(15),
            Mode = KeyExchangeMode.DiffieHellman,
            Key = [1, 2, 3, 4],
            Error = MessageStatus.BadTime,
            OtherData = [5, 6]
        };

        var b = (TKEYRecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Algorithm).IsEqualTo(b.Algorithm);
        await Assert.That(a.Inception).IsEqualTo(b.Inception);
        await Assert.That(a.Expiration).IsEqualTo(b.Expiration);
        await Assert.That(a.Mode).IsEqualTo(b.Mode);
        await Assert.That(a.Key).IsEquivalentTo(b.Key!);
        await Assert.That(a.Error).IsEqualTo(b.Error);
        await Assert.That(a.OtherData).IsEquivalentTo(b.OtherData!);
    }
}
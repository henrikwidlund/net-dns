using System;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class TSIGRecordTest
{
    [Test]
    public async Task Defaults()
    {
        var tsig = new TSIGRecord();

        await Assert.That(tsig.Type).IsEqualTo(DnsType.TSIG);
        await Assert.That(tsig.Class).IsEqualTo(DnsClass.ANY);
        await Assert.That(tsig.TTL).IsEqualTo(TimeSpan.Zero);
        await Assert.That(tsig.TimeSigned.Kind).IsEqualTo(DateTimeKind.Utc);
        await Assert.That(tsig.TimeSigned.Millisecond).IsEqualTo(0);
        await Assert.That(tsig.Fudge).IsEqualTo(TimeSpan.FromSeconds(300));
    }

    [Test]
    public async Task Roundtrip()
    {
        var a = new TSIGRecord
        {
            Name = "host.example.com",
            Algorithm = TSIGRecord.HMACMD5,
            TimeSigned = new DateTime(1997, 1, 21, 3, 4, 5, DateTimeKind.Utc),
            Fudge = TimeSpan.FromSeconds(ushort.MaxValue),
            MAC = [1, 2, 3, 4],
            OriginalMessageId = 0xfbad,
            Error = MessageStatus.BadTime,
            OtherData = [5, 6]
        };

        var b = (TSIGRecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Algorithm).IsEqualTo(b.Algorithm);
        await Assert.That(a.TimeSigned).IsEqualTo(b.TimeSigned);
        await Assert.That(a.Fudge).IsEqualTo(b.Fudge);
        await Assert.That(a.MAC).IsEquivalentTo(b.MAC!);
        await Assert.That(a.OriginalMessageId).IsEqualTo(b.OriginalMessageId);
        await Assert.That(a.Error).IsEqualTo(b.Error);
        await Assert.That(a.OtherData).IsEquivalentTo(b.OtherData!);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new TSIGRecord
        {
            Name = "host.example.com",
            Algorithm = TSIGRecord.HMACMD5,
            TimeSigned = new DateTime(1997, 1, 21, 3, 4, 5, DateTimeKind.Utc),
            Fudge = TimeSpan.FromSeconds(ushort.MaxValue),
            MAC = [1, 2, 3, 4],
            OriginalMessageId = 0xfbad,
            Error = MessageStatus.BadTime
        };

        var b = (TSIGRecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Algorithm).IsEqualTo(b.Algorithm);
        await Assert.That(a.TimeSigned).IsEqualTo(b.TimeSigned);
        await Assert.That(a.MAC).IsEquivalentTo(b.MAC!);
        await Assert.That(a.OriginalMessageId).IsEqualTo(b.OriginalMessageId);
        await Assert.That(a.Error).IsEqualTo(b.Error);
        await Assert.That(a.OtherData).IsEquivalentTo(b.OtherData!);
    }
}
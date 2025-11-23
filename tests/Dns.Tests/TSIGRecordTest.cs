using System;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class TSIGRecordTest
{
    [Test]
    public void Defaults()
    {
        var tsig = new TSIGRecord();

        tsig.Type.ShouldBe(DnsType.TSIG);
        tsig.Class.ShouldBe(DnsClass.ANY);
        tsig.TTL.ShouldBe(TimeSpan.Zero);
        tsig.TimeSigned!.Kind.ShouldBe(DateTimeKind.Utc);
        tsig.TimeSigned!.Millisecond.ShouldBe(0);
        tsig.Fudge.ShouldBe(TimeSpan.FromSeconds(300));
    }

    [Test]
    public void Roundtrip()
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

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Algorithm.ShouldBe(b.Algorithm);
        a.TimeSigned.ShouldBe(b.TimeSigned);
        a.Fudge.ShouldBe(b.Fudge);
        a.MAC.ShouldBe(b.MAC);
        a.OriginalMessageId.ShouldBe(b.OriginalMessageId);
        a.Error.ShouldBe(b.Error);
        a.OtherData.ShouldBe(b.OtherData);
    }

    [Test]
    public void Roundtrip_Master()
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

        var b = (TSIGRecord)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Algorithm.ShouldBe(b.Algorithm);
        a.TimeSigned.ShouldBe(b.TimeSigned);
        a.MAC.ShouldBe(b.MAC);
        a.OriginalMessageId.ShouldBe(b.OriginalMessageId);
        a.Error.ShouldBe(b.Error);
        a.OtherData.ShouldBe(b.OtherData);
    }
}
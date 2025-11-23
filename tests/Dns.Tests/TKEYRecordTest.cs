using System;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class TKEYRecordTest
{
    [Test]
    public void Defaults()
    {
        var tsig = new TKEYRecord();

        tsig.Type.ShouldBe(DnsType.TKEY);
        tsig.Class.ShouldBe(DnsClass.ANY);
        tsig.TTL.ShouldBe(TimeSpan.Zero);
    }

    [Test]
    public void Roundtrip()
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

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Algorithm.ShouldBe(b.Algorithm);
        a.Inception.ShouldBe(b.Inception);
        a.Expiration.ShouldBe(b.Expiration);
        a.Mode.ShouldBe(b.Mode);
        a.Key.ShouldBe(b.Key);
        a.Error.ShouldBe(b.Error);
        a.OtherData.ShouldBe(b.OtherData);
    }

    [Test]
    public void Roundtrip_Master()
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

        var b = (TKEYRecord)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Algorithm.ShouldBe(b.Algorithm);
        a.Inception.ShouldBe(b.Inception);
        a.Expiration.ShouldBe(b.Expiration);
        a.Mode.ShouldBe(b.Mode);
        a.Key.ShouldBe(b.Key);
        a.Error.ShouldBe(b.Error);
        a.OtherData.ShouldBe(b.OtherData);
    }
}
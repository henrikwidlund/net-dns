using System;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class RRSIGRecordTest
{
    [Fact]
    public void Roundtrip()
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

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.TypeCovered.ShouldBe(b.TypeCovered);
        a.Algorithm.ShouldBe(b.Algorithm);
        a.Labels.ShouldBe(b.Labels);
        a.OriginalTTL.ShouldBe(b.OriginalTTL);
        a.SignatureExpiration.ToUniversalTime().ShouldBe(b.SignatureExpiration);
        a.SignatureInception.ToUniversalTime().ShouldBe(b.SignatureInception);
        a.KeyTag.ShouldBe(b.KeyTag);
        a.SignerName.ShouldBe(b.SignerName);
        a.Signature.ShouldBe(b.Signature);
    }

    [Fact]
    public void Roundtrip_Master()
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

        var b = (RRSIGRecord)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.TypeCovered.ShouldBe(b.TypeCovered);
        a.Algorithm.ShouldBe(b.Algorithm);
        a.Labels.ShouldBe(b.Labels);
        a.OriginalTTL.ShouldBe(b.OriginalTTL);
        a.SignatureExpiration.ToUniversalTime().ShouldBe(b.SignatureExpiration);
        a.SignatureInception.ToUniversalTime().ShouldBe(b.SignatureInception);
        a.KeyTag.ShouldBe(b.KeyTag);
        a.SignerName.ShouldBe(b.SignerName);
        a.Signature.ShouldBe(b.Signature);
    }
}
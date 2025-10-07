using System;
using Makaretu.Dns;
using Shouldly;
using Xunit;
using SimpleBase;

namespace DnsTests;

public class NSEC3RecordTest
{
    [Fact]
    public void Roundtrip()
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

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.HashAlgorithm.ShouldBe(b.HashAlgorithm);
        a.Flags.ShouldBe(b.Flags);
        a.Iterations.ShouldBe(b.Iterations);
        a.Salt.ShouldBe(b.Salt);
        a.NextHashedOwnerName.ShouldBe(b.NextHashedOwnerName);
        a.Types.ShouldBe(b.Types);
    }

    [Fact]
    public void Roundtrip_Master()
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

        var b = (NSEC3Record)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.HashAlgorithm.ShouldBe(b.HashAlgorithm);
        a.Flags.ShouldBe(b.Flags);
        a.Iterations.ShouldBe(b.Iterations);
        a.Salt.ShouldBe(b.Salt);
        a.NextHashedOwnerName.ShouldBe(b.NextHashedOwnerName);
        a.Types.ShouldBe(b.Types);
    }
}
using System;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class NSEC3PARAMRecordTest
{
    [Test]
    public void Roundtrip()
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

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.HashAlgorithm.ShouldBe(b.HashAlgorithm);
        a.Flags.ShouldBe(b.Flags);
        a.Iterations.ShouldBe(b.Iterations);
        a.Salt.ShouldBe(b.Salt);
    }

    [Test]
    public void Roundtrip_Master()
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

        var b = (NSEC3PARAMRecord)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.HashAlgorithm.ShouldBe(b.HashAlgorithm);
        a.Flags.ShouldBe(b.Flags);
        a.Iterations.ShouldBe(b.Iterations);
        a.Salt.ShouldBe(b.Salt);
    }

    [Test]
    public void Roundtrip_Master_NullSalt()
    {
        var a = new NSEC3PARAMRecord
        {
            Name = "example",
            TTL = TimeSpan.FromDays(1),
            HashAlgorithm = DigestType.Sha1,
            Flags = 1,
            Iterations = 12
        };

        var b = (NSEC3PARAMRecord)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.HashAlgorithm.ShouldBe(b.HashAlgorithm);
        a.Flags.ShouldBe(b.Flags);
        a.Iterations.ShouldBe(b.Iterations);
        b.Salt.ShouldBeNull();
    }

    [Test]
    public void Roundtrip_Master_EmptySalt()
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

        var b = (NSEC3PARAMRecord)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.HashAlgorithm.ShouldBe(b.HashAlgorithm);
        a.Flags.ShouldBe(b.Flags);
        a.Iterations.ShouldBe(b.Iterations);
        b.Salt.ShouldBeNull();
    }
}
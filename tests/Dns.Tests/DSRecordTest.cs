using System;
using Makaretu.Dns;
using Shouldly;
using Xunit;
using SimpleBase;

namespace DnsTests;

public class DSRecordTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new DSRecord
        {
            Name = "dskey.example.com",
            TTL = TimeSpan.FromSeconds(86400),
            KeyTag = 60485,
            Algorithm = SecurityAlgorithm.RSASHA1,
            HashAlgorithm = DigestType.Sha1,
            Digest = Base16.Decode("2BB183AF5F22588179A53B0A98631FAD1A292118").ToArray()
        };
        
        var b = (DSRecord)new ResourceRecord().Read(a.ToByteArray());
        
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.KeyTag.ShouldBe(b.KeyTag);
        a.Algorithm.ShouldBe(b.Algorithm);
        a.HashAlgorithm.ShouldBe(b.HashAlgorithm);
        a.Digest.ShouldBe(b.Digest);
    }

    [Fact]
    public void Roundtrip_Master()
    {
        var a = new DSRecord
        {
            Name = "dskey.example.com",
            TTL = TimeSpan.FromSeconds(86400),
            KeyTag = 60485,
            Algorithm = SecurityAlgorithm.RSASHA1,
            HashAlgorithm = DigestType.Sha1,
            Digest = Base16.Decode("2BB183AF5F22588179A53B0A98631FAD1A292118").ToArray()
        };
        
        var b = (DSRecord)new ResourceRecord().Read(a.ToString());
        
        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.KeyTag.ShouldBe(b.KeyTag);
        a.Algorithm.ShouldBe(b.Algorithm);
        a.HashAlgorithm.ShouldBe(b.HashAlgorithm);
        a.Digest.ShouldBe(b.Digest);
    }

    [Fact]
    public void FromDNSKEY()
    {
        // From https://tools.ietf.org/html/rfc4034#section-5.4
        var key = new DNSKEYRecord
        {
            Name = "dskey.example.com",
            TTL = TimeSpan.FromSeconds(86400),
            Flags = DnsKeys.ZoneKey,
            Algorithm = SecurityAlgorithm.RSASHA1,
            PublicKey = Convert.FromBase64String(
                """
                    AQOeiiR0GOMYkDshWoSKz9Xz
                    fwJr1AYtsmx3TGkJaNXVbfi/
                    2pHm822aJ5iI9BMzNXxeYCmZ
                    DRD99WYwYqUSdjMmmAphXdvx
                    egXd/M5+X7OrzKBaMbCVdFLU
                    Uh6DhweJBjEVv5f2wwjM9Xzc
                    nOf+EPbtG9DMBmADjFDc2w/r
                    ljwvFw==
                """)
        };
        
        var ds = new DSRecord(key, force: true);
        
        key.Name.ShouldBe(ds.Name);
        key.Class.ShouldBe(ds.Class);
        ds.Type.ShouldBe(DnsType.DS);
        key.TTL.ShouldBe(ds.TTL);
        ds.KeyTag!.Value.ShouldBe((ushort)60485);
        ds.Algorithm.ShouldBe(SecurityAlgorithm.RSASHA1);
        ds.HashAlgorithm.ShouldBe(DigestType.Sha1);
        ds.Digest.ShouldBe(Base16.Decode("2BB183AF5F22588179A53B0A98631FAD1A292118").ToArray());
    }

    [Fact]
    public void FromDNSKEY_Missing_ZK()
    {
        var key = new DNSKEYRecord
        {
            Name = "example.com",
            Flags = DnsKeys.SecureEntryPoint,
            Algorithm = SecurityAlgorithm.RSASHA1,
            PublicKey = Convert.FromBase64String(
                """
                    AQOeiiR0GOMYkDshWoSKz9Xz
                    fwJr1AYtsmx3TGkJaNXVbfi/
                    2pHm822aJ5iI9BMzNXxeYCmZ
                    DRD99WYwYqUSdjMmmAphXdvx
                    egXd/M5+X7OrzKBaMbCVdFLU
                    Uh6DhweJBjEVv5f2wwjM9Xzc
                    nOf+EPbtG9DMBmADjFDc2w/r
                    ljwvFw==
                """)
        };
        
        ExceptionAssert.Throws<ArgumentException>(() =>
        {
            _ = new DSRecord(key);
        });
    }

    [Fact]
    public void FromDNSKEY_Missing_SEP()
    {
        var key = new DNSKEYRecord
        {
            Name = "example.com",
            Flags = DnsKeys.ZoneKey,
            Algorithm = SecurityAlgorithm.RSASHA1,
            PublicKey = Convert.FromBase64String(
                """
                    AQOeiiR0GOMYkDshWoSKz9Xz
                    fwJr1AYtsmx3TGkJaNXVbfi/
                    2pHm822aJ5iI9BMzNXxeYCmZ
                    DRD99WYwYqUSdjMmmAphXdvx
                    egXd/M5+X7OrzKBaMbCVdFLU
                    Uh6DhweJBjEVv5f2wwjM9Xzc
                    nOf+EPbtG9DMBmADjFDc2w/r
                    ljwvFw==
                """)
        };
        
        ExceptionAssert.Throws<ArgumentException>(() =>
        {
            _ = new DSRecord(key);
        });
    }
}
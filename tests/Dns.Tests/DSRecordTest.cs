using System;
using System.Threading.Tasks;

using Makaretu.Dns;

using SimpleBase;

namespace DnsTests;

public class DSRecordTest
{
    [Test]
    public async Task Roundtrip()
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

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.KeyTag).IsEqualTo(b.KeyTag);
        await Assert.That(a.Algorithm).IsEqualTo(b.Algorithm);
        await Assert.That(a.HashAlgorithm).IsEqualTo(b.HashAlgorithm);
        await Assert.That(a.Digest).IsEquivalentTo(b.Digest!);
    }

    [Test]
    public async Task Roundtrip_Master()
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
        
        var b = (DSRecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.KeyTag).IsEqualTo(b.KeyTag);
        await Assert.That(a.Algorithm).IsEqualTo(b.Algorithm);
        await Assert.That(a.HashAlgorithm).IsEqualTo(b.HashAlgorithm);
        await Assert.That(a.Digest).IsEquivalentTo(b.Digest!);
    }

    [Test]
    public async Task FromDNSKEY()
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

        await Assert.That(key.Name).IsEqualTo(ds.Name);
        await Assert.That(key.Class).IsEqualTo(ds.Class);
        await Assert.That(ds.Type).IsEqualTo(DnsType.DS);
        await Assert.That(key.TTL).IsEqualTo(ds.TTL);
        await Assert.That(ds.KeyTag).IsEqualTo((ushort)60485);
        await Assert.That(ds.Algorithm).IsEqualTo(SecurityAlgorithm.RSASHA1);
        await Assert.That(ds.HashAlgorithm).IsEqualTo(DigestType.Sha1);
        await Assert.That(ds.Digest).IsEquivalentTo(Base16.Decode("2BB183AF5F22588179A53B0A98631FAD1A292118").ToArray());
    }

    [Test]
    public async Task FromDNSKEY_Missing_ZK()
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
        
        await ExceptionAssert.Throws<ArgumentException>(() =>
        {
            _ = new DSRecord(key);
        });
    }

    [Test]
    public async Task FromDNSKEY_Missing_SEP()
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
        
        await ExceptionAssert.Throws<ArgumentException>(() =>
        {
            _ = new DSRecord(key);
        });
    }
}
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class DNSKEYRecordTest
{
    private static readonly byte[] Key = Convert.FromBase64String("AQPSKmynfzW4kyBv015MUG2DeIQ3Cbl+BBZH4b/0PY1kxkmvHjcZc8nokfzj31GajIQKY+5CptLr3buXA10hWqTkF7H6RfoRqXQeogmMHfpftf6zMv1LyBUgia7za6ZEzOJBOztyvhjL742iU/TpPSEDhm2SNKLijfUppn1UaNvv4w==");

    [Test]
    public async Task Roundtrip()
    {
        var a = new DNSKEYRecord
        {
            Name = "example.com",
            TTL = TimeSpan.FromDays(2),
            Flags = DnsKeys.ZoneKey,
            Protocol = 3,
            Algorithm = SecurityAlgorithm.RSASHA1,
            PublicKey = Key
        };

        var b = (DNSKEYRecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Flags).IsEqualTo(b.Flags);
        await Assert.That(a.Protocol).IsEqualTo(b.Protocol);
        await Assert.That(a.Algorithm).IsEqualTo(b.Algorithm);
        await Assert.That(a.PublicKey).IsEquivalentTo(b.PublicKey!);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new DNSKEYRecord
        {
            Name = "example.com",
            TTL = TimeSpan.FromDays(2),
            Flags = DnsKeys.ZoneKey,
            Protocol = 3,
            Algorithm = SecurityAlgorithm.RSASHA1,
            PublicKey = Key
        };

        var b = (DNSKEYRecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Flags).IsEqualTo(b.Flags);
        await Assert.That(a.Protocol).IsEqualTo(b.Protocol);
        await Assert.That(a.Algorithm).IsEqualTo(b.Algorithm);
        await Assert.That(a.PublicKey).IsEquivalentTo(b.PublicKey!);
    }

    [Test]
    public async Task KeyTag()
    {
        // From https://tools.ietf.org/html/rfc4034#section-5.4
        var a = new DNSKEYRecord
        {
            Name = "example.com",
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

        await Assert.That(a.KeyTag()).IsEqualTo((ushort)60485);
    }

    [Test]
    public async Task FromRsaSha256()
    {
        // From https://tools.ietf.org/html/rfc5702#section-6.1
        var modulus = Convert.FromBase64String("wVwaxrHF2CK64aYKRUibLiH30KpPuPBjel7E8ZydQW1HYWHfoGmidzC2RnhwCC293hCzw+TFR2nqn8OVSY5t2Q==");
        var publicExponent = Convert.FromBase64String("AQAB");
        var dnsPublicKey = Convert.FromBase64String("AwEAAcFcGsaxxdgiuuGmCkVImy4h99CqT7jwY3pexPGcnUFtR2Fh36BponcwtkZ4cAgtvd4Qs8PkxUdp6p/DlUmObdk=");

        var parameters = new RSAParameters
        {
            Exponent = publicExponent,
            Modulus = modulus
        };
        
        using var publicKey = RSA.Create();
        publicKey.ImportParameters(parameters);

        var dnskey = new DNSKEYRecord(publicKey, SecurityAlgorithm.RSASHA256)
        {
            Flags = DnsKeys.ZoneKey
        };

        await Assert.That(dnskey.Flags).IsEqualTo(DnsKeys.ZoneKey);
        await Assert.That(dnskey.Protocol).IsEqualTo((byte)3);
        await Assert.That(dnskey.Algorithm).IsEqualTo(SecurityAlgorithm.RSASHA256);
        await Assert.That(dnskey.PublicKey).IsEquivalentTo(dnsPublicKey);
        await Assert.That(dnskey.KeyTag()).IsEqualTo((ushort)9033);
    }

    [Test]
    public async Task FromRsaSha256_BadAlgorithm()
    {
        // From https://tools.ietf.org/html/rfc5702#section-6.1
        var modulus = Convert.FromBase64String("wVwaxrHF2CK64aYKRUibLiH30KpPuPBjel7E8ZydQW1HYWHfoGmidzC2RnhwCC293hCzw+TFR2nqn8OVSY5t2Q==");
        var publicExponent = Convert.FromBase64String("AQAB");

        var parameters = new RSAParameters
        {
            Exponent = publicExponent,
            Modulus = modulus
        };
        
        var publicKey = RSA.Create();
        publicKey.ImportParameters(parameters);

        await Assert.That(() =>
        {
            _ = new DNSKEYRecord(publicKey, SecurityAlgorithm.ECDSAP256SHA256);
        }).ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task FromRsaSha512()
    {
        // From https://tools.ietf.org/html/rfc5702#section-6.2
        var modulus = Convert.FromBase64String("0eg1M5b563zoq4k5ZEOnWmd2/BvpjzedJVdfIsDcMuuhE5SQ3pfQ7qmdaeMlC6Nf8DKGoUPGPXe06cP27/WRODtxXquSUytkO0kJDk8KX8PtA0+yBWwy7UnZDyCkynO00Uuk8HPVtZeMO1pHtlAGVnc8VjXZlNKdyit99waaE4s=");
        var publicExponent = Convert.FromBase64String("AQAB");
        var dnsPublicKey = Convert.FromBase64String("AwEAAdHoNTOW+et86KuJOWRDp1pndvwb6Y83nSVXXyLA3DLroROUkN6X0O6pnWnjJQujX/AyhqFDxj13tOnD9u/1kTg7cV6rklMrZDtJCQ5PCl/D7QNPsgVsMu1J2Q8gpMpztNFLpPBz1bWXjDtaR7ZQBlZ3PFY12ZTSncorffcGmhOL");

        var parameters = new RSAParameters
        {
            Exponent = publicExponent,
            Modulus = modulus
        };

        var publicKey = RSA.Create();
        publicKey.ImportParameters(parameters);

        var dnskey = new DNSKEYRecord(publicKey, SecurityAlgorithm.RSASHA512)
        {
            Flags = DnsKeys.ZoneKey
        };

        await Assert.That(dnskey.Flags).IsEqualTo(DnsKeys.ZoneKey);
        await Assert.That(dnskey.Protocol).IsEqualTo((byte)3);
        await Assert.That(dnskey.Algorithm).IsEqualTo(SecurityAlgorithm.RSASHA512);
        await Assert.That(dnskey.PublicKey).IsEquivalentTo(dnsPublicKey);
        await Assert.That(dnskey.KeyTag()).IsEqualTo((ushort)3740);
    }

    [Test]
    public async Task FromECDsaP256()
    {
        // From https://tools.ietf.org/html/rfc6605#section-6.1
        var dnsPublicKey = Convert.FromBase64String("GojIhhXUN/u4v54ZQqGSnyhWJwaubCvTmeexv7bR6edbkrSqQpF64cYbcB7wNcP+e+MAnLr+Wi9xMWyQLc8NAA==");
        var qx = new byte[32];
        var qy = new byte[32];
        Array.Copy(dnsPublicKey, 0, qx, 0, 32);
        Array.Copy(dnsPublicKey, 32, qy, 0, 32);

        // Create the public key
        var parameters = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint
            {
                X = qx,
                Y = qy
            }
        };
        
        ECDsa publicKey;
        try
        {
            publicKey = ECDsa.Create(parameters);
        }
        catch (NotImplementedException)
        {
            return;
        }

        var dnskey = new DNSKEYRecord(publicKey)
        {
            Flags = DnsKeys.ZoneKey | DnsKeys.SecureEntryPoint
        };

        await Assert.That(dnskey.Flags).IsEqualTo(DnsKeys.ZoneKey | DnsKeys.SecureEntryPoint);
        await Assert.That(dnskey.Protocol).IsEqualTo((byte)3);
        await Assert.That(dnskey.Algorithm).IsEqualTo(SecurityAlgorithm.ECDSAP256SHA256);
        await Assert.That(dnskey.PublicKey).IsEquivalentTo(dnsPublicKey);
        await Assert.That(dnskey.KeyTag()).IsEqualTo((ushort)55648);
    }

    [Test]
    public async Task FromECDsaP384()
    {
        // From https://tools.ietf.org/html/rfc6605#section-6.2
        var dnsPublicKey = Convert.FromBase64String("xKYaNhWdGOfJ+nPrL8/arkwf2EY3MDJ+SErKivBVSum1w/egsXvSADtNJhyem5RCOpgQ6K8X1DRSEkrbYQ+OB+v8/uX45NBwY8rp65F6Glur8I/mlVNgF6W/qTI37m40");
        var qx = new byte[48];
        var qy = new byte[48];
        Array.Copy(dnsPublicKey, 0, qx, 0, 48);
        Array.Copy(dnsPublicKey, 48, qy, 0, 48);

        // Create the public key
        var parameters = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP384,
            Q = new ECPoint
            {
                X = qx,
                Y = qy
            }
        };
        
        ECDsa publicKey;
        try
        {
            publicKey = ECDsa.Create(parameters);
        }
        catch (NotImplementedException)
        {
            return;
        }

        var dnskey = new DNSKEYRecord(publicKey)
        {
            Flags = DnsKeys.ZoneKey | DnsKeys.SecureEntryPoint
        };

        await Assert.That(dnskey.Flags).IsEqualTo(DnsKeys.ZoneKey | DnsKeys.SecureEntryPoint);
        await Assert.That(dnskey.Protocol).IsEqualTo((byte)3);
        await Assert.That(dnskey.Algorithm).IsEqualTo(SecurityAlgorithm.ECDSAP384SHA384);
        await Assert.That(dnskey.PublicKey).IsEquivalentTo(dnsPublicKey);
        await Assert.That(dnskey.KeyTag()).IsEqualTo((ushort)10771);
    }
}
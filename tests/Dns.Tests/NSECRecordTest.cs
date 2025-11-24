using System;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class NSECRecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new NSECRecord
        {
            Name = "alfa.example.com",
            TTL = TimeSpan.FromDays(1),
            NextOwnerName = "host.example.com",
            Types = { DnsType.A, DnsType.MX, DnsType.RRSIG, DnsType.NSEC, (DnsType)1234 }
        };
        
        var b = (NSECRecord)new ResourceRecord().Read(a.ToByteArray());
        
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.NextOwnerName).IsEqualTo(b.NextOwnerName);
        await Assert.That(a.Types).IsEquivalentTo(b.Types);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new NSECRecord
        {
            Name = "alfa.example.com",
            TTL = TimeSpan.FromDays(1),
            NextOwnerName = "host.example.com",
            Types = { DnsType.A, DnsType.MX, DnsType.RRSIG, DnsType.NSEC, (DnsType)1234 }
        };
        
        var b = (NSECRecord)new ResourceRecord().Read(a.ToString())!;
        
        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.NextOwnerName).IsEqualTo(b.NextOwnerName);
        await Assert.That(a.Types).IsEquivalentTo(b.Types);
    }
}
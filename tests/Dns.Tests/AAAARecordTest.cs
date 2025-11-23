using System.Net;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class AAAARecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new AAAARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb")
        };
        var b = (AAAARecord)new ResourceRecord().Read(a.ToByteArray());
        
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Address).IsEqualTo(b.Address);
    }

    [Test]
    public async Task Roundtrip_ScopeId()
    {
        var a = new AAAARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("fe80::7573:b0a8:46b0:bfea%17")
        };
        var b = (AAAARecord)new ResourceRecord().Read(a.ToByteArray());
        
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(IPAddress.Parse("fe80::7573:b0a8:46b0:bfea")).IsEqualTo(b.Address);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new AAAARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb")
        };
        var b = (AAAARecord)new ResourceRecord().Read(a.ToString())!;
        
        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Address).IsEqualTo(b.Address);
    }

    [Test]
    public async Task Equality()
    {
        var a = new AAAARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb")
        };
        var b = new AAAARecord
        {
            Name = "emanon.org",
            Address = IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25ce")
        };
        
        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }
}
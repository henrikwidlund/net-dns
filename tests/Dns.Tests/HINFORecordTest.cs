using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class HINFORecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new HINFORecord
        {
            Name = "emanaon.org",
            Cpu = "DEC-2020",
            OS = "TOPS20"
        };
        
        var b = (HINFORecord)new ResourceRecord().Read(a.ToByteArray());
        
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Cpu).IsEqualTo(b.Cpu);
        await Assert.That(a.OS).IsEqualTo(b.OS);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new HINFORecord
        {
            Name = "emanaon.org",
            Cpu = "DEC-2020",
            OS = "TOPS20"
        };
        
        var b = (HINFORecord)new ResourceRecord().Read(a.ToString())!;
        
        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Cpu).IsEqualTo(b.Cpu);
        await Assert.That(a.OS).IsEqualTo(b.OS);
    }

    [Test]
    public async Task Equality()
    {
        var a = new HINFORecord
        {
            Name = "emanaon.org",
            Cpu = "DEC-2020",
            OS = "TOPS20"
        };
        
        var b = new HINFORecord
        {
            Name = "emanaon.org",
            Cpu = "DEC-2040",
            OS = "TOPS20"
        };

        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }
}
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class SRVRecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new SRVRecord
        {
            Name = "_foobar._tcp",
            Priority = 1,
            Weight = 2,
            Port = 9,
            Target = "foobar.example.com"
        };
        
        var b = (SRVRecord)new ResourceRecord().Read(a.ToByteArray());
        
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Priority).IsEqualTo(b.Priority);
        await Assert.That(a.Weight).IsEqualTo(b.Weight);
        await Assert.That(a.Port).IsEqualTo(b.Port);
        await Assert.That(a.Target).IsEqualTo(b.Target);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new SRVRecord
        {
            Name = "_foobar._tcp",
            Priority = 1,
            Weight = 2,
            Port = 9,
            Target = "foobar.example.com"
        };
        
        var b = (SRVRecord)new ResourceRecord().Read(a.ToString())!;
        
        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Priority).IsEqualTo(b.Priority);
        await Assert.That(a.Weight).IsEqualTo(b.Weight);
        await Assert.That(a.Port).IsEqualTo(b.Port);
        await Assert.That(a.Target).IsEqualTo(b.Target);
    }

    [Test]
    public async Task Equality()
    {
        var a = new SRVRecord
        {
            Name = "_foobar._tcp",
            Priority = 1,
            Weight = 2,
            Port = 9,
            Target = "foobar.example.com"
        };
        
        var b = new SRVRecord
        {
            Name = "_foobar._tcp",
            Priority = 1,
            Weight = 2,
            Port = 9,
            Target = "foobar-x.example.com"
        };

        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }
}
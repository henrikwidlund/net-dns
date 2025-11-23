using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class NULLRecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new NULLRecord
        {
            Name = "emanon.org",
            Data = [1, 2, 3, 4]
        };
        
        var b = (NULLRecord)new ResourceRecord().Read(a.ToByteArray());
        
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Data).IsEquivalentTo(b.Data!);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new NULLRecord
        {
            Name = "emanon.org",
            Data = [1, 2, 3, 4]
        };
        
        var b = (NULLRecord)new ResourceRecord().Read(a.ToString())!;
        
        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Data).IsEquivalentTo(b.Data!);
    }

    [Test]
    public async Task Equality()
    {
        var a = new NULLRecord
        {
            Name = "emanon.org",
            Data = [1, 2, 3, 4]
        };
        
        var b = new NULLRecord
        {
            Name = "emanon.org",
            Data = [1, 2, 3, 40]
        };
        
        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }
}
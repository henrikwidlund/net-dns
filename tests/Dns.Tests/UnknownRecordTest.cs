using System.Threading.Tasks;

using Makaretu.Dns;

namespace DnsTests;

public class UnknownRecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new UnknownRecord
        {
            Name = "emanon.org",
            Data = [10, 11, 12]
        };
        
        var b = (UnknownRecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Data).IsEquivalentTo(b.Data!);
    }

    [Test]
    public async Task Equality()
    {
        var a = new UnknownRecord
        {
            Name = "emanon.org",
            Data = [1, 2, 3, 4]
        };
        
        var b = new UnknownRecord
        {
            Name = "emanon.org",
            Data = [1, 2, 3, 40]
        };
        
        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new UnknownRecord
        {
            Name = "a.example",
            Class = (DnsClass)32,
            Type = (DnsType)731,
            Data = [0xab, 0xcd, 0xef, 0x01, 0x23, 0x45]
        };
        
        var b = (UnknownRecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Data).IsEquivalentTo(b.Data!);
    }
}
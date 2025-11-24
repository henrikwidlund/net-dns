using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class TXTRecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new TXTRecord
        {
            Name = "the.printer.local",
            Strings =
            [
                "paper=A4",
                "colour=false"
            ]
        };
        
        var b = (TXTRecord)new ResourceRecord().Read(a.ToByteArray());
        
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Strings).IsEquivalentTo(b.Strings);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new TXTRecord
        {
            Name = "the.printer.local",
            Strings =
            [
                "paper=A4",
                "colour=false",
                "foo1=a b",
                @"foo2=a\b",
                "foo3=a\""
            ]
        };
        
        var b = (TXTRecord)new ResourceRecord().Read(a.ToString())!;
        
        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Strings).IsEquivalentTo(b.Strings);
    }

    [Test]
    public async Task NoStrings()
    {
        var a = new TXTRecord
        {
            Name = "the.printer.local"
        };
        
        var b = (TXTRecord)new ResourceRecord().Read(a.ToByteArray());
        
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Strings).IsEquivalentTo(b.Strings);
    }

    [Test]
    public async Task Equality()
    {
        var a = new TXTRecord
        {
            Name = "the.printer.local",
            Strings =
            [
                "paper=A4",
                "colour=false"
            ]
        };
        
        var b = new TXTRecord
        {
            Name = "the.printer.local",
            Strings =
            [
                "paper=A4",
                "colour=true"
            ]
        };

        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
        await Assert.That(a!.GetHashCode()).IsNotEqualTo(new TXTRecord().GetHashCode());
    }
}
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class PTRRecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new PTRRecord
        {
            Name = "emanon.org",
            DomainName = "somewhere.else.org"
        };
        
        var b = (PTRRecord)new ResourceRecord().Read(a.ToByteArray());
        
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.DomainName).IsEqualTo(b.DomainName);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new PTRRecord
        {
            Name = "emanon.org",
            DomainName = "somewhere.else.org"
        };
        
        var b = (PTRRecord)new ResourceRecord().Read(a.ToString())!;
        
        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.DomainName).IsEqualTo(b.DomainName);
    }

    [Test]
    public async Task Equality()
    {
        var a = new PTRRecord
        {
            Name = "emanon.org",
            DomainName = "somewhere.else.org"
        };
        
        var b = new PTRRecord
        {
            Name = "emanon.org",
            DomainName = "somewhere.org"
        };

        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }
}
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class NSRecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new NSRecord
        {
            Name = "emanon.org",
            Authority = "mydomain.name"
        };

        var b = (NSRecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Authority).IsEqualTo(b.Authority);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new NSRecord
        {
            Name = "emanon.org",
            Authority = "mydomain.name"
        };

        var b = (NSRecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.Authority).IsEqualTo(b.Authority);
    }

    [Test]
    public async Task Equality()
    {
        var a = new NSRecord
        {
            Name = "emanon.org",
            Authority = "mydomain.name"
        };

        var b = new NSRecord
        {
            Name = "emanon.org",
            Authority = "mydomainx.name"
        };

        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }
}
using System;
using System.Net;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class UpdateResourceListTest
{
    [Test]
    public async Task AddResource()
    {
        var rr = new ARecord
        {
            Name = "local",
            Class = DnsClass.IN,
            Address = IPAddress.Parse("127.0.0.0")
        };

        var updates = new UpdateResourceList()
            .AddResource(rr);
        await Assert.That(updates).Count().IsEqualTo(1);
        var p = updates[0];

        await Assert.That(p).IsNotNull();
        await Assert.That(p.Class).IsEqualTo(rr.Class);
        await Assert.That(p.Name).IsEqualTo(rr.Name);
        await Assert.That(p.TTL).IsEqualTo(rr.TTL);
        await Assert.That(p.Type).IsEqualTo(rr.Type);
        await Assert.That(p.GetDataLength()).IsEqualTo(rr.GetDataLength());
        await Assert.That(rr.GetData().SequenceEqual(p.GetData())).IsTrue();
    }

    [Test]
    public async Task DeleteResource_Name()
    {
        var updates = new UpdateResourceList()
            .DeleteResource("www.example.org");
        await Assert.That(updates).Count().IsEqualTo(1);
        var p = updates[0];

        await Assert.That(p).IsNotNull();
        await Assert.That(p.Class).IsEqualTo(DnsClass.ANY);
        await Assert.That(p.Name).IsEquatableOrEqualTo("www.example.org");
        await Assert.That(p.TTL).IsEqualTo(TimeSpan.Zero);
        await Assert.That(p.Type).IsEqualTo(DnsType.ANY);
        await Assert.That(p.GetDataLength()).IsEqualTo(0);
    }

    [Test]
    public async Task DeleteResource_Name_Type()
    {
        var updates = new UpdateResourceList()
            .DeleteResource("www.example.org", DnsType.A);
        await Assert.That(updates).Count().IsEqualTo(1);
        var p = updates[0];

        await Assert.That(p).IsNotNull();
        await Assert.That(p.Class).IsEqualTo(DnsClass.ANY);
        await Assert.That(p.Name).IsEquatableOrEqualTo("www.example.org");
        await Assert.That(p.TTL).IsEqualTo(TimeSpan.Zero);
        await Assert.That(p.Type).IsEqualTo(DnsType.A);
        await Assert.That(p.GetDataLength()).IsEqualTo(0);
    }

    [Test]
    public async Task DeleteResource_Name_Typename()
    {
        var updates = new UpdateResourceList()
            .DeleteResource<ARecord>("www.example.org");
        await Assert.That(updates).Count().IsEqualTo(1);
        var p = updates[0];

        await Assert.That(p).IsNotNull();
        await Assert.That(p.Class).IsEqualTo(DnsClass.ANY);
        await Assert.That(p.Name).IsEquatableOrEqualTo("www.example.org");
        await Assert.That(p.TTL).IsEqualTo(TimeSpan.Zero);
        await Assert.That(p.Type).IsEqualTo(DnsType.A);
        await Assert.That(p.GetDataLength()).IsEqualTo(0);
    }

    [Test]
    public async Task DeleteResource()
    {
        var rr = new ARecord
        {
            Name = "local",
            Class = DnsClass.IN,
            Address = IPAddress.Parse("127.0.0.0")
        };

        var updates = new UpdateResourceList()
            .DeleteResource(rr);
        await Assert.That(updates).Count().IsEqualTo(1);
        var p = updates[0];

        await Assert.That(p).IsNotNull();
        await Assert.That(p.Class).IsEqualTo(DnsClass.None);
        await Assert.That(p.Name).IsEqualTo(rr.Name);
        await Assert.That(p.TTL).IsEqualTo(TimeSpan.Zero);
        await Assert.That(p.Type).IsEqualTo(rr.Type);
        await Assert.That(p.GetDataLength()).IsEqualTo(rr.GetDataLength());
        await Assert.That(rr.GetData().SequenceEqual(p.GetData())).IsTrue();
    }
}
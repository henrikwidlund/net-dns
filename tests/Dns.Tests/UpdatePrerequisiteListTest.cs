using System;
using System.Net;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class UpdatePrerequisiteListTest
{
    [Test]
    public async Task MustExist_Name()
    {
        var prerequisites = new UpdatePrerequisiteList()
            .MustExist("www.example.org");
        await Assert.That(prerequisites).HasCount(1);
        var p = prerequisites[0];

        await Assert.That(p).IsNotNull();
        await Assert.That(p.Class).IsEqualTo(DnsClass.ANY);
        await Assert.That(p.Name).IsEquatableOrEqualTo("www.example.org");
        await Assert.That(p.TTL).IsEqualTo(TimeSpan.Zero);
        await Assert.That(p.Type).IsEqualTo(DnsType.ANY);
        await Assert.That(p.GetDataLength()).IsEqualTo(0);
    }

    [Test]
    public async Task MustExist_Name_Type()
    {
        var prerequisites = new UpdatePrerequisiteList()
            .MustExist("www.example.org", DnsType.A);
        await Assert.That(prerequisites).HasCount(1);
        var p = prerequisites[0];

        await Assert.That(p).IsNotNull();
        await Assert.That(p.Class).IsEqualTo(DnsClass.ANY);
        await Assert.That(p.Name).IsEquatableOrEqualTo("www.example.org");
        await Assert.That(p.TTL).IsEqualTo(TimeSpan.Zero);
        await Assert.That(p.Type).IsEqualTo(DnsType.A);
        await Assert.That(p.GetDataLength()).IsEqualTo(0);
    }

    [Test]
    public async Task MustExist_Name_Typename()
    {
        var prerequisites = new UpdatePrerequisiteList()
            .MustExist<ARecord>("www.example.org");
        await Assert.That(prerequisites).HasCount(1);
        var p = prerequisites[0];

        await Assert.That(p).IsNotNull();
        await Assert.That(p.Class).IsEqualTo(DnsClass.ANY);
        await Assert.That(p.Name).IsEquatableOrEqualTo("www.example.org");
        await Assert.That(p.TTL).IsEqualTo(TimeSpan.Zero);
        await Assert.That(p.Type).IsEqualTo(DnsType.A);
        await Assert.That(p.GetDataLength()).IsEqualTo(0);
    }

    [Test]
    public async Task MustExist_ResourceRecord()
    {
        var rr = new ARecord
        {
            Name = "local",
            Class = DnsClass.IN,
            Address = IPAddress.Parse("127.0.0.0")
        };
        var prerequisites = new UpdatePrerequisiteList()
            .MustExist(rr);
        await Assert.That(prerequisites).HasCount(1);
        var p = prerequisites[0];

        await Assert.That(p).IsNotNull();
        await Assert.That(p.Class).IsEqualTo(rr.Class);
        await Assert.That(p.Name).IsEqualTo(rr.Name);
        await Assert.That(p.TTL).IsEqualTo(TimeSpan.Zero);
        await Assert.That(p.Type).IsEqualTo(rr.Type);
        await Assert.That(p.GetDataLength()).IsEqualTo(rr.GetDataLength());
        await Assert.That(rr.GetData().SequenceEqual(p.GetData())).IsTrue();
    }

    [Test]
    public async Task MustNotExist_Name()
    {
        var prerequisites = new UpdatePrerequisiteList()
            .MustNotExist("www.example.org");
        await Assert.That(prerequisites).HasCount(1);
        var p = prerequisites[0];

        await Assert.That(p).IsNotNull();
        await Assert.That(p.Class).IsEqualTo(DnsClass.None);
        await Assert.That(p.Name).IsEquatableOrEqualTo("www.example.org");
        await Assert.That(p.TTL).IsEqualTo(TimeSpan.Zero);
        await Assert.That(p.Type).IsEqualTo(DnsType.ANY);
        await Assert.That(p.GetDataLength()).IsEqualTo(0);
    }

    [Test]
    public async Task MustNotExist_Name_Type()
    {
        var prerequisites = new UpdatePrerequisiteList()
            .MustNotExist("www.example.org", DnsType.A);
        await Assert.That(prerequisites).HasCount(1);
        var p = prerequisites[0];

        await Assert.That(p).IsNotNull();
        await Assert.That(p.Class).IsEqualTo(DnsClass.None);
        await Assert.That(p.Name).IsEquatableOrEqualTo("www.example.org");
        await Assert.That(p.TTL).IsEqualTo(TimeSpan.Zero);
        await Assert.That(p.Type).IsEqualTo(DnsType.A);
        await Assert.That(p.GetDataLength()).IsEqualTo(0);
    }

    [Test]
    public async Task MustNotExist_Name_Typename()
    {
        var prerequisites = new UpdatePrerequisiteList()
            .MustNotExist<ARecord>("www.example.org");
        await Assert.That(prerequisites).HasCount(1);
        var p = prerequisites[0];

        await Assert.That(p).IsNotNull();
        await Assert.That(p.Class).IsEqualTo(DnsClass.None);
        await Assert.That(p.Name).IsEquatableOrEqualTo("www.example.org");
        await Assert.That(p.TTL).IsEqualTo(TimeSpan.Zero);
        await Assert.That(p.Type).IsEqualTo(DnsType.A);
        await Assert.That(p.GetDataLength()).IsEqualTo(0);
    }
}
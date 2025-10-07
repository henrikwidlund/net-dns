using System;
using System.Linq;
using System.Net;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class UpdateResourceListTest
{
    [Fact]
    public void AddResource()
    {
        var rr = new ARecord
        {
            Name = "local",
            Class = DnsClass.IN,
            Address = IPAddress.Parse("127.0.0.0")
        };

        var updates = new UpdateResourceList()
            .AddResource(rr);
        var p = updates[0];

        p.ShouldNotBeNull();
        p.Class.ShouldBe(rr.Class);
        p.Name.ShouldBe(rr.Name);
        p.TTL.ShouldBe(rr.TTL);
        p.Type.ShouldBe(rr.Type);
        p.GetDataLength().ShouldBe(rr.GetDataLength());
        rr.GetData().SequenceEqual(p.GetData()).ShouldBeTrue();
    }

    [Fact]
    public void DeleteResource_Name()
    {
        var updates = new UpdateResourceList()
            .DeleteResource("www.example.org");
        var p = updates[0];

        p.ShouldNotBeNull();
        p.Class.ShouldBe(DnsClass.ANY);
        p.Name.ShouldBe("www.example.org");
        p.TTL.ShouldBe(TimeSpan.Zero);
        p.Type.ShouldBe(DnsType.ANY);
        p.GetDataLength().ShouldBe(0);
    }

    [Fact]
    public void DeleteResource_Name_Type()
    {
        var updates = new UpdateResourceList()
            .DeleteResource("www.example.org", DnsType.A);
        var p = updates[0];

        p.ShouldNotBeNull();
        p.Class.ShouldBe(DnsClass.ANY);
        p.Name.ShouldBe("www.example.org");
        p.TTL.ShouldBe(TimeSpan.Zero);
        p.Type.ShouldBe(DnsType.A);
        p.GetDataLength().ShouldBe(0);
    }

    [Fact]
    public void DeleteResource_Name_Typename()
    {
        var updates = new UpdateResourceList()
            .DeleteResource<ARecord>("www.example.org");
        var p = updates[0];

        p.ShouldNotBeNull();
        p.Class.ShouldBe(DnsClass.ANY);
        p.Name.ShouldBe("www.example.org");
        p.TTL.ShouldBe(TimeSpan.Zero);
        p.Type.ShouldBe(DnsType.A);
        p.GetDataLength().ShouldBe(0);
    }

    [Fact]
    public void DeleteResource()
    {
        var rr = new ARecord
        {
            Name = "local",
            Class = DnsClass.IN,
            Address = IPAddress.Parse("127.0.0.0")
        };

        var updates = new UpdateResourceList()
            .DeleteResource(rr);
        var p = updates[0];

        p.ShouldNotBeNull();
        p.Class.ShouldBe(DnsClass.None);
        p.Name.ShouldBe(rr.Name);
        p.TTL.ShouldBe(TimeSpan.Zero);
        p.Type.ShouldBe(rr.Type);
        p.GetDataLength().ShouldBe(rr.GetDataLength());
        rr.GetData().SequenceEqual(p.GetData()).ShouldBeTrue();
    }
}
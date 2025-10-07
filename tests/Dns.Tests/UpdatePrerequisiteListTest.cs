using System;
using System.Linq;
using System.Net;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class UpdatePrerequisiteListTest
{
    [Fact]
    public void MustExist_Name()
    {
        var prerequisites = new UpdatePrerequisiteList()
            .MustExist("www.example.org");
        var p = prerequisites[0];

        p.ShouldNotBeNull();
        p.Class.ShouldBe(DnsClass.ANY);
        p.Name.ShouldBe("www.example.org");
        p.TTL.ShouldBe(TimeSpan.Zero);
        p.Type.ShouldBe(DnsType.ANY);
        p.GetDataLength().ShouldBe(0);
    }

    [Fact]
    public void MustExist_Name_Type()
    {
        var prerequisites = new UpdatePrerequisiteList()
            .MustExist("www.example.org", DnsType.A);
        var p = prerequisites[0];

        p.ShouldNotBeNull();
        p.Class.ShouldBe(DnsClass.ANY);
        p.Name.ShouldBe("www.example.org");
        p.TTL.ShouldBe(TimeSpan.Zero);
        p.Type.ShouldBe(DnsType.A);
        p.GetDataLength().ShouldBe(0);
    }

    [Fact]
    public void MustExist_Name_Typename()
    {
        var prerequisites = new UpdatePrerequisiteList()
            .MustExist<ARecord>("www.example.org");
        var p = prerequisites[0];

        p.ShouldNotBeNull();
        p.Class.ShouldBe(DnsClass.ANY);
        p.Name.ShouldBe("www.example.org");
        p.TTL.ShouldBe(TimeSpan.Zero);
        p.Type.ShouldBe(DnsType.A);
        p.GetDataLength().ShouldBe(0);
    }

    [Fact]
    public void MustExist_ResourceRecord()
    {
        var rr = new ARecord
        {
            Name = "local",
            Class = DnsClass.IN,
            Address = IPAddress.Parse("127.0.0.0")
        };
        var prerequisites = new UpdatePrerequisiteList()
            .MustExist(rr);
        var p = prerequisites[0];

        p.ShouldNotBeNull();
        p.Class.ShouldBe(rr.Class);
        p.Name.ShouldBe(rr.Name);
        p.TTL.ShouldBe(TimeSpan.Zero);
        p.Type.ShouldBe(rr.Type);
        p.GetDataLength().ShouldBe(rr.GetDataLength());
        rr.GetData().SequenceEqual(p.GetData()).ShouldBeTrue();
    }

    [Fact]
    public void MustNotExist_Name()
    {
        var prerequisites = new UpdatePrerequisiteList()
            .MustNotExist("www.example.org");
        var p = prerequisites[0];

        p.ShouldNotBeNull();
        p.Class.ShouldBe(DnsClass.None);
        p.Name.ShouldBe("www.example.org");
        p.TTL.ShouldBe(TimeSpan.Zero);
        p.Type.ShouldBe(DnsType.ANY);
        p.GetDataLength().ShouldBe(0);
    }

    [Fact]
    public void MustNotExist_Name_Type()
    {
        var prerequisites = new UpdatePrerequisiteList()
            .MustNotExist("www.example.org", DnsType.A);
        var p = prerequisites[0];

        p.ShouldNotBeNull();
        p.Class.ShouldBe(DnsClass.None);
        p.Name.ShouldBe("www.example.org");
        p.TTL.ShouldBe(TimeSpan.Zero);
        p.Type.ShouldBe(DnsType.A);
        p.GetDataLength().ShouldBe(0);
    }

    [Fact]
    public void MustNotExist_Name_Typename()
    {
        var prerequisites = new UpdatePrerequisiteList()
            .MustNotExist<ARecord>("www.example.org");
        var p = prerequisites[0];

        p.ShouldNotBeNull();
        p.Class.ShouldBe(DnsClass.None);
        p.Name.ShouldBe("www.example.org");
        p.TTL.ShouldBe(TimeSpan.Zero);
        p.Type.ShouldBe(DnsType.A);
        p.GetDataLength().ShouldBe(0);
    }

}
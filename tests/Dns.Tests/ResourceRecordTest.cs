using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class ResourceRecordTest
{
    [Fact]
    public void Defaults()
    {
        var rr = new ResourceRecord();

        rr.Class.ShouldBe(DnsClass.IN);
        rr.TTL.ShouldBe(ResourceRecord.DefaultTTL);
    }

    [Fact]
    public void DataLength()
    {
        var rr = new ResourceRecord();

        rr.GetDataLength().ShouldBe(0);
    }

    [Fact]
    public void DataLength_DerivedClass()
    {
        var a = new ARecord { Address = IPAddress.Parse("127.0.0.1") };

        a.GetDataLength().ShouldBe(4);
    }

    [Fact]
    public void Data()
    {
        var rr = new ResourceRecord();

        rr.GetData().Length.ShouldBe(0);
    }

    [Fact]
    public void Data_DerivedClass()
    {
        var a = new ARecord { Address = IPAddress.Parse("127.0.0.1") };

        a.GetData().Length.ShouldNotBe(0);
    }

    [Fact]
    public void RoundTrip()
    {
        var a = new ResourceRecord
        {
            Name = "emanon.org",
            Class = DnsClass.CH,
            Type = (DnsType)0xFFFF,
            TTL = TimeSpan.FromDays(2)
        };

        var b = (ResourceRecord)new ResourceRecord().Read(a.ToByteArray());

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.GetDataLength().ShouldBe(b.GetDataLength());
        a.GetHashCode().ShouldBe(b.GetHashCode());
        b.ShouldBeAssignableTo<ResourceRecord>();
    }

    [Fact]
    public void Value_Equality()
    {
        var a0 = new ResourceRecord
        {
            Name = "alpha",
            Class = DnsClass.IN,
            Type = DnsType.A,
            TTL = TimeSpan.FromSeconds(1)
        };

        var a1 = new ResourceRecord
        {
            Name = "alpha",
            Class = DnsClass.IN,
            Type = DnsType.A,
            TTL = TimeSpan.FromSeconds(2)
        };

        var b = new ResourceRecord
        {
            Name = "beta",
            Class = DnsClass.IN,
            Type = DnsType.A,
            TTL = TimeSpan.FromSeconds(1)
        };

        ResourceRecord c = null;
        ResourceRecord d = null;
        ResourceRecord e = new();

        // ReSharper disable ConditionIsAlwaysTrueOrFalse
        (c == d).ShouldBeTrue();
        (c == b).ShouldBeFalse();
        (b == c).ShouldBeFalse();


        (c != d).ShouldBeFalse();
        (c != b).ShouldBeTrue();
        (b != c).ShouldBeTrue();
        // ReSharper restore ConditionIsAlwaysTrueOrFalse

#pragma warning disable 1718
        // ReSharper disable once EqualExpressionComparison
        (a0 == a0).ShouldBeTrue();
        (a0 == a1).ShouldBeTrue();
        (a0 == b).ShouldBeFalse();

        // ReSharper disable once EqualExpressionComparison
        (a0 != a0).ShouldBeFalse();
        (a0 != a1).ShouldBeFalse();
        (a0 != b).ShouldBeTrue();

        // ReSharper disable once EqualExpressionComparison
        a0.Equals(a0).ShouldBeTrue();
        a0.Equals(a1).ShouldBeTrue();
        a0.Equals(b).ShouldBeFalse();

        a0.ShouldBe(a0);
        a0.ShouldBe(a1);
        a0.ShouldNotBe(b);

        e.ShouldBe(e);
        e.ShouldNotBe(a0);

        a0.GetHashCode().ShouldBe(a0.GetHashCode());
        a0.GetHashCode().ShouldBe(a1.GetHashCode());
        a0.GetHashCode().ShouldNotBe(b.GetHashCode());
        e.GetHashCode().ShouldBe(e.GetHashCode());
    }

    [Fact]
    public void Stringing()
    {
        var a = new ResourceRecord
        {
            Name = "x.emanon.org",
            Type = DnsType.A
        };
        a.ToString().ShouldBe("x.emanon.org IN A \\# 0");

        a = new ResourceRecord
        {
            Name = "x.emanon.org",
            Type = DnsType.A,
            Class = DnsClass.CH
        };
        a.ToString().ShouldBe("x.emanon.org CH A \\# 0");

        a = new ResourceRecord
        {
            Name = "x.emanon.org",
            Type = DnsType.A,
            TTL = TimeSpan.FromSeconds(123)
        };
        a.ToString().ShouldBe("x.emanon.org 123 IN A \\# 0");
    }

    [Fact]
    public async Task CreationTime()
    {
        var now = DateTime.Now;
        var rr = new ResourceRecord();
        rr.CreationTime.Kind.ShouldBe(DateTimeKind.Local);
        rr.CreationTime.ShouldBeGreaterThanOrEqualTo(now);

        await Task.Delay(50);
        var clone = rr.Clone<ResourceRecord>();
        rr.CreationTime.ShouldBe(clone.CreationTime);
    }

    [Fact]
    public void IsExpired()
    {
        var rr = new ResourceRecord { TTL = TimeSpan.FromSeconds(2) };

        rr.IsExpired().ShouldBeFalse();
        rr.IsExpired(DateTime.Now + TimeSpan.FromSeconds(-3)).ShouldBeFalse();
        rr.IsExpired(DateTime.Now + TimeSpan.FromSeconds(3)).ShouldBeTrue();
    }

    [Fact]
    public void Stringing_UnknownClass()
    {
        var a = new ResourceRecord
        {
            Name = "x.emanon.org",
            Class = (DnsClass)1234,
            Type = DnsType.A
        };

        a.ToString().ShouldBe("x.emanon.org CLASS1234 A \\# 0");
    }

    [Fact]
    public void Stringing_UnknownType()
    {
        var a = new ResourceRecord
        {
            Name = "x.emanon.org",
            Type = (DnsType)1234
        };

        a.ToString().ShouldBe("x.emanon.org IN TYPE1234 \\# 0");
    }

    [Fact]
    public void CanonicalName()
    {
        var rr = new ResourceRecord { Name = "x.EmAnOn.OrG" };

        rr.CanonicalName.ShouldBe("x.emanon.org");
    }

    [Fact]
    public void RDATA_Underflow()
    {
        var ms = new MemoryStream(Convert.FromBase64String("A2ZvbwAABQABAAFRgAAKB3Vua25vd24A/w=="))
        {
            Position = 0
        };

        Should.Throw<InvalidDataException>(() =>
        {
            _ = new ResourceRecord().Read(ms);
        });
    }
}
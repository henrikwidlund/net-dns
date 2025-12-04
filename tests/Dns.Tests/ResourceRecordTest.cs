using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class ResourceRecordTest
{
    [Test]
    public async Task Defaults()
    {
        var rr = new ResourceRecord();

        await Assert.That(rr.Class).IsEqualTo(DnsClass.IN);
        await Assert.That(rr.TTL).IsEqualTo(ResourceRecord.DefaultTTL);
    }

    [Test]
    public async Task DataLength()
    {
        var rr = new ResourceRecord();

        await Assert.That(rr.GetDataLength()).IsEqualTo(0);
    }

    [Test]
    public async Task DataLength_DerivedClass()
    {
        var a = new ARecord { Address = IPAddress.Parse("127.0.0.1") };

        await Assert.That(a.GetDataLength()).IsEqualTo(4);
    }

    [Test]
    public async Task Data()
    {
        var rr = new ResourceRecord();

        await Assert.That(rr.GetData()).Count().IsEqualTo(0);
    }

    [Test]
    public async Task Data_DerivedClass()
    {
        var a = new ARecord { Address = IPAddress.Parse("127.0.0.1") };

        await Assert.That(a.GetData()).Count().IsNotEqualTo(0);
    }

    [Test]
    public async Task RoundTrip()
    {
        var a = new ResourceRecord
        {
            Name = "emanon.org",
            Class = DnsClass.CH,
            Type = (DnsType)0xFFFF,
            TTL = TimeSpan.FromDays(2)
        };

        var b = (ResourceRecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.GetDataLength()).IsEqualTo(b.GetDataLength());
        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
        await Assert.That(b).IsAssignableTo<ResourceRecord>();
    }

    [Test]
    public async Task Value_Equality()
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

        ResourceRecord? c = null;
        ResourceRecord? d = null;
        ResourceRecord e = new();

        // ReSharper disable ConditionIsAlwaysTrueOrFalse
        await Assert.That(c == d).IsTrue();
        await Assert.That(c == b).IsFalse();
        await Assert.That(b == c).IsFalse();


        await Assert.That(c != d).IsFalse();
        await Assert.That(c != b).IsTrue();
        await Assert.That(b != c).IsTrue();
        // ReSharper restore ConditionIsAlwaysTrueOrFalse

#pragma warning disable 1718
        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a0 == a0).IsTrue();
        await Assert.That(a0 == a1).IsTrue();
        await Assert.That(a0 == b).IsFalse();

        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a0 != a0).IsFalse();
        await Assert.That(a0 != a1).IsFalse();
        await Assert.That(a0 != b).IsTrue();

        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a0.Equals(a0)).IsTrue();
        await Assert.That(a0.Equals(a1)).IsTrue();
        await Assert.That(a0.Equals(b)).IsFalse();

        await Assert.That(a0).IsEqualTo(a0);
        await Assert.That(a0).IsEqualTo(a1);
        await Assert.That(a0).IsNotEqualTo(b);

        await Assert.That(e).IsEqualTo(e);
        await Assert.That(e).IsNotEqualTo(a0);

        await Assert.That(a0.GetHashCode()).IsEqualTo(a0.GetHashCode());
        await Assert.That(a0.GetHashCode()).IsEqualTo(a1.GetHashCode());
        await Assert.That(a0.GetHashCode()).IsNotEqualTo(b.GetHashCode());
        await Assert.That(e.GetHashCode()).IsEqualTo(e.GetHashCode());
    }

    [Test]
    public async Task Stringing()
    {
        var a = new ResourceRecord
        {
            Name = "x.emanon.org",
            Type = DnsType.A
        };
        await Assert.That(a.ToString()).IsEqualTo("x.emanon.org IN A \\# 0");

        a = new ResourceRecord
        {
            Name = "x.emanon.org",
            Type = DnsType.A,
            Class = DnsClass.CH
        };
        await Assert.That(a.ToString()).IsEqualTo("x.emanon.org CH A \\# 0");

        a = new ResourceRecord
        {
            Name = "x.emanon.org",
            Type = DnsType.A,
            TTL = TimeSpan.FromSeconds(123)
        };
        await Assert.That(a.ToString()).IsEqualTo("x.emanon.org 123 IN A \\# 0");
    }

    [Test]
    public async Task CreationTime()
    {
        var now = DateTime.Now;
        var rr = new ResourceRecord();
        await Assert.That(rr.CreationTime.Kind).IsEqualTo(DateTimeKind.Local);
        await Assert.That(rr.CreationTime).IsGreaterThanOrEqualTo(now);

        await Task.Delay(50, TestContext.Current!.Execution.CancellationToken);
        var clone = rr.Clone<ResourceRecord>();
        await Assert.That(rr.CreationTime).IsEqualTo(clone.CreationTime);
    }

    [Test]
    public async Task IsExpired()
    {
        var rr = new ResourceRecord { TTL = TimeSpan.FromSeconds(2) };

        await Assert.That(rr.IsExpired()).IsFalse();
        await Assert.That(rr.IsExpired(DateTime.Now + TimeSpan.FromSeconds(-3))).IsFalse();
        await Assert.That(rr.IsExpired(DateTime.Now + TimeSpan.FromSeconds(3))).IsTrue();
    }

    [Test]
    public async Task Stringing_UnknownClass()
    {
        var a = new ResourceRecord
        {
            Name = "x.emanon.org",
            Class = (DnsClass)1234,
            Type = DnsType.A
        };

        await Assert.That(a.ToString()).IsEqualTo("x.emanon.org CLASS1234 A \\# 0");
    }

    [Test]
    public async Task Stringing_UnknownType()
    {
        var a = new ResourceRecord
        {
            Name = "x.emanon.org",
            Type = (DnsType)1234
        };

        await Assert.That(a.ToString()).IsEqualTo("x.emanon.org IN TYPE1234 \\# 0");
    }

    [Test]
    public async Task CanonicalName()
    {
        var rr = new ResourceRecord { Name = "x.EmAnOn.OrG" };

        await Assert.That(rr.CanonicalName).IsEqualTo("x.emanon.org");
    }

    [Test]
    public async Task RDATA_Underflow()
    {
        var ms = new MemoryStream(Convert.FromBase64String("A2ZvbwAABQABAAFRgAAKB3Vua25vd24A/w=="))
        {
            Position = 0
        };

        await Assert.That(() => new ResourceRecord().Read(ms)).Throws<InvalidDataException>();
    }
}
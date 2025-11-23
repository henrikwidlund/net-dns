using System;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class SOARecordTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new SOARecord
        {
            Name = "owner-name",
            PrimaryName = "emanaon.org",
            Mailbox = "hostmaster.emanon.org",
            SerialNumber = 1,
            Refresh = TimeSpan.FromDays(1),
            Retry = TimeSpan.FromMinutes(20),
            Expire = TimeSpan.FromDays(7 * 3),
            Minimum = TimeSpan.FromHours(2)
        };
        var b = (SOARecord)new ResourceRecord().Read(a.ToByteArray());
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.PrimaryName).IsEqualTo(b.PrimaryName);
        await Assert.That(a.Mailbox).IsEqualTo(b.Mailbox);
        await Assert.That(a.SerialNumber).IsEqualTo(b.SerialNumber);
        await Assert.That(a.Retry).IsEqualTo(b.Retry);
        await Assert.That(a.Expire).IsEqualTo(b.Expire);
        await Assert.That(a.Refresh).IsEqualTo(b.Refresh);
        await Assert.That(a.Minimum).IsEqualTo(b.Minimum);
    }

    [Test]
    public async Task Roundtrip_Master()
    {
        var a = new SOARecord
        {
            Name = "owner-name",
            PrimaryName = "emanaon.org",
            Mailbox = "hostmaster.emanon.org",
            SerialNumber = 1,
            Refresh = TimeSpan.FromDays(1),
            Retry = TimeSpan.FromMinutes(20),
            Expire = TimeSpan.FromDays(7 * 3),
            Minimum = TimeSpan.FromHours(2)
        };

        var b = (SOARecord)new ResourceRecord().Read(a.ToString())!;

        await Assert.That(b).IsNotNull();
        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.PrimaryName).IsEqualTo(b.PrimaryName);
        await Assert.That(a.Mailbox).IsEqualTo(b.Mailbox);
        await Assert.That(a.SerialNumber).IsEqualTo(b.SerialNumber);
        await Assert.That(a.Retry).IsEqualTo(b.Retry);
        await Assert.That(a.Expire).IsEqualTo(b.Expire);
        await Assert.That(a.Refresh).IsEqualTo(b.Refresh);
        await Assert.That(a.Minimum).IsEqualTo(b.Minimum);
    }

    [Test]
    public async Task Equality()
    {
        var a = new SOARecord
        {
            Name = "owner-name",
            PrimaryName = "emanaon.org",
            Mailbox = "hostmaster.emanon.org",
            SerialNumber = 1,
            Refresh = TimeSpan.FromDays(1),
            Retry = TimeSpan.FromMinutes(20),
            Expire = TimeSpan.FromDays(7 * 3),
            Minimum = TimeSpan.FromHours(2)
        };

        var b = new SOARecord
        {
            Name = "owner-name",
            PrimaryName = "emanaon.org",
            Mailbox = "hostmaster-x.emanon.org",
            SerialNumber = 1,
            Refresh = TimeSpan.FromDays(1),
            Retry = TimeSpan.FromMinutes(20),
            Expire = TimeSpan.FromDays(7 * 3),
            Minimum = TimeSpan.FromHours(2)
        };

        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }
}
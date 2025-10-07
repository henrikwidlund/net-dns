using System;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class SOARecordTest
{
    [Fact]
    public void Roundtrip()
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
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.PrimaryName.ShouldBe(b.PrimaryName);
        a.Mailbox.ShouldBe(b.Mailbox);
        a.SerialNumber.ShouldBe(b.SerialNumber);
        a.Retry.ShouldBe(b.Retry);
        a.Expire.ShouldBe(b.Expire);
        a.Refresh.ShouldBe(b.Refresh);
        a.Minimum.ShouldBe(b.Minimum);
    }

    [Fact]
    public void Roundtrip_Master()
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

        var b = (SOARecord)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.PrimaryName.ShouldBe(b.PrimaryName);
        a.Mailbox.ShouldBe(b.Mailbox);
        a.SerialNumber.ShouldBe(b.SerialNumber);
        a.Retry.ShouldBe(b.Retry);
        a.Expire.ShouldBe(b.Expire);
        a.Refresh.ShouldBe(b.Refresh);
        a.Minimum.ShouldBe(b.Minimum);
    }

    [Fact]
    public void Equality()
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
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }
}
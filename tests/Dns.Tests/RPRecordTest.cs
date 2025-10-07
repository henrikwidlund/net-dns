﻿using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class RPRecordTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new RPRecord
        {
            Name = "emanon.org",
            Mailbox = "nowon.emanon.org"
        };

        var b = (RPRecord)new ResourceRecord().Read(a.ToByteArray());

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Mailbox.ShouldBe(b.Mailbox);
        a.TextName.ShouldBe(b.TextName);
    }

    [Fact]
    public void Roundtrip_Master()
    {
        var a = new RPRecord
        {
            Name = "emanon.org",
            Mailbox = "nowon.emanon.org"
        };

        var b = (RPRecord)new ResourceRecord().Read(a.ToString());

        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Mailbox.ShouldBe(b.Mailbox);
        a.TextName.ShouldBe(b.TextName);
    }

    [Fact]
    public void Equality()
    {
        var a = new RPRecord
        {
            Name = "emanon.org",
            Mailbox = "nowon.emanon.org"
        };

        var b = new RPRecord
        {
            Name = "emanon.org",
            Mailbox = "someone.emanon.org"
        };

        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }
}
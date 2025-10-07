using System;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class NSECRecordTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new NSECRecord
        {
            Name = "alfa.example.com",
            TTL = TimeSpan.FromDays(1),
            NextOwnerName = "host.example.com",
            Types = { DnsType.A, DnsType.MX, DnsType.RRSIG, DnsType.NSEC, (DnsType)1234 }
        };
        
        var b = (NSECRecord)new ResourceRecord().Read(a.ToByteArray());
        
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.NextOwnerName.ShouldBe(b.NextOwnerName);
        a.Types.ShouldBe(b.Types);
    }

    [Fact]
    public void Roundtrip_Master()
    {
        var a = new NSECRecord
        {
            Name = "alfa.example.com",
            TTL = TimeSpan.FromDays(1),
            NextOwnerName = "host.example.com",
            Types = { DnsType.A, DnsType.MX, DnsType.RRSIG, DnsType.NSEC, (DnsType)1234 }
        };
        
        var b = (NSECRecord)new ResourceRecord().Read(a.ToString());
        
        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.NextOwnerName.ShouldBe(b.NextOwnerName);
        a.Types.ShouldBe(b.Types);
    }
}
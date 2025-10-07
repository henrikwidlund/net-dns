using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class UnknownRecordTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new UnknownRecord
        {
            Name = "emanon.org",
            Data = [10, 11, 12]
        };
        
        var b = (UnknownRecord)new ResourceRecord().Read(a.ToByteArray());
        
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Data.ShouldBe(b.Data);
    }

    [Fact]
    public void Equality()
    {
        var a = new UnknownRecord
        {
            Name = "emanon.org",
            Data = [1, 2, 3, 4]
        };
        
        var b = new UnknownRecord
        {
            Name = "emanon.org",
            Data = [1, 2, 3, 40]
        };
        
        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void Roundtrip_Master()
    {
        var a = new UnknownRecord
        {
            Name = "a.example",
            Class = (DnsClass)32,
            Type = (DnsType)731,
            Data = [0xab, 0xcd, 0xef, 0x01, 0x23, 0x45]
        };
        
        var b = (UnknownRecord)new ResourceRecord().Read(a.ToString());
        
        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Data.ShouldBe(b.Data);
    }
}
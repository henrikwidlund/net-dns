using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class HINFORecordTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new HINFORecord
        {
            Name = "emanaon.org",
            Cpu = "DEC-2020",
            OS = "TOPS20"
        };
        
        var b = (HINFORecord)new ResourceRecord().Read(a.ToByteArray());
        
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Cpu.ShouldBe(b.Cpu);
        a.OS.ShouldBe(b.OS);
    }

    [Fact]
    public void Roundtrip_Master()
    {
        var a = new HINFORecord
        {
            Name = "emanaon.org",
            Cpu = "DEC-2020",
            OS = "TOPS20"
        };
        
        var b = (HINFORecord)new ResourceRecord().Read(a.ToString());
        
        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Cpu.ShouldBe(b.Cpu);
        a.OS.ShouldBe(b.OS);
    }

    [Fact]
    public void Equality()
    {
        var a = new HINFORecord
        {
            Name = "emanaon.org",
            Cpu = "DEC-2020",
            OS = "TOPS20"
        };
        
        var b = new HINFORecord
        {
            Name = "emanaon.org",
            Cpu = "DEC-2040",
            OS = "TOPS20"
        };

        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }
}
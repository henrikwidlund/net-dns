using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class NULLRecordTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new NULLRecord
        {
            Name = "emanon.org",
            Data = [1, 2, 3, 4]
        };
        
        var b = (NULLRecord)new ResourceRecord().Read(a.ToByteArray());
        
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Data.ShouldBe(b.Data);
    }

    [Fact]
    public void Roundtrip_Master()
    {
        var a = new NULLRecord
        {
            Name = "emanon.org",
            Data = [1, 2, 3, 4]
        };
        
        var b = (NULLRecord)new ResourceRecord().Read(a.ToString());
        
        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Data.ShouldBe(b.Data);
    }

    [Fact]
    public void Equality()
    {
        var a = new NULLRecord
        {
            Name = "emanon.org",
            Data = [1, 2, 3, 4]
        };
        
        var b = new NULLRecord
        {
            Name = "emanon.org",
            Data = [1, 2, 3, 40]
        };
        
        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }
}
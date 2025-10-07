using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class TXTRecordTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new TXTRecord
        {
            Name = "the.printer.local",
            Strings =
            [
                "paper=A4",
                "colour=false"
            ]
        };
        
        var b = (TXTRecord)new ResourceRecord().Read(a.ToByteArray());
        
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Strings.ShouldBe(b.Strings);
    }

    [Fact]
    public void Roundtrip_Master()
    {
        var a = new TXTRecord
        {
            Name = "the.printer.local",
            Strings =
            [
                "paper=A4",
                "colour=false",
                "foo1=a b",
                @"foo2=a\b",
                "foo3=a\""
            ]
        };
        
        var b = (TXTRecord)new ResourceRecord().Read(a.ToString());
        
        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Strings.ShouldBe(b.Strings);
    }

    [Fact]
    public void NoStrings()
    {
        var a = new TXTRecord
        {
            Name = "the.printer.local"
        };
        
        var b = (TXTRecord)new ResourceRecord().Read(a.ToByteArray());
        
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Strings.ShouldBe(b.Strings);
    }

    [Fact]
    public void Equality()
    {
        var a = new TXTRecord
        {
            Name = "the.printer.local",
            Strings =
            [
                "paper=A4",
                "colour=false"
            ]
        };
        
        var b = new TXTRecord
        {
            Name = "the.printer.local",
            Strings =
            [
                "paper=A4",
                "colour=true"
            ]
        };

        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
        a.GetHashCode().ShouldNotBe(new TXTRecord().GetHashCode());
    }
}
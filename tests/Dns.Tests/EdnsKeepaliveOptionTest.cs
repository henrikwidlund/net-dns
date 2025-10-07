using System;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class EdnsKeepaliveOptionTest
{
    [Fact]
    public void Roundtrip()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsKeepaliveOption
        {
            Timeout = TimeSpan.FromSeconds(3)
        };
        
        expected.Type.ShouldBe(EdnsOptionType.Keepalive);

        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsKeepaliveOption)opt2.Options[0];
        
        actual.Type.ShouldBe(expected.Type);
        actual.Timeout.HasValue.ShouldBeTrue();
        actual.Timeout.Value.ShouldBe(expected.Timeout.Value);
    }

    [Fact]
    public void Roundtrip_Empty()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsKeepaliveOption();
        
        expected.Type.ShouldBe(EdnsOptionType.Keepalive);
        expected.Timeout.HasValue.ShouldBeFalse();

        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsKeepaliveOption)opt2.Options[0];
        
        actual.Type.ShouldBe(expected.Type);
        actual.Timeout.HasValue.ShouldBe(expected.Timeout.HasValue);
    }
}
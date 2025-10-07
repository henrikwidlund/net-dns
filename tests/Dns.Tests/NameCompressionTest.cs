using System.IO;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class NameCompressionTest
{
    [Fact]
    public void Writing()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName("a");
        writer.WriteDomainName("b");
        writer.WriteDomainName("b");
        var bytes = ms.ToArray();
        
        var expected = new byte[]
        {
            0x01, (byte)'a', 0,
            0x01, (byte)'b', 0,
            0XC0, 3
        };
        
        bytes.ShouldBe(expected);
    }

    [Fact]
    public void Writing_Labels()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName("a.b.c");
        writer.WriteDomainName("a.b.c");
        writer.WriteDomainName("b.c");
        writer.WriteDomainName("c");
        writer.WriteDomainName("x.b.c");
        var bytes = ms.ToArray();
        
        var expected = new byte[]
        {
            0x01, (byte)'a', 0x01, (byte)'b', 0x01, (byte)'c', 00,
            0xC0, 0x00,
            0xC0, 0x02,
            0xC0, 0x04,
            0x01, (byte)'x', 0xC0, 0x02
        };
        bytes.ShouldBe(expected);
    }

    [Fact]
    public void Writing_Past_MaxPointer()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteBytes(new byte[0x4000]);
        writer.WriteDomainName("a");
        writer.WriteDomainName("b");
        writer.WriteDomainName("b");

        ms.Position = 0;
        var reader = new WireReader(ms);
        reader.ReadBytes(0x4000);
        reader.ReadDomainName().ShouldBe("a");
        reader.ReadDomainName().ShouldBe("b");
        reader.ReadDomainName().ShouldBe("b");
    }

    [Fact]
    public void Reading_Labels()
    {
        var bytes = new byte[]
        {
            0x01, (byte)'a', 0x01, (byte)'b', 0x01, (byte)'c', 00,
            0xC0, 0x00,
            0xC0, 0x02,
            0xC0, 0x04,
            0x01, (byte)'x', 0xC0, 0x02
        };
        
        using var ms = new MemoryStream(bytes);
        var reader = new WireReader(ms);
        reader.ReadDomainName().ShouldBe("a.b.c");
        reader.ReadDomainName().ShouldBe("a.b.c");
        reader.ReadDomainName().ShouldBe("b.c");
        reader.ReadDomainName().ShouldBe("c");
        reader.ReadDomainName().ShouldBe("x.b.c");
    }

    [Fact]
    public void Reading()
    {
        var bytes = new byte[]
        {
            0x01, (byte)'a', 0,
            0x01, (byte)'b', 0,
            0XC0, 3
        };
        
        using var ms = new MemoryStream(bytes);
        var reader = new WireReader(ms);
        reader.ReadDomainName().ShouldBe("a");
        reader.ReadDomainName().ShouldBe("b");
        reader.ReadDomainName().ShouldBe("b");
    }
}
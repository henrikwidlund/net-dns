using System.IO;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class NameCompressionTest
{
    [Test]
    public async Task Writing()
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
        
        await Assert.That(bytes).IsEquivalentTo(expected);
    }

    [Test]
    public async Task Writing_Labels()
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
        await Assert.That(bytes).IsEquivalentTo(expected);
    }

    [Test]
    public async Task Writing_Past_MaxPointer()
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
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("a");
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("b");
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("b");
    }

    [Test]
    public async Task Reading_Labels()
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
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("a.b.c");
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("a.b.c");
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("b.c");
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("c");
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("x.b.c");
    }

    [Test]
    public async Task Reading()
    {
        var bytes = new byte[]
        {
            0x01, (byte)'a', 0,
            0x01, (byte)'b', 0,
            0XC0, 3
        };
        
        using var ms = new MemoryStream(bytes);
        var reader = new WireReader(ms);
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("a");
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("b");
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("b");
    }
}
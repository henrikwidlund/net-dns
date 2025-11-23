using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class WireReaderWriterTest
{
    [Test]
    public async Task Roundtrip()
    {
        var someBytes = new byte[] { 1, 2, 3 };
        var someDate = new DateTime(1997, 1, 21, 3, 4, 5, DateTimeKind.Utc);

        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName("emanon.org");
        writer.WriteString("alpha");
        writer.WriteTimeSpan32(TimeSpan.FromHours(3));
        writer.WriteUInt16(ushort.MaxValue);
        writer.WriteUInt32(uint.MaxValue);
        writer.WriteUInt48(0XFFFFFFFFFFFFul);
        writer.WriteBytes(someBytes);
        writer.WriteByteLengthPrefixedBytes(someBytes);
        writer.WriteByteLengthPrefixedBytes(null);
        writer.WriteIPAddress(IPAddress.Parse("127.0.0.1"));
        writer.WriteIPAddress(IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb"));
        writer.WriteDateTime32(someDate);
        writer.WriteDateTime48(someDate);
        ms.Position = 0;
        var reader = new WireReader(ms);
        
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("emanon.org");
        await Assert.That(reader.ReadString()).IsEqualTo("alpha");
        await Assert.That(reader.ReadTimeSpan32()).IsEqualTo(TimeSpan.FromHours(3));
        await Assert.That(reader.ReadUInt16()).IsEqualTo(ushort.MaxValue);
        await Assert.That(reader.ReadUInt32()).IsEqualTo(uint.MaxValue);
        await Assert.That(reader.ReadUInt48()).IsEqualTo(0XFFFFFFFFFFFFul);
        await Assert.That(reader.ReadBytes(3)).IsEquivalentTo(someBytes);
        await Assert.That(reader.ReadByteLengthPrefixedBytes()).IsEquivalentTo(someBytes);
        await Assert.That(reader.ReadByteLengthPrefixedBytes()).IsEquivalentTo(Array.Empty<byte>());
        await Assert.That(reader.ReadIPAddress()).IsEqualTo(IPAddress.Parse("127.0.0.1"));
        await Assert.That(reader.ReadIPAddress(16)).IsEqualTo(IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb"));
        await Assert.That(reader.ReadDateTime32()).IsEqualTo(someDate);
        await Assert.That(reader.ReadDateTime48()).IsEqualTo(someDate);
    }

    [Test]
    public async Task Write_DomainName()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName("a.b");
        ms.Position = 0;
        
        await Assert.That(ms.ReadByte()).IsEqualTo(1);
        await Assert.That((char)ms.ReadByte()).IsEqualTo('a');
        await Assert.That(ms.ReadByte()).IsEqualTo(1);
        await Assert.That((char)ms.ReadByte()).IsEqualTo('b');
        await Assert.That(ms.ReadByte()).IsEqualTo(0);
    }

    [Test]
    public async Task Write_EscapedDomainName()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName(@"a\.b");
        ms.Position = 0;
        
        await Assert.That(ms.ReadByte()).IsEqualTo(3);
        await Assert.That((char)ms.ReadByte()).IsEqualTo('a');
        await Assert.That((char)ms.ReadByte()).IsEqualTo('.');
        await Assert.That((char)ms.ReadByte()).IsEqualTo('b');
        await Assert.That(ms.ReadByte()).IsEqualTo(0);
    }

    [Test]
    public async Task BufferOverflow_Byte()
    {
        using var ms = new MemoryStream([]);
        var reader = new WireReader(ms);
        
        await Assert.That(() => reader.ReadByte()).Throws<EndOfStreamException>();
    }

    [Test]
    public async Task BufferOverflow_Bytes()
    {
        using var ms = new MemoryStream([1, 2]);
        var reader = new WireReader(ms);
        
        await Assert.That(() => reader.ReadBytes(3)).Throws<EndOfStreamException>();
    }

    [Test]
    public async Task BufferOverflow_DomainName()
    {
        using var ms = new MemoryStream([1, (byte)'a']);
        var reader = new WireReader(ms);
        
        await Assert.That(() => reader.ReadDomainName()).Throws<EndOfStreamException>();
    }

    [Test]
    public async Task BufferOverflow_String()
    {
        using var ms = new MemoryStream([10, 1]);
        var reader = new WireReader(ms);
        
        await Assert.That(() => reader.ReadString()).Throws<EndOfStreamException>();
    }

    [Test]
    public async Task BytePrefixedArray_TooBig()
    {
        var bytes = new byte[byte.MaxValue + 1];
        var writer = new WireWriter(new MemoryStream());
        
        await Assert.That(() => writer.WriteByteLengthPrefixedBytes(bytes)).Throws<ArgumentException>();
    }

    [Test]
    public async Task LengthPrefixedScope()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteString("abc");
        writer.PushLengthPrefixedScope();
        writer.WriteDomainName("a");
        writer.WriteDomainName("a");
        writer.PopLengthPrefixedScope();

        ms.Position = 0;
        var reader = new WireReader(ms);
        
        await Assert.That(reader.ReadString()).IsEqualTo("abc");
        await Assert.That(reader.ReadUInt16()).IsEqualTo((ushort)5);
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("a");
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("a");
    }

    [Test]
    public async Task EmptyDomainName()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName("");
        writer.WriteString("abc");

        ms.Position = 0;
        var reader = new WireReader(ms);
        
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("");
        await Assert.That(reader.ReadString()).IsEqualTo("abc");
    }

    [Test]
    public async Task CanonicalDomainName()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms) { CanonicalForm = true };
        writer.WriteDomainName("FOO");
        writer.WriteDomainName("FOO");
        await Assert.That(writer.Position).IsEqualTo(5 * 2);

        ms.Position = 0;
        var reader = new WireReader(ms);
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("foo");
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("foo");
    }

    [Test]
    public async Task NullDomainName_String()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName(null);
        writer.WriteString("abc");

        ms.Position = 0;
        var reader = new WireReader(ms);
        
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("");
        await Assert.That(reader.ReadString()).IsEqualTo("abc");
    }

    [Test]
    public async Task NullDomainName_Class()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName((DomainName?)null);
        writer.WriteString("abc");

        ms.Position = 0;
        var reader = new WireReader(ms);
        
        await Assert.That(reader.ReadDomainName()).IsEquatableOrEqualTo("");
        await Assert.That(reader.ReadString()).IsEqualTo("abc");
    }

    [Test]
    public async Task Read_EscapedDotDomainName()
    {
        const string domainName = @"a\.b";
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName(domainName);

        ms.Position = 0;
        var reader = new WireReader(ms);
        var name = reader.ReadDomainName();
        
        await Assert.That(name).IsEquatableOrEqualTo(domainName);
    }

    [Test]
    public async Task Bitmap()
    {
        // From https://tools.ietf.org/html/rfc3845#section-2.3
        var wire = new byte[]
        {
            0x00, 0x06, 0x40, 0x01, 0x00, 0x00, 0x00, 0x03,
            0x04, 0x1b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x20
        };
        
        using var ms1 = new MemoryStream(wire, false);
        var reader = new WireReader(ms1);
        var first = new ushort[] { 1, 15, 46, 47 };
        var second = new ushort[] { 1234 };
        await Assert.That(reader.ReadBitmap()).IsEquivalentTo(first);
        await Assert.That(reader.ReadBitmap()).IsEquivalentTo(second);

        using var ms2 = new MemoryStream();
        var writer = new WireWriter(ms2);
        writer.WriteBitmap([1, 15, 46, 47, 1234]);
        await Assert.That(ms2.ToArray()).IsEquivalentTo(wire);
    }

    [Test]
    public async Task Uint48TooBig()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        await Assert.That(() => writer.WriteUInt48(0X1FFFFFFFFFFFFul)).Throws<ArgumentException>();
    }

    [Test]
    public async Task ReadDateTime48()
    {
        // From https://tools.ietf.org/html/rfc2845 section 3.3
        var expected = new DateTime(1997, 1, 21, 0, 0, 0, DateTimeKind.Utc);
        using var ms = new MemoryStream([0x00, 0x00, 0x32, 0xe4, 0x07, 0x00]);
        var reader = new WireReader(ms);
        
        await Assert.That(reader.ReadDateTime48()).IsEqualTo(expected);
    }

    [Test]
    public async Task WriteString_NotAscii()
    {
        var writer = new WireWriter(Stream.Null);
        await Assert.That(() => writer.WriteString("δοκιμή")).Throws<ArgumentException>();
    }

    [Test]
    public async Task WriteString_TooBig()
    {
        var writer = new WireWriter(Stream.Null);
        await Assert.That(() => writer.WriteString(new string('a', 0x100))).Throws<ArgumentException>();
    }

    [Test]
    public async Task ReadString_NotAscii()
    {
        using var ms = new MemoryStream([1, 0xFF]);
        var reader = new WireReader(ms);
        await Assert.That(() => reader.ReadString()).Throws<InvalidDataException>();
    }

    [Test]
    public async Task WriteDateTime32_TooManySeconds()
    {
        var writer = new WireWriter(Stream.Null);
        writer.WriteDateTime32(DateTimeOffset.UnixEpoch.UtcDateTime);
        writer.WriteDateTime32(DateTimeOffset.UnixEpoch.UtcDateTime.AddSeconds(uint.MaxValue));

        await Assert.That(() => writer.WriteDateTime32(DateTimeOffset.UnixEpoch.UtcDateTime.AddSeconds((long)uint.MaxValue + 1))).Throws<OverflowException>();
    }
}
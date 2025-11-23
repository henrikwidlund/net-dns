using System;
using System.IO;
using System.Net;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class WireReaderWriterTest
{
    [Test]
    public void Roundtrip()
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
        
        reader.ReadDomainName().ShouldBe("emanon.org");
        reader.ReadString().ShouldBe("alpha");
        reader.ReadTimeSpan32().ShouldBe(TimeSpan.FromHours(3));
        reader.ReadUInt16().ShouldBe(ushort.MaxValue);
        reader.ReadUInt32().ShouldBe(uint.MaxValue);
        reader.ReadUInt48().ShouldBe(0XFFFFFFFFFFFFul);
        reader.ReadBytes(3).ShouldBe(someBytes);
        reader.ReadByteLengthPrefixedBytes().ShouldBe(someBytes);
        reader.ReadByteLengthPrefixedBytes().ShouldBe([]);
        reader.ReadIPAddress().ShouldBe(IPAddress.Parse("127.0.0.1"));
        reader.ReadIPAddress(16).ShouldBe(IPAddress.Parse("2406:e001:13c7:1:7173:ef8:852f:25cb"));
        reader.ReadDateTime32().ShouldBe(someDate);
        reader.ReadDateTime48().ShouldBe(someDate);
    }

    [Test]
    public void Write_DomainName()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName("a.b");
        ms.Position = 0;
        
        ms.ReadByte().ShouldBe(1, customMessage: "length of 'a'");
        ((char)ms.ReadByte()).ShouldBe('a');
        ms.ReadByte().ShouldBe(1, customMessage: "length of 'b'");
        ((char)ms.ReadByte()).ShouldBe('b');
        ms.ReadByte().ShouldBe(0, customMessage: "trailing nul");
    }

    [Test]
    public void Write_EscapedDomainName()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName(@"a\.b");
        ms.Position = 0;
        
        ms.ReadByte().ShouldBe(3, customMessage: "length of 'a.b'");
        ((char)ms.ReadByte()).ShouldBe('a');
        ((char)ms.ReadByte()).ShouldBe('.');
        ((char)ms.ReadByte()).ShouldBe('b');
        ms.ReadByte().ShouldBe(0, customMessage: "trailing nul");
    }

    [Test]
    public void BufferOverflow_Byte()
    {
        using var ms = new MemoryStream([]);
        var reader = new WireReader(ms);
        
        Should.Throw<EndOfStreamException>(() => reader.ReadByte());
    }

    [Test]
    public void BufferOverflow_Bytes()
    {
        using var ms = new MemoryStream([1, 2]);
        var reader = new WireReader(ms);
        
        Should.Throw<EndOfStreamException>(() => reader.ReadBytes(3));
    }

    [Test]
    public void BufferOverflow_DomainName()
    {
        using var ms = new MemoryStream([1, (byte)'a']);
        var reader = new WireReader(ms);
        
        Should.Throw<EndOfStreamException>(() => reader.ReadDomainName());
    }

    [Test]
    public void BufferOverflow_String()
    {
        using var ms = new MemoryStream([10, 1]);
        var reader = new WireReader(ms);
        
        Should.Throw<EndOfStreamException>(() => reader.ReadString());
    }

    [Test]
    public void BytePrefixedArray_TooBig()
    {
        var bytes = new byte[byte.MaxValue + 1];
        var writer = new WireWriter(new MemoryStream());
        
        Should.Throw<ArgumentException>(() => writer.WriteByteLengthPrefixedBytes(bytes));
    }

    [Test]
    public void LengthPrefixedScope()
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
        
        reader.ReadString().ShouldBe("abc");
        reader.ReadUInt16().ShouldBe((ushort)5);
        reader.ReadDomainName().ShouldBe("a");
        reader.ReadDomainName().ShouldBe("a");
    }

    [Test]
    public void EmptyDomainName()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName("");
        writer.WriteString("abc");

        ms.Position = 0;
        var reader = new WireReader(ms);
        
        reader.ReadDomainName().ShouldBe("");
        reader.ReadString().ShouldBe("abc");
    }

    [Test]
    public void CanonicalDomainName()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms) { CanonicalForm = true };
        writer.WriteDomainName("FOO");
        writer.WriteDomainName("FOO");
        writer.Position.ShouldBe(5 * 2);

        ms.Position = 0;
        var reader = new WireReader(ms);
        reader.ReadDomainName().ShouldBe("foo");
        reader.ReadDomainName().ShouldBe("foo");
    }

    [Test]
    public void NullDomainName_String()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName(null);
        writer.WriteString("abc");

        ms.Position = 0;
        var reader = new WireReader(ms);
        
        reader.ReadDomainName().ShouldBe("");
        reader.ReadString().ShouldBe("abc");
    }

    [Test]
    public void NullDomainName_Class()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName((DomainName)null);
        writer.WriteString("abc");

        ms.Position = 0;
        var reader = new WireReader(ms);
        
        reader.ReadDomainName().ShouldBe("");
        reader.ReadString().ShouldBe("abc");
    }

    [Test]
    public void Read_EscapedDotDomainName()
    {
        const string domainName = @"a\.b";
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        writer.WriteDomainName(domainName);

        ms.Position = 0;
        var reader = new WireReader(ms);
        var name = reader.ReadDomainName();
        
        name.ShouldBe(domainName);
    }

    [Test]
    public void Bitmap()
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
        reader.ReadBitmap().ShouldBe(first);
        reader.ReadBitmap().ShouldBe(second);

        using var ms2 = new MemoryStream();
        var writer = new WireWriter(ms2);
        writer.WriteBitmap([1, 15, 46, 47, 1234]);
        ms2.ToArray().ShouldBe(wire);
    }

    [Test]
    public void Uint48TooBig()
    {
        using var ms = new MemoryStream();
        var writer = new WireWriter(ms);
        Should.Throw<ArgumentException>(() => writer.WriteUInt48(0X1FFFFFFFFFFFFul));
    }

    [Test]
    public void ReadDateTime48()
    {
        // From https://tools.ietf.org/html/rfc2845 section 3.3
        var expected = new DateTime(1997, 1, 21, 0, 0, 0, DateTimeKind.Utc);
        using var ms = new MemoryStream([0x00, 0x00, 0x32, 0xe4, 0x07, 0x00]);
        var reader = new WireReader(ms);
        
        reader.ReadDateTime48().ShouldBe(expected);
    }

    [Test]
    public void WriteString_NotAscii()
    {
        var writer = new WireWriter(Stream.Null);
        Should.Throw<ArgumentException>(() => writer.WriteString("δοκιμή")); // test in Greek
    }

    [Test]
    public void WriteString_TooBig()
    {
        var writer = new WireWriter(Stream.Null);
        Should.Throw<ArgumentException>(() => writer.WriteString(new string('a', 0x100)));
    }

    [Test]
    public void ReadString_NotAscii()
    {
        using var ms = new MemoryStream([1, 0xFF]);
        var reader = new WireReader(ms);
        Should.Throw<InvalidDataException>(() => reader.ReadString());
    }

    [Test]
    public void WriteDateTime32_TooManySeconds()
    {
        var writer = new WireWriter(Stream.Null);
        writer.WriteDateTime32(DateTimeOffset.UnixEpoch.UtcDateTime);
        writer.WriteDateTime32(DateTimeOffset.UnixEpoch.UtcDateTime.AddSeconds(uint.MaxValue));

        Should.Throw<OverflowException>(() =>
        {
            writer.WriteDateTime32(DateTimeOffset.UnixEpoch.UtcDateTime.AddSeconds((long)(uint.MaxValue) + 1));
        });
    }
}
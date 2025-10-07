using System;
using System.IO;
using System.Net;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class PresentationWriterTest
{
    [Fact]
    public void WriteByte()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteByte(byte.MaxValue);
        writer.WriteByte(1, appendSpace: false);

        text.ToString().ShouldBe("255 1");
    }

    [Fact]
    public void WriteUInt16()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteUInt16(ushort.MaxValue);
        writer.WriteUInt16(1, appendSpace: false);

        text.ToString().ShouldBe("65535 1");
    }

    [Fact]
    public void WriteUInt32()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteUInt32(int.MaxValue);
        writer.WriteUInt32(1, appendSpace: false);

        text.ToString().ShouldBe("2147483647 1");
    }

    [Fact]
    public void WriteString()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteString("alpha");
        writer.WriteString("a b");
        writer.WriteString(null);
        writer.WriteString("");
        writer.WriteString(" ");
        writer.WriteString("a\\b");
        writer.WriteString("a\"b");
        writer.WriteString("end", appendSpace: false);

        text.ToString().ShouldBe("alpha \"a b\" \"\" \"\" \" \" a\\\\b a\\\"b end");
    }

    [Fact]
    public void WriteStringUnencoded()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteStringUnencoded("\\a");
        writer.WriteStringUnencoded("\\b", appendSpace: false);

        text.ToString().ShouldBe(@"\a \b");
    }

    [Fact]
    public void WriteDomainName()
    {
        using var text1 = new StringWriter();
        var writer = new PresentationWriter(text1);
        writer.WriteString("alpha.com");
        writer.WriteString("omega.com", appendSpace: false);
        text1.ToString().ShouldBe("alpha.com omega.com");

        using var text2 = new StringWriter();
        writer = new PresentationWriter(text2);
        writer.WriteDomainName(new DomainName("alpha.com"), false);
        text2.ToString().ShouldBe("alpha.com");
    }

    [Fact]
    public void WriteDomainName_Escaped()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteDomainName(new DomainName(@"dr\. smith.com"), false);

        text.ToString().ShouldBe(@"dr\.\032smith.com");
    }

    [Fact]
    public void WriteBase16String()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteBase16String([1, 2, 3]);
        writer.WriteBase16String([1, 2, 3], appendSpace: false);

        text.ToString().ShouldBe("010203 010203");
    }

    [Fact]
    public void WriteBase64String()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteBase64String([1, 2, 3]);
        writer.WriteBase64String([1, 2, 3], appendSpace: false);

        text.ToString().ShouldBe("AQID AQID");
    }

    [Fact]
    public void WriteTimeSpan16()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteTimeSpan16(TimeSpan.FromSeconds(ushort.MaxValue));
        writer.WriteTimeSpan16(TimeSpan.Zero, appendSpace: false);

        text.ToString().ShouldBe("65535 0");
    }

    [Fact]
    public void WriteTimeSpan32()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteTimeSpan32(TimeSpan.FromSeconds(int.MaxValue));
        writer.WriteTimeSpan32(TimeSpan.Zero, appendSpace: false);

        text.ToString().ShouldBe("2147483647 0");
    }

    [Fact]
    public void WriteDateTime()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteDateTime(DateTime.UnixEpoch);
        writer.WriteDateTime(DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc), appendSpace: false);

        text.ToString().ShouldBe("19700101000000 99991231235959");
    }

    [Fact]
    public void WriteIPAddress()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteIPAddress(IPAddress.Loopback);
        writer.WriteIPAddress(IPAddress.IPv6Loopback, appendSpace: false);

        text.ToString().ShouldBe("127.0.0.1 ::1");
    }

    [Fact]
    public void WriteDnsType()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteDnsType(DnsType.ANY);
        writer.WriteDnsType((DnsType)1234, appendSpace: false);

        text.ToString().ShouldBe("ANY TYPE1234");
    }

    [Fact]
    public void WriteDnsClass()
    {
        using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteDnsClass(DnsClass.IN);
        writer.WriteDnsClass((DnsClass)1234, appendSpace: false);

        text.ToString().ShouldBe("IN CLASS1234");
    }
}
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class PresentationWriterTest
{
    [Test]
    public async Task WriteByte()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteByte(byte.MaxValue);
        writer.WriteByte(1, appendSpace: false);

        await Assert.That(text.ToString()).IsEqualTo("255 1");
    }

    [Test]
    public async Task WriteUInt16()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteUInt16(ushort.MaxValue);
        writer.WriteUInt16(1, appendSpace: false);

        await Assert.That(text.ToString()).IsEqualTo("65535 1");
    }

    [Test]
    public async Task WriteUInt32()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteUInt32(int.MaxValue);
        writer.WriteUInt32(1, appendSpace: false);

        await Assert.That(text.ToString()).IsEqualTo("2147483647 1");
    }

    [Test]
    public async Task WriteString()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteString("alpha");
        writer.WriteString("a b");
        writer.WriteString(null);
        writer.WriteString("");
        writer.WriteString(" ");
        writer.WriteString("a\\b");
        writer.WriteString("a\"b");
        writer.WriteString("end", appendSpace: false);

        await Assert.That(text.ToString()).IsEqualTo("alpha \"a b\" \"\" \"\" \" \" a\\\\b a\\\"b end");
    }

    [Test]
    public async Task WriteStringUnencoded()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteStringUnencoded("\\a");
        writer.WriteStringUnencoded("\\b", appendSpace: false);

        await Assert.That(text.ToString()).IsEqualTo(@"\a \b");
    }

    [Test]
    public async Task WriteDomainName()
    {
        await using var text1 = new StringWriter();
        var writer = new PresentationWriter(text1);
        writer.WriteString("alpha.com");
        writer.WriteString("omega.com", appendSpace: false);
        await Assert.That(text1.ToString()).IsEqualTo("alpha.com omega.com");

        await using var text2 = new StringWriter();
        writer = new PresentationWriter(text2);
        writer.WriteDomainName(new DomainName("alpha.com"), false);
        await Assert.That(text2.ToString()).IsEqualTo("alpha.com");
    }

    [Test]
    public async Task WriteDomainName_Escaped()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteDomainName(new DomainName(@"dr\. smith.com"), false);

        await Assert.That(text.ToString()).IsEqualTo(@"dr\.\032smith.com");
    }

    [Test]
    public async Task WriteBase16String()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteBase16String([1, 2, 3]);
        writer.WriteBase16String([1, 2, 3], appendSpace: false);

        await Assert.That(text.ToString()).IsEqualTo("010203 010203");
    }

    [Test]
    public async Task WriteBase64String()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteBase64String([1, 2, 3]);
        writer.WriteBase64String([1, 2, 3], appendSpace: false);

        await Assert.That(text.ToString()).IsEqualTo("AQID AQID");
    }

    [Test]
    public async Task WriteTimeSpan16()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteTimeSpan16(TimeSpan.FromSeconds(ushort.MaxValue));
        writer.WriteTimeSpan16(TimeSpan.Zero, appendSpace: false);

        await Assert.That(text.ToString()).IsEqualTo("65535 0");
    }

    [Test]
    public async Task WriteTimeSpan32()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteTimeSpan32(TimeSpan.FromSeconds(int.MaxValue));
        writer.WriteTimeSpan32(TimeSpan.Zero, appendSpace: false);

        await Assert.That(text.ToString()).IsEqualTo("2147483647 0");
    }

    [Test]
    public async Task WriteDateTime()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteDateTime(DateTime.UnixEpoch);
        writer.WriteDateTime(DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc), appendSpace: false);

        await Assert.That(text.ToString()).IsEqualTo("19700101000000 99991231235959");
    }

    [Test]
    public async Task WriteIPAddress()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteIPAddress(IPAddress.Loopback);
        writer.WriteIPAddress(IPAddress.IPv6Loopback, appendSpace: false);

        await Assert.That(text.ToString()).IsEqualTo("127.0.0.1 ::1");
    }

    [Test]
    public async Task WriteDnsType()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteDnsType(DnsType.ANY);
        writer.WriteDnsType((DnsType)1234, appendSpace: false);

        await Assert.That(text.ToString()).IsEqualTo("ANY TYPE1234");
    }

    [Test]
    public async Task WriteDnsClass()
    {
        await using var text = new StringWriter();
        var writer = new PresentationWriter(text);
        writer.WriteDnsClass(DnsClass.IN);
        writer.WriteDnsClass((DnsClass)1234, appendSpace: false);

        await Assert.That(text.ToString()).IsEqualTo("IN CLASS1234");
    }
}
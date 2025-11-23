using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class PresentationReaderTest
{
    [Test]
    public async Task ReadString()
    {
        var reader = new PresentationReader(new StringReader("  alpha   beta   omega"));

        await Assert.That(reader.ReadString()).IsEqualTo("alpha");
        await Assert.That(reader.ReadString()).IsEqualTo("beta");
        await Assert.That(reader.ReadString()).IsEqualTo("omega");
    }

    [Test]
    public async Task ReadQuotedStrings()
    {
        var reader = new PresentationReader(new StringReader("  \"a b c\"  \"x y z\""));

        await Assert.That(reader.ReadString()).IsEqualTo("a b c");
        await Assert.That(reader.ReadString()).IsEqualTo("x y z");
    }

    [Test]
    public async Task ReadEscapedStrings()
    {
        var reader = new PresentationReader(new StringReader("  alpha\\ beta   omega"));

        await Assert.That(reader.ReadString()).IsEqualTo("alpha beta");
        await Assert.That(reader.ReadString()).IsEqualTo("omega");
    }

    [Test]
    public async Task ReadDecimalEscapedString()
    {
        var reader = new PresentationReader(new StringReader("a\\098c"));

        await Assert.That(reader.ReadString()).IsEqualTo("abc");
    }

    [Test]
    public async Task ReadInvalidDecimalEscapedString()
    {
        var reader = new PresentationReader(new StringReader("a\\256c"));

        await Assert.That(() => reader.ReadString()).Throws<FormatException>();
    }

    [Test]
    public async Task ReadResource()
    {
        var reader = new PresentationReader(new StringReader("me A 127.0.0.1"));
        var resource = reader.ReadResourceRecord();

        await Assert.That(resource).IsNotNull();
        await Assert.That(resource!.Name).IsEquatableOrEqualTo("me");
        await Assert.That(resource.Class).IsEqualTo(DnsClass.IN);
        await Assert.That(resource.Type).IsEqualTo(DnsType.A);
        await Assert.That(resource.TTL).IsEqualTo(ResourceRecord.DefaultTTL);
        await Assert.That(resource).IsTypeOf<ARecord>();
    }

    [Test]
    public async Task ReadResourceWithNameOfType()
    {
        var reader = new PresentationReader(new StringReader("A A 127.0.0.1"));
        var resource = reader.ReadResourceRecord();

        await Assert.That(resource).IsNotNull();
        await Assert.That(resource!.Name).IsEquatableOrEqualTo("A");
        await Assert.That(resource.Class).IsEqualTo(DnsClass.IN);
        await Assert.That(resource.Type).IsEqualTo(DnsType.A);
        await Assert.That(resource.TTL).IsEqualTo(ResourceRecord.DefaultTTL);
        await Assert.That(resource).IsTypeOf<ARecord>();
    }

    [Test]
    public async Task ReadResourceWithNameOfClass()
    {
        var reader = new PresentationReader(new StringReader("CH A 127.0.0.1"));
        var resource = reader.ReadResourceRecord();

        await Assert.That(resource).IsNotNull();
        await Assert.That(resource!.Name).IsEquatableOrEqualTo("CH");
        await Assert.That(resource.Class).IsEqualTo(DnsClass.IN);
        await Assert.That(resource.Type).IsEqualTo(DnsType.A);
        await Assert.That(resource.TTL).IsEqualTo(ResourceRecord.DefaultTTL);
        await Assert.That(resource).IsTypeOf<ARecord>();
    }

    [Test]
    public async Task ReadResourceWithClassAndTTL()
    {
        var reader = new PresentationReader(new StringReader("me CH 63 A 127.0.0.1"));
        var resource = reader.ReadResourceRecord();

        await Assert.That(resource).IsNotNull();
        await Assert.That(resource!.Name).IsEquatableOrEqualTo("me");
        await Assert.That(resource.Class).IsEqualTo(DnsClass.CH);
        await Assert.That(resource.Type).IsEqualTo(DnsType.A);
        await Assert.That(resource.TTL).IsEqualTo(TimeSpan.FromSeconds(63));
        await Assert.That(resource).IsTypeOf<ARecord>();
    }

    [Test]
    public async Task ReadResourceWithUnknownClass()
    {
        var reader = new PresentationReader(new StringReader("me CLASS1234 A 127.0.0.1"));
        var resource = reader.ReadResourceRecord();

        await Assert.That(resource).IsNotNull();
        await Assert.That(resource!.Name).IsEquatableOrEqualTo("me");
        await Assert.That(resource.Class).IsEqualTo((DnsClass)1234);
        await Assert.That(resource.Type).IsEqualTo(DnsType.A);
        await Assert.That(resource).IsTypeOf<ARecord>();
    }

    [Test]
    public async Task ReadResourceWithUnknownType()
    {
        var reader = new PresentationReader(new StringReader("me CH TYPE1234 \\# 0"));
        var resource = reader.ReadResourceRecord();

        await Assert.That(resource).IsNotNull();
        await Assert.That(resource!.Name).IsEquatableOrEqualTo("me");
        await Assert.That(resource.Class).IsEqualTo(DnsClass.CH);
        await Assert.That(resource.Type).IsEqualTo((DnsType)1234);
        await Assert.That(resource).IsTypeOf<UnknownRecord>();
    }

    [Test]
    public async Task ReadResourceMissingName()
    {
        var reader = new PresentationReader(new StringReader("  NS ns1"));
        await Assert.That(() => reader.ReadResourceRecord()).Throws<InvalidDataException>();
    }

    [Test]
    public async Task ReadResourceWithComment()
    {
        var reader = new PresentationReader(new StringReader("; comment\r\nme A 127.0.0.1"));
        var resource = reader.ReadResourceRecord();

        await Assert.That(resource).IsNotNull();
        await Assert.That(resource!.Name).IsEquatableOrEqualTo("me");
        await Assert.That(resource.Class).IsEqualTo(DnsClass.IN);
        await Assert.That(resource.Type).IsEqualTo(DnsType.A);
        await Assert.That(resource.TTL).IsEqualTo(ResourceRecord.DefaultTTL);
        await Assert.That(resource).IsTypeOf<ARecord>();
    }

    [Test]
    public async Task ReadResourceWithOrigin()
    {
        const string text = """
                            $ORIGIN emanon.org. ; no such place\r\n
                            @ PTR localhost
                            """;
        var reader = new PresentationReader(new StringReader(text));
        var resource = reader.ReadResourceRecord();

        await Assert.That(resource).IsNotNull();
        await Assert.That(resource!.Name).IsEquatableOrEqualTo("emanon.org");
        await Assert.That(resource.Class).IsEqualTo(DnsClass.IN);
        await Assert.That(resource.Type).IsEqualTo(DnsType.PTR);
        await Assert.That(resource.TTL).IsEqualTo(ResourceRecord.DefaultTTL);
        await Assert.That(resource).IsTypeOf<PTRRecord>();
    }

    [Test]
    public async Task ReadResourceWithEscapedOrigin()
    {
        const string text = """
                            $ORIGIN emanon\.org. ; no such place\r\n
                            @ PTR localhost
                            """;
        var reader = new PresentationReader(new StringReader(text));
        var resource = reader.ReadResourceRecord();

        await Assert.That(resource).IsNotNull();
        await Assert.That(resource!.Name).IsEquatableOrEqualTo(@"emanon\.org");
        await Assert.That(resource.Class).IsEqualTo(DnsClass.IN);
        await Assert.That(resource.Type).IsEqualTo(DnsType.PTR);
        await Assert.That(resource.TTL).IsEqualTo(ResourceRecord.DefaultTTL);
        await Assert.That(resource).IsTypeOf<PTRRecord>();
        await Assert.That(resource.Name).IsNotNull();
        await Assert.That(resource.Name!.Labels).HasCount(1);
    }

    [Test]
    public async Task ReadResourceWithTTL()
    {
        const string text = """
                            $TTL 120 ; 2 minutes\r\n
                            emanon.org PTR localhost
                            """;
        var reader = new PresentationReader(new StringReader(text));
        var resource = reader.ReadResourceRecord();

        await  Assert.That(resource).IsNotNull();
        await Assert.That(resource!.Name).IsEquatableOrEqualTo("emanon.org");
        await Assert.That(resource.Class).IsEqualTo(DnsClass.IN);
        await Assert.That(resource.Type).IsEqualTo(DnsType.PTR);
        await Assert.That(resource.TTL).IsEqualTo(TimeSpan.FromMinutes(2));
        await Assert.That(resource).IsTypeOf<PTRRecord>();
    }

    [Test]
    public async Task ReadResourceWithPreviousDomain()
    {
        const string text = """
                            emanon.org A 127.0.0.1
                                       AAAA ::1
                            """;
        var reader = new PresentationReader(new StringReader(text));
        var a = reader.ReadResourceRecord();

        await Assert.That(a).IsNotNull();
        await Assert.That(a!.Name).IsEquatableOrEqualTo("emanon.org");
        await Assert.That(a.Class).IsEqualTo(DnsClass.IN);
        await Assert.That(a.Type).IsEqualTo(DnsType.A);
        await Assert.That(a.TTL).IsEqualTo(ResourceRecord.DefaultTTL);
        await Assert.That(a).IsTypeOf<ARecord>();

        var aaaa = reader.ReadResourceRecord();
        await Assert.That(aaaa).IsNotNull();
        await Assert.That(aaaa!.Name).IsEquatableOrEqualTo("emanon.org");
        await Assert.That(aaaa.Class).IsEqualTo(DnsClass.IN);
        await Assert.That(aaaa.Type).IsEqualTo(DnsType.AAAA);
        await Assert.That(aaaa.TTL).IsEqualTo(ResourceRecord.DefaultTTL);
        await Assert.That(aaaa).IsTypeOf<AAAARecord>();
    }

    [Test]
    public async Task ReadResourceWithPreviousEscapedDomain()
    {
        const string text = """
                            emanon\126.org A 127.0.0.1
                                       AAAA ::1
                            """;
        var reader = new PresentationReader(new StringReader(text));
        var a = reader.ReadResourceRecord();

        await Assert.That(a).IsNotNull();
        await Assert.That(a!.Name).IsEquatableOrEqualTo("emanon~.org");
        await Assert.That(a.Class).IsEqualTo(DnsClass.IN);
        await Assert.That(a.Type).IsEqualTo(DnsType.A);
        await Assert.That(a.TTL).IsEqualTo(ResourceRecord.DefaultTTL);
        await Assert.That(a).IsTypeOf<ARecord>();
        await Assert.That(a.Name).IsNotNull();
        await Assert.That(a.Name!.Labels).HasCount(2);

        var aaaa = reader.ReadResourceRecord();
        await  Assert.That(aaaa).IsNotNull();
        await Assert.That(aaaa!.Name).IsEquatableOrEqualTo("emanon~.org");
        await Assert.That(aaaa.Class).IsEqualTo(DnsClass.IN);
        await Assert.That(aaaa.Type).IsEqualTo(DnsType.AAAA);
        await Assert.That(aaaa.TTL).IsEqualTo(ResourceRecord.DefaultTTL);
        await Assert.That(aaaa).IsTypeOf<AAAARecord>();
        await Assert.That(aaaa.Name).IsNotNull();
        await Assert.That(aaaa.Name!.Labels).HasCount(2);
    }

    [Test]
    public async Task ReadResourceWithLeadingEscapedDomainName()
    {
        const string text = @"\126emanon.org A 127.0.0.1";
        var reader = new PresentationReader(new StringReader(text));
        var a = reader.ReadResourceRecord();

        await Assert.That(a).IsNotNull();
        await Assert.That(a!.Name).IsEquatableOrEqualTo("~emanon.org");
        await Assert.That(a.Class).IsEqualTo(DnsClass.IN);
        await Assert.That(a.Type).IsEqualTo(DnsType.A);
        await Assert.That(a.TTL).IsEqualTo(ResourceRecord.DefaultTTL);
        await Assert.That(a).IsTypeOf<ARecord>();
        await Assert.That(a.Name).IsNotNull();
        await Assert.That(a.Name!.Labels).HasCount(2);
    }

    [Test]
    public async Task ReadZoneFile()
    {
        const string text = """
                            $ORIGIN example.com.     ; designates the start of this zone file in the namespace
                            $TTL 3600                  ; default expiration time of all resource records without their own TTL value
                            ; example.com.  IN  SOA   ns.example.com. username.example.com. ( 2007120710 1d 2h 4w 1h )
                            example.com.  IN  SOA   ns.example.com. username.example.com. ( 2007120710 1 2 4 1 )
                            example.com.  IN  NS    ns                    ; ns.example.com is a nameserver for example.com
                            example.com.  IN  NS    ns.somewhere.example. ; ns.somewhere.example is a backup nameserver for example.com
                            example.com.  IN  MX    10 mail.example.com.  ; mail.example.com is the mailserver for example.com
                            @             IN  MX    20 mail2.example.com. ; equivalent to above line, '@' represents zone origin
                            @             IN  MX    50 mail3              ; equivalent to above line, but using a relative host name
                            example.com.  IN  A     192.0.2.1             ; IPv4 address for example.com
                                          IN  AAAA  2001:db8:10::1        ; IPv6 address for example.com
                            ns            IN  A     192.0.2.2             ; IPv4 address for ns.example.com
                                          IN  AAAA  2001:db8:10::2        ; IPv6 address for ns.example.com
                            www           IN  CNAME example.com.          ; www.example.com is an alias for example.com
                            wwwtest       IN  CNAME www                   ; wwwtest.example.com is another alias for www.example.com mail          IN  A     192.0.2.3             ; IPv4 address for mail.example.com
                            mail          IN  A     192.0.2.3             ; IPv4 address for mail.example.com
                            mail2         IN  A     192.0.2.4             ; IPv4 address for mail2.example.com
                            mail3         IN  A     192.0.2.5             ; IPv4 address for mail3.example.com
                            """;

        var reader = new PresentationReader(new StringReader(text));
        var resources = new List<ResourceRecord>();

        while (true)
        {
            var r = reader.ReadResourceRecord();
            if (r == null)
                break;
            resources.Add(r);
        }

        await Assert.That(resources).HasCount(15);
    }

    [Test]
    public async Task ReadResourceData()
    {
        var reader = new PresentationReader(new StringReader("\\# 0"));
        var rdata = reader.ReadResourceData();
        await Assert.That(rdata).HasCount().Zero();

        reader = new PresentationReader(new StringReader("\\# 3 abcdef"));
        rdata = reader.ReadResourceData();
        await Assert.That(new byte[] { 0xab, 0xcd, 0xef }).IsEquivalentTo(rdata);

        reader = new PresentationReader(new StringReader("\\# 3 ab cd ef"));
        rdata = reader.ReadResourceData();
        await Assert.That(new byte[] { 0xab, 0xcd, 0xef }).IsEquivalentTo(rdata);

        reader = new PresentationReader(new StringReader("\\# 3 abcd (\r\n  ef )"));
        rdata = reader.ReadResourceData();
        await Assert.That(new byte[] { 0xab, 0xcd, 0xef }).IsEquivalentTo(rdata);
    }

    [Test]
    public void ReadResourceData_MissingLeadin()
    {
        var reader = new PresentationReader(new StringReader("0"));
        Assert.Throws<FormatException>(() => reader.ReadResourceData());
    }

    [Test]
    public void ReadResourceData_BadHex_BadDigit()
    {
        var reader = new PresentationReader(new StringReader("\\# 3 ab cd ez"));
        Assert.Throws<FormatException>(() => reader.ReadResourceData());
    }

    [Test]
    public void ReadResourceData_BadHex_NotEven()
    {
        var reader = new PresentationReader(new StringReader("\\# 3 ab cd e"));
        Assert.Throws<FormatException>(() => reader.ReadResourceData());
    }

    [Test]
    public void ReadResourceData_BadHex_TooFew()
    {
        var reader = new PresentationReader(new StringReader("\\# 3 abcd"));
        Assert.Throws<FormatException>(() => reader.ReadResourceData());
    }

    [Test]
    public async Task ReadType()
    {
        var reader = new PresentationReader(new StringReader("A TYPE1 MX"));
        await Assert.That(reader.ReadDnsType()).IsEqualTo(DnsType.A);
        await Assert.That(reader.ReadDnsType()).IsEqualTo(DnsType.A);
        await Assert.That(reader.ReadDnsType()).IsEqualTo(DnsType.MX);
    }

    [Test]
    public void ReadType_BadName()
    {
        var reader = new PresentationReader(new StringReader("BADNAME"));
        Assert.Throws<ArgumentException>(() => reader.ReadDnsType());
    }

    [Test]
    public void ReadType_BadDigit()
    {
        var reader = new PresentationReader(new StringReader("TYPEX"));
        Assert.Throws<FormatException>(() => reader.ReadDnsType());
    }

    [Test]
    public async Task ReadMultipleStrings()
    {
        var expected = new List<string> { "abc", "def", "ghi" };
        var reader = new PresentationReader(new StringReader("abc def (\r\nghi)\r\n"));
        var actual = new List<string>();
        while (!reader.IsEndOfLine())
            actual.Add(reader.ReadString());

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task ReadMultipleStrings2()
    {
        var expected = new List<string> { "abc", "def", "ghi", "jkl" };
        var reader = new PresentationReader(new StringReader("abc def (\r\nghi) jkl   \r\n"));
        var actual = new List<string>();
        while (!reader.IsEndOfLine())
            actual.Add(reader.ReadString());

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task ReadMultipleStrings3()
    {
        var expected = new List<string> { "abc", "def", "ghi" };
        var reader = new PresentationReader(new StringReader("abc def (\rghi)\r"));
        var actual = new List<string>();
        while (!reader.IsEndOfLine())
            actual.Add(reader.ReadString());

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task ReadMultipleStrings_LF()
    {
        var expected = new List<string> { "abc", "def" };
        var reader = new PresentationReader(new StringReader("abc def\rghi"));
        var actual = new List<string>();
        while (!reader.IsEndOfLine())
            actual.Add(reader.ReadString());

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task ReadMultipleStrings_CRLF()
    {
        var expected = new List<string> { "abc", "def" };
        var reader = new PresentationReader(new StringReader("abc def\r\nghi"));
        var actual = new List<string>();
        while (!reader.IsEndOfLine())
            actual.Add(reader.ReadString());

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task ReadBase64String()
    {
        var expected = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        var reader = new PresentationReader(new StringReader("AAECAwQFBgcICQoLDA0ODw=="));
        await Assert.That(reader.ReadBase64String()).IsEquivalentTo(expected);

        reader = new PresentationReader(new StringReader("AAECAwQFBg  cICQoLDA0ODw=="));
        await Assert.That(reader.ReadBase64String()).IsEquivalentTo(expected);

        reader = new PresentationReader(new StringReader("AAECAwQFBg  (\r\n  cICQo\r\n  LDA0ODw\r\n== )"));
        await Assert.That(reader.ReadBase64String()).IsEquivalentTo(expected);
    }

    [Test]
    public async Task ReadDateTime()
    {
        DateTime expected = new(2004, 9, 16, 0, 0, 0, DateTimeKind.Utc);
        var reader = new PresentationReader(new StringReader("1095292800 20040916000000"));

        await Assert.That(reader.ReadDateTime()).IsEqualTo(expected);
        await Assert.That(reader.ReadDateTime()).IsEqualTo(expected);
    }

    [Test]
    public async Task ReadDomainName_Escaped()
    {
        var foo = new DomainName("foo.com");
        var drSmith = new DomainName(@"dr\. smith.com");
        var reader = new PresentationReader(new StringReader(@"dr\.\032smith.com foo.com"));

        await Assert.That(reader.ReadDomainName()).IsEqualTo(drSmith);
        await Assert.That(reader.ReadDomainName()).IsEqualTo(foo);
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class PresentationReaderTest
{
    [Test]
    public void ReadString()
    {
        var reader = new PresentationReader(new StringReader("  alpha   beta   omega"));

        reader.ReadString().ShouldBe("alpha");
        reader.ReadString().ShouldBe("beta");
        reader.ReadString().ShouldBe("omega");
    }

    [Test]
    public void ReadQuotedStrings()
    {
        var reader = new PresentationReader(new StringReader("  \"a b c\"  \"x y z\""));

        reader.ReadString().ShouldBe("a b c");
        reader.ReadString().ShouldBe("x y z");
    }

    [Test]
    public void ReadEscapedStrings()
    {
        var reader = new PresentationReader(new StringReader("  alpha\\ beta   omega"));

        reader.ReadString().ShouldBe("alpha beta");
        reader.ReadString().ShouldBe("omega");
    }

    [Test]
    public void ReadDecimalEscapedString()
    {
        var reader = new PresentationReader(new StringReader("a\\098c"));

        reader.ReadString().ShouldBe("abc");
    }

    [Test]
    public void ReadInvalidDecimalEscapedString()
    {
        var reader = new PresentationReader(new StringReader("a\\256c"));

        Should.Throw<FormatException>(() => reader.ReadString());
    }

    [Test]
    public void ReadResource()
    {
        var reader = new PresentationReader(new StringReader("me A 127.0.0.1"));
        var resource = reader.ReadResourceRecord();

        resource.ShouldNotBeNull();
        resource.Name.ShouldBe("me");
        resource.Class.ShouldBe(DnsClass.IN);
        resource.Type.ShouldBe(DnsType.A);
        resource.TTL.ShouldBe(ResourceRecord.DefaultTTL);
        resource.ShouldBeOfType<ARecord>();
    }

    [Test]
    public void ReadResourceWithNameOfType()
    {
        var reader = new PresentationReader(new StringReader("A A 127.0.0.1"));
        var resource = reader.ReadResourceRecord();

        resource.ShouldNotBeNull();
        resource.Name.ShouldBe("A");
        resource.Class.ShouldBe(DnsClass.IN);
        resource.Type.ShouldBe(DnsType.A);
        resource.TTL.ShouldBe(ResourceRecord.DefaultTTL);
        resource.ShouldBeOfType<ARecord>();
    }

    [Test]
    public void ReadResourceWithNameOfClass()
    {
        var reader = new PresentationReader(new StringReader("CH A 127.0.0.1"));
        var resource = reader.ReadResourceRecord();

        resource.ShouldNotBeNull();
        resource.Name.ShouldBe("CH");
        resource.Class.ShouldBe(DnsClass.IN);
        resource.Type.ShouldBe(DnsType.A);
        resource.TTL.ShouldBe(ResourceRecord.DefaultTTL);
        resource.ShouldBeOfType<ARecord>();
    }

    [Test]
    public void ReadResourceWithClassAndTTL()
    {
        var reader = new PresentationReader(new StringReader("me CH 63 A 127.0.0.1"));
        var resource = reader.ReadResourceRecord();

        resource.ShouldNotBeNull();
        resource.Name.ShouldBe("me");
        resource.Class.ShouldBe(DnsClass.CH);
        resource.Type.ShouldBe(DnsType.A);
        resource.TTL.ShouldBe(TimeSpan.FromSeconds(63));
        resource.ShouldBeOfType<ARecord>();
    }

    [Test]
    public void ReadResourceWithUnknownClass()
    {
        var reader = new PresentationReader(new StringReader("me CLASS1234 A 127.0.0.1"));
        var resource = reader.ReadResourceRecord();

        resource.ShouldNotBeNull();
        resource.Name.ShouldBe("me");
        resource.Class.ShouldBe((DnsClass)1234);
        resource.Type.ShouldBe(DnsType.A);
        resource.ShouldBeOfType<ARecord>();
    }

    [Test]
    public void ReadResourceWithUnknownType()
    {
        var reader = new PresentationReader(new StringReader("me CH TYPE1234 \\# 0"));
        var resource = reader.ReadResourceRecord();

        resource.ShouldNotBeNull();
        resource.Name.ShouldBe("me");
        resource.Class.ShouldBe(DnsClass.CH);
        resource.Type.ShouldBe((DnsType)1234);
        resource.ShouldBeOfType<UnknownRecord>();
    }

    [Test]
    public void ReadResourceMissingName()
    {
        var reader = new PresentationReader(new StringReader("  NS ns1"));
        Should.Throw<InvalidDataException>(() => reader.ReadResourceRecord());
    }

    [Test]
    public void ReadResourceWithComment()
    {
        var reader = new PresentationReader(new StringReader("; comment\r\nme A 127.0.0.1"));
        var resource = reader.ReadResourceRecord();

        resource.ShouldNotBeNull();
        resource.Name.ShouldBe("me");
        resource.Class.ShouldBe(DnsClass.IN);
        resource.Type.ShouldBe(DnsType.A);
        resource.TTL.ShouldBe(ResourceRecord.DefaultTTL);
        resource.ShouldBeOfType<ARecord>();
    }

    [Test]
    public void ReadResourceWithOrigin()
    {
        const string text = """
                            $ORIGIN emanon.org. ; no such place\r\n
                            @ PTR localhost
                            """;
        var reader = new PresentationReader(new StringReader(text));
        var resource = reader.ReadResourceRecord();

        resource.ShouldNotBeNull();
        resource.Name.ShouldBe("emanon.org");
        resource.Class.ShouldBe(DnsClass.IN);
        resource.Type.ShouldBe(DnsType.PTR);
        resource.TTL.ShouldBe(ResourceRecord.DefaultTTL);
        resource.ShouldBeOfType<PTRRecord>();
    }

    [Test]
    public void ReadResourceWithEscapedOrigin()
    {
        const string text = """
                            $ORIGIN emanon\.org. ; no such place\r\n
                            @ PTR localhost
                            """;
        var reader = new PresentationReader(new StringReader(text));
        var resource = reader.ReadResourceRecord();

        resource.ShouldNotBeNull();
        resource.Name.ShouldBe(@"emanon\.org");
        resource.Class.ShouldBe(DnsClass.IN);
        resource.Type.ShouldBe(DnsType.PTR);
        resource.TTL.ShouldBe(ResourceRecord.DefaultTTL);
        resource.ShouldBeOfType<PTRRecord>();
        resource.Name.ShouldNotBeNull();
        resource.Name.Labels.Count.ShouldBe(1);
    }

    [Test]
    public void ReadResourceWithTTL()
    {
        const string text = """
                            $TTL 120 ; 2 minutes\r\n
                            emanon.org PTR localhost
                            """;
        var reader = new PresentationReader(new StringReader(text));
        var resource = reader.ReadResourceRecord();

        resource.ShouldNotBeNull();
        resource.Name.ShouldBe("emanon.org");
        resource.Class.ShouldBe(DnsClass.IN);
        resource.Type.ShouldBe(DnsType.PTR);
        resource.TTL.ShouldBe(TimeSpan.FromMinutes(2));
        resource.ShouldBeOfType<PTRRecord>();
    }

    [Test]
    public void ReadResourceWithPreviousDomain()
    {
        const string text = """
                            emanon.org A 127.0.0.1
                                       AAAA ::1
                            """;
        var reader = new PresentationReader(new StringReader(text));
        var a = reader.ReadResourceRecord();

        a.ShouldNotBeNull();
        a.Name.ShouldBe("emanon.org");
        a.Class.ShouldBe(DnsClass.IN);
        a.Type.ShouldBe(DnsType.A);
        a.TTL.ShouldBe(ResourceRecord.DefaultTTL);
        a.ShouldBeOfType<ARecord>();

        var aaaa = reader.ReadResourceRecord();
        aaaa.ShouldNotBeNull();
        aaaa.Name.ShouldBe("emanon.org");
        aaaa.Class.ShouldBe(DnsClass.IN);
        aaaa.Type.ShouldBe(DnsType.AAAA);
        aaaa.TTL.ShouldBe(ResourceRecord.DefaultTTL);
        aaaa.ShouldBeOfType<AAAARecord>();
    }

    [Test]
    public void ReadResourceWithPreviousEscapedDomain()
    {
        const string text = """
                            emanon\126.org A 127.0.0.1
                                       AAAA ::1
                            """;
        var reader = new PresentationReader(new StringReader(text));
        var a = reader.ReadResourceRecord();

        a.ShouldNotBeNull();
        a.Name.ShouldBe("emanon~.org");
        a.Class.ShouldBe(DnsClass.IN);
        a.Type.ShouldBe(DnsType.A);
        a.TTL.ShouldBe(ResourceRecord.DefaultTTL);
        a.ShouldBeOfType<ARecord>();
        a.Name.ShouldNotBeNull();
        a.Name.Labels.Count.ShouldBe(2);

        var aaaa = reader.ReadResourceRecord();
        aaaa.ShouldNotBeNull();
        aaaa.Name.ShouldBe("emanon~.org");
        aaaa.Class.ShouldBe(DnsClass.IN);
        aaaa.Type.ShouldBe(DnsType.AAAA);
        aaaa.TTL.ShouldBe(ResourceRecord.DefaultTTL);
        aaaa.ShouldBeOfType<AAAARecord>();
        aaaa.Name.Labels.Count.ShouldBe(2);
    }

    [Test]
    public void ReadResourceWithLeadingEscapedDomainName()
    {
        const string text = @"\126emanon.org A 127.0.0.1";
        var reader = new PresentationReader(new StringReader(text));
        var a = reader.ReadResourceRecord();

        a.ShouldNotBeNull();
        a.Name.ShouldBe("~emanon.org");
        a.Class.ShouldBe(DnsClass.IN);
        a.Type.ShouldBe(DnsType.A);
        a.TTL.ShouldBe(ResourceRecord.DefaultTTL);
        a.ShouldBeOfType<ARecord>();
        a.Name.ShouldNotBeNull();
        a.Name.Labels.Count.ShouldBe(2);
    }

    [Test]
    public void ReadZoneFile()
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

        resources.Count.ShouldBe(15);
    }

    [Test]
    public void ReadResourceData()
    {
        var reader = new PresentationReader(new StringReader("\\# 0"));
        var rdata = reader.ReadResourceData();
        rdata.Length.ShouldBe(0);

        reader = new PresentationReader(new StringReader("\\# 3 abcdef"));
        rdata = reader.ReadResourceData();
        new byte[] { 0xab, 0xcd, 0xef }.ShouldBe(rdata);

        reader = new PresentationReader(new StringReader("\\# 3 ab cd ef"));
        rdata = reader.ReadResourceData();
        new byte[] { 0xab, 0xcd, 0xef }.ShouldBe(rdata);

        reader = new PresentationReader(new StringReader("\\# 3 abcd (\r\n  ef )"));
        rdata = reader.ReadResourceData();
        new byte[] { 0xab, 0xcd, 0xef }.ShouldBe(rdata);
    }

    [Test]
    public void ReadResourceData_MissingLeadin()
    {
        var reader = new PresentationReader(new StringReader("0"));
        Should.Throw<FormatException>(() => _ = reader.ReadResourceData());
    }

    [Test]
    public void ReadResourceData_BadHex_BadDigit()
    {
        var reader = new PresentationReader(new StringReader("\\# 3 ab cd ez"));
        Should.Throw<FormatException>(() => _ = reader.ReadResourceData());
    }

    [Test]
    public void ReadResourceData_BadHex_NotEven()
    {
        var reader = new PresentationReader(new StringReader("\\# 3 ab cd e"));
        Should.Throw<FormatException>(() => _ = reader.ReadResourceData());
    }

    [Test]
    public void ReadResourceData_BadHex_TooFew()
    {
        var reader = new PresentationReader(new StringReader("\\# 3 abcd"));
        Should.Throw<FormatException>(() => _ = reader.ReadResourceData());
    }

    [Test]
    public void ReadType()
    {
        var reader = new PresentationReader(new StringReader("A TYPE1 MX"));
        reader.ReadDnsType().ShouldBe(DnsType.A);
        reader.ReadDnsType().ShouldBe(DnsType.A);
        reader.ReadDnsType().ShouldBe(DnsType.MX);
    }

    [Test]
    public void ReadType_BadName()
    {
        var reader = new PresentationReader(new StringReader("BADNAME"));
        Should.Throw<Exception>(() => reader.ReadDnsType());
    }

    [Test]
    public void ReadType_BadDigit()
    {
        var reader = new PresentationReader(new StringReader("TYPEX"));
        Should.Throw<FormatException>(() => reader.ReadDnsType());
    }

    [Test]
    public void ReadMultipleStrings()
    {
        var expected = new List<string> { "abc", "def", "ghi" };
        var reader = new PresentationReader(new StringReader("abc def (\r\nghi)\r\n"));
        var actual = new List<string>();
        while (!reader.IsEndOfLine())
            actual.Add(reader.ReadString());

        actual.ShouldBe(expected);
    }

    [Test]
    public void ReadMultipleStrings2()
    {
        var expected = new List<string> { "abc", "def", "ghi", "jkl" };
        var reader = new PresentationReader(new StringReader("abc def (\r\nghi) jkl   \r\n"));
        var actual = new List<string>();
        while (!reader.IsEndOfLine())
            actual.Add(reader.ReadString());

        actual.ShouldBe(expected);
    }

    [Test]
    public void ReadMultipleStrings3()
    {
        var expected = new List<string> { "abc", "def", "ghi" };
        var reader = new PresentationReader(new StringReader("abc def (\rghi)\r"));
        var actual = new List<string>();
        while (!reader.IsEndOfLine())
            actual.Add(reader.ReadString());

        actual.ShouldBe(expected);
    }

    [Test]
    public void ReadMultipleStrings_LF()
    {
        var expected = new List<string> { "abc", "def" };
        var reader = new PresentationReader(new StringReader("abc def\rghi"));
        var actual = new List<string>();
        while (!reader.IsEndOfLine())
            actual.Add(reader.ReadString());

        actual.ShouldBe(expected);
    }

    [Test]
    public void ReadMultipleStrings_CRLF()
    {
        var expected = new List<string> { "abc", "def" };
        var reader = new PresentationReader(new StringReader("abc def\r\nghi"));
        var actual = new List<string>();
        while (!reader.IsEndOfLine())
            actual.Add(reader.ReadString());

        actual.ShouldBe(expected);
    }

    [Test]
    public void ReadBase64String()
    {
        var expected = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        var reader = new PresentationReader(new StringReader("AAECAwQFBgcICQoLDA0ODw=="));
        reader.ReadBase64String().ShouldBe(expected);

        reader = new PresentationReader(new StringReader("AAECAwQFBg  cICQoLDA0ODw=="));
        reader.ReadBase64String().ShouldBe(expected);

        reader = new PresentationReader(new StringReader("AAECAwQFBg  (\r\n  cICQo\r\n  LDA0ODw\r\n== )"));
        reader.ReadBase64String().ShouldBe(expected);
    }

    [Test]
    public void ReadDateTime()
    {
        DateTime expected = new(2004, 9, 16, 0, 0, 0, DateTimeKind.Utc);
        var reader = new PresentationReader(new StringReader("1095292800 20040916000000"));

        reader.ReadDateTime().ShouldBe(expected);
        reader.ReadDateTime().ShouldBe(expected);
    }

    [Test]
    public void ReadDomainName_Escaped()
    {
        var foo = new DomainName("foo.com");
        var drSmith = new DomainName(@"dr\. smith.com");
        var reader = new PresentationReader(new StringReader(@"dr\.\032smith.com foo.com"));

        reader.ReadDomainName().ShouldBe(drSmith);
        reader.ReadDomainName().ShouldBe(foo);
    }
}
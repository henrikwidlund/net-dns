using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Makaretu.Dns;
using Makaretu.Dns.Resolving;

namespace DnsTests.Resolving;

public class CatalogTest
{
    public const string ExampleDotOrgZoneText = """

                                                $ORIGIN example.org.
                                                $TTL 3600
                                                @    SOA   ns1 username.example.org. ( 2007120710 1 2 4 1 )
                                                     NS    ns1
                                                     NS    ns2
                                                     MX    10 mail
                                                ns1  A     192.0.2.1
                                                ns2  A     192.0.2.2
                                                mail A     192.0.2.3
                                                x    PTR   ns1
                                                _http._tcp   PTR a._http._tcp
                                                a._http._tcp SRV 0 5 80 mail
                                                             TXT needhttps=false needcredential=true

                                                """;

    public const string ExampleDotComZoneText = """

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
                                                wwwtest       IN  CNAME www                   ; wwwtest.example.com is another alias for www.example.com
                                                mail          IN  A     192.0.2.3             ; IPv4 address for mail.example.com
                                                mail2         IN  A     192.0.2.4             ; IPv4 address for mail2.example.com
                                                mail3         IN  A     192.0.2.5             ; IPv4 address for mail3.example.com

                                                """;
    
    [Test]
    public async Task IncludeZone()
    {
        var catalog = new Catalog();
        using var stringReader = new StringReader(ExampleDotComZoneText);
        var reader = new PresentationReader(stringReader);
        var zone = catalog.IncludeZone(reader);
        await Assert.That(zone.Name).IsEquatableOrEqualTo("example.com");

        await Assert.That(catalog).ContainsKey("example.com");
        await Assert.That(catalog).ContainsKey("ns.example.com");
        await Assert.That(catalog).ContainsKey("www.example.com");
        await Assert.That(catalog).ContainsKey("wwwtest.example.com");
        await Assert.That(catalog).ContainsKey("mail.example.com");
        await Assert.That(catalog).ContainsKey("mail2.example.com");
        await Assert.That(catalog).ContainsKey("mail3.example.com");

        await Assert.That(catalog["example.com"].Authoritative).IsTrue();
    }

    [Test]
    public async Task IncludeZone_AlreadyExists()
    {
        var catalog = new Catalog();
        var reader = new PresentationReader(new StringReader(ExampleDotComZoneText));
        var zone = catalog.IncludeZone(reader);
        await Assert.That(zone.Name).IsEquatableOrEqualTo("example.com");

        reader = new PresentationReader(new StringReader(ExampleDotComZoneText));
        await Assert.That(() => catalog.IncludeZone(reader)).ThrowsExactly<InvalidDataException>();
    }

    [Test]
    public async Task IncludeZone_NoResources()
    {
        var catalog = new Catalog();
        using var stringReader = new StringReader("");
        var reader = new PresentationReader(stringReader);
        await Assert.That(() => catalog.IncludeZone(reader)).ThrowsExactly<InvalidDataException>();
    }

    [Test]
    public async Task IncludeZone_MissingSOA()
    {
        const string text = "foo.org A 127.0.0.1";
        var catalog = new Catalog();
        using var stringReader = new StringReader(text);
        var reader = new PresentationReader(stringReader);
        await Assert.That(() => catalog.IncludeZone(reader)).ThrowsExactly<InvalidDataException>();
    }

    [Test]
    public async Task IncludeZone_InvalidName()
    {
        // Missing a new line
        const string text = ExampleDotOrgZoneText + " not.in.zone. A 127.0.0.1 ; bad";
        var catalog = new Catalog();
        using var stringReader = new StringReader(text);
        var reader = new PresentationReader(stringReader);
        await Assert.That(() => catalog.IncludeZone(reader)).ThrowsExactly<InvalidDataException>();
    }

    [Test]
    public async Task MultipleZones()
    {
        var catalog = new Catalog();

        using var stringReader = new StringReader(ExampleDotComZoneText);
        var reader = new PresentationReader(stringReader);
        var zone = catalog.IncludeZone(reader);
        await Assert.That(zone.Name).IsEquatableOrEqualTo("example.com");

        using var textReader = new StringReader(ExampleDotOrgZoneText);
        reader = new PresentationReader(textReader);
        zone = catalog.IncludeZone(reader);
        await Assert.That(zone.Name).IsEquatableOrEqualTo("example.org");
    }

    [Test]
    public async Task RemoveZone()
    {
        var catalog = new Catalog();

        using var stringReader = new StringReader(ExampleDotComZoneText);
        var reader = new PresentationReader(stringReader);
        var zone = catalog.IncludeZone(reader);
        await Assert.That(zone.Name).IsEquatableOrEqualTo("example.com");
        await Assert.That(catalog).Count().IsEqualTo(7);

        using var textReader = new StringReader(ExampleDotOrgZoneText);
        reader = new PresentationReader(textReader);
        zone = catalog.IncludeZone(reader);
        await Assert.That(zone.Name).IsEquatableOrEqualTo("example.org");
        await Assert.That(catalog).Count().IsEqualTo(14);

        catalog.RemoveZone("example.org");
        await Assert.That(catalog).Count().IsEqualTo(7);

        catalog.RemoveZone("example.com");
        await Assert.That(catalog).Count().IsEqualTo(0);
    }

    [Test]
    public async Task NamesAreCaseInsenstive()
    {
        var catalog = new Catalog();
        using var stringReader = new StringReader(ExampleDotComZoneText);
        var reader = new PresentationReader(stringReader);
        catalog.IncludeZone(reader);

        await Assert.That(catalog).ContainsKey("EXAMPLE.COM");
        await Assert.That(catalog).ContainsKey("NS.EXAMPLE.COM");
        await Assert.That(catalog).ContainsKey("WWW.EXAMPLE.COM");
        await Assert.That(catalog).ContainsKey("WWWTEST.EXAMPLE.COM");
        await Assert.That(catalog).ContainsKey("MAIL.EXAMPLE.COM");
        await Assert.That(catalog).ContainsKey("MAIL2.EXAMPLE.COM");
        await Assert.That(catalog).ContainsKey("MAIL3.EXAMPLE.COM");
    }

    [Test]
    public async Task AddResource()
    {
        var a = AddressRecord.Create("foo", IPAddress.Loopback);
        var aaaa = AddressRecord.Create("foo", IPAddress.IPv6Loopback);
        var catalog = new Catalog();
        var n1 = catalog.Add(a, true);
        await Assert.That(n1.Authoritative).IsTrue();
        await Assert.That(n1.Resources).Contains(a);

        var n2 = catalog.Add(aaaa);
        await Assert.That(n1).IsSameReferenceAs(n2);
        await Assert.That(n1.Authoritative).IsTrue();
        await Assert.That(n1.Resources).Contains(a);
        await Assert.That(n1.Resources).Contains(aaaa);
    }

    [Test]
    public async Task AddResource_Same()
    {
        var a = AddressRecord.Create("foo", IPAddress.Loopback);
        var catalog = new Catalog();
        var n1 = catalog.Add(a);
        await Assert.That(n1.Resources).Contains(a);

        var n2 = catalog.Add(a);
        await Assert.That(n1).IsSameReferenceAs(n2);
        await Assert.That(n1.Resources).Contains(a);
        await Assert.That(n1.Resources).Count().IsEqualTo(1);
    }

    [Test]
    public async Task AddResource_Duplicate()
    {
        var a = AddressRecord.Create("foo", IPAddress.Loopback);
        var b = AddressRecord.Create("foo", IPAddress.Loopback);
        await Assert.That(a).IsEqualTo(b);

        var catalog = new Catalog();
        var n1 = catalog.Add(a);
        await Assert.That(n1.Resources).Contains(a);

        var n2 = catalog.Add(b);
        await Assert.That(n1).IsSameReferenceAs(n2);
        await Assert.That(n1.Resources).Contains(a);
        await Assert.That(n1.Resources).Contains(b);
        await Assert.That(n1.Resources).Count().IsEqualTo(1);
    }

    [Test]
    public async Task AddResource_Latest()
    {
        var a = AddressRecord.Create("foo", IPAddress.Loopback);
        var b = AddressRecord.Create("foo", IPAddress.Loopback);
        a.TTL = TimeSpan.FromHours(2);
        b.CreationTime = a.CreationTime + TimeSpan.FromHours(1);
        b.TTL = TimeSpan.FromHours(3);
        await Assert.That(a).IsEqualTo(b);

        var catalog = new Catalog();
        var n1 = catalog.Add(a);
        await Assert.That(n1.Resources).Contains(a);

        var n2 = catalog.Add(b);
        await Assert.That(n1).IsSameReferenceAs(n2);
        await Assert.That(n1.Resources).Contains(a);
        await Assert.That(n1.Resources).Contains(b);
        await Assert.That(n1.Resources).Count().IsEqualTo(1);
        await Assert.That(n1.Resources.First().CreationTime).IsEqualTo(b.CreationTime);
        await Assert.That(n1.Resources.First().TTL).IsEqualTo(b.TTL);
    }

    [Test]
    public async Task RootHints()
    {
        var catalog = new Catalog();
        var root = catalog.IncludeRootHints();
        await Assert.That(root.Name).IsEquatableOrEqualTo("");
        await Assert.That(root.Authoritative).IsTrue();
        await Assert.That(root.Resources.OfType<NSRecord>()).IsNotEmpty();
    }

    [Test]
    public async Task CanonicalOrder()
    {
        var catalog = new Catalog
        {
            AddressRecord.Create("*.z.example", IPAddress.Loopback),
            AddressRecord.Create("a.example", IPAddress.Loopback),
            AddressRecord.Create("yljkjljk.a.example", IPAddress.Loopback),
            AddressRecord.Create("Z.a.example", IPAddress.Loopback),
            AddressRecord.Create("zABC.a.EXAMPLE", IPAddress.Loopback),
            AddressRecord.Create("z.example", IPAddress.Loopback),
            AddressRecord.Create("!.z.example", IPAddress.Loopback),
            AddressRecord.Create("~.z.example", IPAddress.Loopback),
            AddressRecord.Create("example", IPAddress.Loopback)
        };

        var expected = new DomainName[]
        {
            "example", "a.example", "yljkjljk.a.example",
            "Z.a.example", "zABC.a.EXAMPLE", "z.example",
            "!.z.example", "*.z.example", "~.z.example"
        };
        var actual = catalog
            .NodesInCanonicalOrder()
            .Select(static node => node.Name)
            .ToArray();
        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task Include()
    {
        const string dig = """

                           ; <<>> DiG 9.9.5-3ubuntu0.15-Ubuntu <<>> +dnssec +split=8 +all com DNSKEY
                           ;; global options: +cmd
                           ;; Got answer:
                           ;; ->>HEADER<<- opcode: QUERY, status: NOERROR, id: 33952
                           ;; flags: qr rd ra; QUERY: 1, ANSWER: 3, AUTHORITY: 0, ADDITIONAL: 1

                           ;; OPT PSEUDOSECTION:
                           ; EDNS: version: 0, flags: do; udp: 512
                           ;; QUESTION SECTION:
                           ;com.				IN	DNSKEY

                           ;; ANSWER SECTION:
                           com.			54254	IN	DNSKEY	256 3 8 AQPeabgR 6Fgrk5FS LilDYUed wsHA0HH2 2e8+Zp/u vp4aj1dV DAy5C9bk RA+xot3s G1KaT5hv goE7eNV9 3F7pBW9r vVE3A/BN vJbLXxKh kAJV5KMF C10NRcdb +xF+sM4X TMPESPrY wTLUEpSF ntMIVLAt UzLaBo6Y pTVR20os gGgc3Q==  ; ZSK; alg = RSASHA256; key id = 46475
                           com.			54254	IN	DNSKEY	257 3 8 AQPDzldN mMvZFX4N cNJ0uEnK Dg7tmv/F 3MyQR0lp BmVcNcsI szxNFxsB fKNW9JYC Yqpik836 6LE7VbIc NRzfp2h9 OO8HRl+H +E08zauK 8k7evWEm u/6od+2b oggPoiEf GNyvNPaS I7FOIroD snw/tagg zHRX1Z7S OiOiPWPN IwSUyWOZ 79VmcQ1G LkC6NlYv G3HwYmyn Qv6oFwGv /KELSw7Z SdrbTQ0H XvZbqMUI 7BaMskmv gm1G7oKZ 1YiF7O9i oVNc0+7A SbqmZN7Z 98EGU/Qh 2K/BgUe8 Hs0XVcdP KrtyYnoQ Hd2ynKPc MMlTEih2 /2HDHjRP J2aywIpK Nnv4oPo/  ; KSK; alg = RSASHA256; key id = 30909
                           com.			54254	IN	RRSIG	DNSKEY 8 1 86400 20180909182533 20180825182033 30909 com. k2uOB+mv 1Fu2Uy+g Y/vF4xQB oyUo8dgg 4d491Z6C Pi551Lh2 /oTWxVQX fWzE8rUk VBCNPkSH sC0BtInK 8iUOqqoE TDDt8wp3 u6zqzc3t 8nXBPIBD G9RIbciY HwxIQWWJ x55J5fcT Hv51nKqK jHCU0Tfr N95Lqg79 qwesU0E3 HP7Lvq9K UppCgnDx nYDxQqz4 Unq3F4Ts 1T+/lwNF xkUs2jY6 EGwIgNjW 0gpAJJJd bsd0pbsi Sn21ydMz pL+gpPru 0zQKHn67 r5kDUMxQ FnRd691C ZLUAWLBK 0NxSfXYS Xj+VjKAa 7WHgvNoU YPDfK216 4XKibHsb +BOAkj2X Aa04XA==

                           ;; Query time: 1 msec
                           ;; SERVER: 172.17.0.1#53(172.17.0.1)
                           ;; WHEN: Mon Sep 03 01:31:04 UTC 2018
                           ;; MSG SIZE  rcvd: 743

                           """;
        
        var catalog = new Catalog();
        var reader = new PresentationReader(new StringReader(dig));
        catalog.Include(reader);
        await Assert.That(catalog).ContainsKey("com");

        var node = catalog["COM"];
        await Assert.That(node.Resources).Count().IsEqualTo(3);
    }

    [Test]
    public async Task IncludeReverseLookupRecords()
    {
        var catalog = new Catalog();
        var reader = new PresentationReader(new StringReader(ExampleDotComZoneText));
        _ = catalog.IncludeZone(reader);
        catalog.IncludeReverseLookupRecords();

        await Assert.That(catalog).ContainsKey("1.2.0.192.in-addr.arpa");
        await Assert.That(catalog).ContainsKey("2.2.0.192.in-addr.arpa");
        await Assert.That(catalog).ContainsKey("3.2.0.192.in-addr.arpa");
        await Assert.That(catalog).ContainsKey("1.2.0.192.in-addr.arpa");

        await Assert.That(catalog["1.2.0.192.in-addr.arpa"].Authoritative).IsTrue();
    }
}
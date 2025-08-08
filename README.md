Forks from the following, but modernized and made async:
- [richardschneider/net-dns](https://github.com/richardschneider/net-dns)
- [richardschneider/net-mdns](https://github.com/richardschneider/net-mdns)
- [jdomnitz/net-dns](https://github.com/jdomnitz/net-dns)
- [jdomnitz/net-mdns](https://github.com/jdomnitz/net-mdns)

### Buy the original author a coffee
<a href="https://www.buymeacoffee.com/kmXOxKJ4E" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/yellow_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>

# net-dns

DNS data model with serializer/deserializer for the wire and "master file" format.

## Features

- Serialization for the wire and master file formats
- Pretty printing of messages
- Supports compressed domain names
- Supports multiple strings in TXT records
- Supports the extended 12-bit RCODE
- Future proof: handles unknown resource records and EDNS options
- Graceful truncation of messages
- A name server that answeres DNS questions
- Data models for
    - [RFC 1035](https://tools.ietf.org/html/rfc1035) Domain Names (DNS)
    - [RFC 1183](https://tools.ietf.org/html/rfc1183) New DNS RR Definitions
    - [RFC 1996](https://tools.ietf.org/html/rfc1996) Zone Changes (DNS NOTIFY)
    - [RFC 2136](https://tools.ietf.org/html/rfc2136) Dynamic Updates (DNS UPDATE)
    - [RFC 2845](https://tools.ietf.org/html/rfc2845) Secret Key Transaction Authentication for DNS (TSIG)
    - [RFC 2930](https://tools.ietf.org/html/rfc2930) Secret Key Establishment for DNS (TKEY RR)
    - [RFC 3225](https://tools.ietf.org/html/rfc3225) Indicating Resolver Support of DNSSEC
    - [RFC 3599](https://tools.ietf.org/html/rfc3596) DNS Extensions to Support IPv6
    - [RFC 4034](https://tools.ietf.org/html/rfc4034) Resource Records for the DNS Security Extensions (DNSSEC)
    - [RFC 5001](https://tools.ietf.org/html/rfc5001) DNS Name Server Identifier (NSID) Option
    - [RFC 6672](https://tools.ietf.org/html/rfc6672) DNAME Redirection in the DNS
    - [RFC 6891](https://tools.ietf.org/html/rfc6891) Extension Mechanisms for DNS (EDNS(0))
    - [RFC 7828](https://tools.ietf.org/html/rfc7828) The edns-tcp-keepalive EDNS0 Option
    - [RFC 7830](https://tools.ietf.org/html/rfc7830) The EDNS(0) Padding Option
- Targets .Net Framework 4.5 and 4.7.2 and .NET Standard 1.4 and 2.0
- CI on Travis (Ubuntu Trusty and OSX) and AppVeyor (Windows Server 2016)

## Getting started

Published releases are available on [NuGet](https://www.nuget.org/packages/Henrik.Makaretu.Dns/).  To install, run the following command in the [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console).

    PM> Install-Package Henrik.Makaretu.Dns

## Usage

### Name Server

Create a name server that can answer questions for a zone.

```csharp
using Makaretu.Dns.Resolving;

var catalog = new Catalog();
catalog.IncludeZone(...);
catalog.IncludeRootHints();
var resolver = new NameServer { Catalog = catalog };
```

Answer a question

```csharp
var request = new Message();
request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.AAAA });
var response = await resolver.ResolveAsync(request);
```

### Data Model

```csharp
using Makaretu.Dns

var msg = new Message
{
	AA = true,
	QR = true,
	Id = 1234
};
msg.Questions.Add(new Question 
{ 
	Name = "emanon.org" 
});
msg.Answers.Add(new ARecord 
{ 
	Name = "emanon.org",
	Address = IPAddress.Parse("127.0.0.1") 
});
msg.AuthorityRecords.Add(new SOARecord
{
	Name = "emanon.org",
	PrimaryName = "erehwon",
	Mailbox = "hostmaster.emanon.org"
});
msg.AdditionalRecords.Add(new ARecord 
{ 
	Name = "erehwon", 
	Address = IPAddress.Parse("127.0.0.1") 
});

```

# Related projects

- [net-mdns](https://github.com/richardschneider/net-mdns) - client and server for multicast DNS
- [net-udns](https://github.com/richardschneider/net-udns) - client for unicast DNS, DNS over HTTPS (DOH) and DNS over TLS (DOT)
- [DNSSEC](https://www.icann.org/resources/pages/dnssec-qaa-2014-01-29-en) -  What Is It and Why Is It Important?

# License
Copyright © 2018 Richard Schneider (makaretu@gmail.com)

The package is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refer to the [LICENSE](https://github.com/richardschneider/net-dns/blob/master/LICENSE) file for more information.


# net-mdns

[![CI](https://img.shields.io/github/actions/workflow/status/henrikwidlund/net-dns/ci.yml?label=CI&logo=github)](https://github.com/henrikwidlund/net-dns/actions/workflows/ci.yml)
[![Version](https://img.shields.io/nuget/v/Henrik.Makaretu.Dns.Multicast.svg)](https://www.nuget.org/packages/Henrik.Makaretu.Dns.Multicast)
[![docs](https://cdn.rawgit.com/richardschneider/net-mdns/master/doc/images/docs-latest-green.svg)](https://richardschneider.github.io/net-mdns/articles/intro.html)

A simple Multicast Domain Name Service based on [RFC 6762](https://tools.ietf.org/html/rfc6762).  Can be used
as both a client (sending queries) or a server (responding to queries).

A higher level DNS Service Discovery based on [RFC 6763](https://tools.ietf.org/html/rfc6763) that automatically responds to any query for the
service or service instance.

## Features

- Targets Framework 4.6.1, .NET Standard 1.4 and 2.0
- Supports IPv6 and IPv4 platforms
- CI on Circle (Debian GNU/Linux), Travis (Ubuntu Xenial and OSX) and AppVeyor (Windows Server 2016)
- Detects new and/or removed network interfaces
- Supports multicasting on multiple network interfaces
- Supports reverse address mapping
- Supports service subtypes (features)
- Handles legacy unicast queries, see #61

## Getting started

Published releases are available on [NuGet](https://www.nuget.org/packages/Henrik.Makaretu.Dns.Multicast/).  To install, run the following command in the [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console)

    PM> Install-Package Henrik.Makaretu.Dns.Multicast

or using .NET CLI run the following command in the project folder

    > dotnet add package Henrik.Makaretu.Dns.Multicast

## Usage Service Discovery

### Advertising

Always broadcast the service ("foo") running on local host with port 1024.

```csharp
using Makaretu.Dns;

var service = new ServiceProfile("x", "_foo._tcp", 1024);
var sd = new ServiceDiscovery();
sd.Advertise(service);
```

See the [example advertiser](Spike/Program.cs) for a working program.

### Discovery

Find all services running on the local link.

```csharp
using Makaretu.Dns;

var sd = new ServiceDiscovery();
sd.ServiceDiscovered += (s, serviceName) => { // Do something };
```

Find all service instances running on the local link.

```csharp
using Makaretu.Dns;

var sd = new ServiceDiscovery();
sd.ServiceInstanceDiscovered += (s, e) => { // Do something };
```

See the [example browser](Browser/Program.cs) for a working program.

## Usage Multicast

### Event Based Queries

Get all the Apple TVs. The query is sent when a network interface is discovered.
The `AnsweredReceived` callback contains any answer that is seen, not just the answer
to the specific query.

```csharp
using Makaretu.Dns;

var mdns = new MulticastService();
mdns.NetworkInterfaceDiscovered += (s, e) => mdns.SendQuery("appletv.local");
mdns.AnswerReceived += (s, e) => { // do something with e.Message };
mdns.Start();
```

### Async Queries

Get the first answer to Apple TVs. Wait 2 seconds for an answer.

```csharp
using Makaretu.Dns;

var service = "appletv.local";
var query = new Message();
query.Questions.Add(new Question { Name = service, Type = DnsType.ANY });
var cancellation = new CancellationTokenSource(2000);

using (var mdns = new MulticastService())
{
    mdns.Start();
    var response = await mdns.ResolveAsync(query, cancellation.Token);
    // Do something
}
```

### Broadcasting

Respond to a query for the service.  Note that `ServiceDiscovery.Advertise` is much easier.

```csharp
using Makaretu.Dns;

var service = "...";
var mdns = new MulticastService();
mdns.QueryReceived += (s, e) =>
{
    var msg = e.Message;
    if (msg.Questions.Any(q => q.Name == service))
    {
        var res = msg.CreateResponse();
        var addresses = MulticastService.GetIPAddresses()
            .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        foreach (var address in addresses)
        {
            res.Answers.Add(new ARecord
            {
                Name = service,
                Address = address
            });
        }
        mdns.SendAnswer(res);
    }
};
mdns.Start();
```

## Related projects

- [net-dns](https://github.com/richardschneider/net-dns) - DNS data model and Name Server with serializer for the wire and master file format
- [net-udns](https://github.com/richardschneider/net-udns) - client for unicast DNS, DNS over HTTPS (DOH) and DNS over TLS (DOT)

## License
Copyright © 2018-2019 Richard Schneider (makaretu@gmail.com)

The package is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refer to the [LICENSE](https://github.com/richardschneider/net-mdns/blob/master/LICENSE) file for more information.

- [richardschneider/net-dns](https://github.com/richardschneider/net-dns/blob/master/LICENSE)
- [richardschneider/net-mdns](https://github.com/richardschneider/net-mdns/blob/master/LICENSE)
- [jdomnitz/net-dns](https://github.com/jdomnitz/net-dns/blob/master/LICENSE)
- [jdomnitz/net-mdns](https://github.com/jdomnitz/net-mdns/blob/master/LICENSE)

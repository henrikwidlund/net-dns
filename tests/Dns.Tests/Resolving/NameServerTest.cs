using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Makaretu.Dns;
using Makaretu.Dns.Resolving;

namespace DnsTests.Resolving;

public class NameServerTest
{
    private readonly Catalog _dotcom = new();
    private readonly Catalog _dotorg = new();

    public NameServerTest()
    {
        _dotcom.IncludeZone(new PresentationReader(new StringReader(CatalogTest.ExampleDotComZoneText)));
        _dotorg.IncludeZone(new PresentationReader(new StringReader(CatalogTest.ExampleDotOrgZoneText)));
    }

    [Test]
    public async Task Simple()
    {
        var resolver = new NameServer { Catalog = _dotcom };
        var question = new Question { Name = "ns.example.com", Type = DnsType.A };
        var response = resolver.Resolve(question, cancel: TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(1);
        await Assert.That(response.Answers[0].Type).IsEqualTo(DnsType.A);
    }

    [Test]
    public async Task Missing_Name()
    {
        var resolver = new NameServer { Catalog = _dotcom };
        var question = new Question { Name = "foo.bar.example.com", Type = DnsType.A };
        var response = resolver.Resolve(question, cancel: TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NameError);

        await Assert.That(response.AuthorityRecords).HasCount().GreaterThan(0);
        var authority = response.AuthorityRecords.OfType<SOARecord>().First();
        await Assert.That(authority.Name).IsEquatableOrEqualTo("example.com");
    }

    [Test]
    public async Task Missing_Type()
    {
        var resolver = new NameServer { Catalog = _dotcom };
        var question = new Question { Name = "ns.example.com", Type = DnsType.MX };
        var response = resolver.Resolve(question, cancel: TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NameError);
    }

    [Test]
    public async Task Missing_Class()
    {
        var resolver = new NameServer { Catalog = _dotcom };
        var question = new Question { Name = "ns.example.com", Class = DnsClass.CH };
        var response = resolver.Resolve(question, cancel: TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NameError);
    }

    [Test]
    public async Task AnyType()
    {
        var resolver = new NameServer { Catalog = _dotcom };
        var question = new Question { Name = "ns.example.com", Type = DnsType.ANY };
        var response = resolver.Resolve(question, cancel: TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(2);
    }

    [Test]
    public async Task AnyClass()
    {
        var resolver = new NameServer { Catalog = _dotcom };
        var question = new Question { Name = "ns.example.com", Class = DnsClass.ANY, Type = DnsType.A };
        var response = resolver.Resolve(question, cancel: TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsFalse();
        await Assert.That(response.Answers).HasCount(1);
    }

    [Test]
    public async Task Alias()
    {
        var resolver = new NameServer { Catalog = _dotcom };
        var question = new Question { Name = "www.example.com", Type = DnsType.A };
        var response = resolver.Resolve(question, cancel: TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);

        await Assert.That(response.Answers).HasCount(2);
        await Assert.That(response.Answers[0].Type).IsEqualTo(DnsType.CNAME);
        await Assert.That(response.Answers[1].Type).IsEqualTo(DnsType.A);
    }

    [Test]
    public async Task Alias_BadZoneTarget()
    {
        var resolver = new NameServer { Catalog = _dotcom };
        var ftp = new Node { Name = "ftp.example.com", Authoritative = true };
        ftp.Resources.Add(new CNAMERecord { Name = ftp.Name, Target = "ftp-server.example.com" });
        resolver.Catalog.TryAdd(ftp.Name, ftp);
        var question = new Question { Name = "ftp.example.com", Type = DnsType.A };
        var response = resolver.Resolve(question, cancel: TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NameError);

        await Assert.That(response.Answers).HasCount(1);
        await Assert.That(response.Answers[0].Type).IsEqualTo(DnsType.CNAME);

        await Assert.That(response.AuthorityRecords).HasCount(1);
        var authority = response.AuthorityRecords.OfType<SOARecord>().First();
        await Assert.That(authority.Name).IsEquatableOrEqualTo("example.com");
    }

    [Test]
    public async Task Alias_BadInterZoneTarget()
    {
        var resolver = new NameServer { Catalog = _dotcom };
        var bad = new Node { Name = "bad.example.com", Authoritative = true };
        bad.Resources.Add(new CNAMERecord { Name = bad.Name, Target = "somewhere-else.org" });
        resolver.Catalog.TryAdd(bad.Name, bad);
        var question = new Question { Name = "bad.example.com", Type = DnsType.A };
        var response = resolver.Resolve(question, cancel: TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NameError);

        await Assert.That(response.Answers).HasCount(1);
        await Assert.That(response.Answers[0].Type).IsEqualTo(DnsType.CNAME);

        await Assert.That(response.AuthorityRecords).HasCount().Zero();
    }

    [Test]
    public async Task MultipleQuestions_AnswerAny()
    {
        var resolver = new NameServer { Catalog = _dotcom };
        var request = new Message();
        request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.A });
        request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.AAAA });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);
        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(1);
    }

    [Test]
    public async Task MultipleQuestions_SomeQuestionNoAnswer_AnswerAny()
    {
        var resolver = new NameServer { Catalog = _dotcom };
        var request = new Message();
        request.Questions.Add(new Question { Name = "unknown-name.com", Type = DnsType.AAAA });
        request.Questions.Add(new Question { Name = "unknown-name.example.com", Type = DnsType.AAAA });
        request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.A });
        request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.AAAA });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(1);
    }

    [Test]
    public async Task MultipleQuestions_AnswerAll()
    {
        var resolver = new NameServer { Catalog = _dotcom, AnswerAllQuestions = true};
        var request = new Message();
        request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.A });
        request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.AAAA });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(2);
    }

    [Test]
    public async Task MultipleQuestions_SomeQuestionNoAnswer_AnswerAll()
    {
        var resolver = new NameServer { Catalog = _dotcom, AnswerAllQuestions = true };
        var request = new Message();
        request.Questions.Add(new Question { Name = "unknown-name.com", Type = DnsType.AAAA });
        request.Questions.Add(new Question { Name = "unknown-name.example.com", Type = DnsType.AAAA });
        request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.AAAA });
        request.Questions.Add(new Question { Name = "ns.example.com", Type = DnsType.A });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(2);
        await Assert.That(response.AuthorityRecords).HasCount(3);
    }

    [Test]
    public async Task AdditionalRecords_PTR_WithAddresses()
    {
        var resolver = new NameServer { Catalog = _dotorg };
        var request = new Message();
        request.Questions.Add(new Question { Name = "x.example.org", Type = DnsType.PTR });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(1);

        await Assert.That(response.AdditionalRecords).HasCount(2);
        await Assert.That(response.AdditionalRecords[0].Type).IsEqualTo(DnsType.A);
        await Assert.That(response.AdditionalRecords[0].Name).IsEquatableOrEqualTo("ns1.example.org");
    }

    [Test]
    public async Task AdditionalRecords_PTR_WithSRV()
    {
        var resolver = new NameServer { Catalog = _dotorg };
        var request = new Message();
        request.Questions.Add(new Question { Name = "_http._tcp.example.org", Type = DnsType.PTR });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(1);

        await Assert.That(response.AdditionalRecords.Any(static a => a.Type == DnsType.SRV)).IsTrue();
        await Assert.That(response.AdditionalRecords.Any(static a => a.Type == DnsType.TXT)).IsTrue();
        await Assert.That(response.AdditionalRecords.Any(static a => a.Type == DnsType.A)).IsTrue();
    }

    [Test]
    public async Task AdditionalRecords_NS()
    {
        var resolver = new NameServer { Catalog = _dotorg };
        var request = new Message();
        request.Questions.Add(new Question { Name = "example.org", Type = DnsType.NS });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(2);

        await Assert.That(response.AdditionalRecords).HasCount(2);
        await Assert.That(response.AdditionalRecords.All(static r => r.Type == DnsType.A)).IsTrue();
    }

    [Test]
    public async Task AdditionalRecords_SOA()
    {
        var resolver = new NameServer { Catalog = _dotorg };
        var request = new Message();
        request.Questions.Add(new Question { Name = "example.org", Type = DnsType.SOA });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(1);

        await Assert.That(response.AdditionalRecords).HasCount(2);
        await Assert.That(response.AdditionalRecords[0].Type).IsEqualTo(DnsType.A);
        await Assert.That(response.AdditionalRecords[0].Name).IsEquatableOrEqualTo("ns1.example.org");
    }

    [Test]
    public async Task AdditionalRecords_SRV()
    {
        var resolver = new NameServer { Catalog = _dotorg };
        var request = new Message();
        request.Questions.Add(new Question { Name = "a._http._tcp.example.org", Type = DnsType.SRV });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(1);

        await Assert.That(response.AdditionalRecords.OfType<TXTRecord>().Any()).IsTrue();
        await Assert.That(response.AdditionalRecords.OfType<ARecord>().Any()).IsTrue();
    }

    [Test]
    public async Task AdditionalRecords_A()
    {
        var resolver = new NameServer { Catalog = _dotcom };
        var request = new Message();
        request.Questions.Add(new Question { Name = "example.com", Type = DnsType.A });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(1);
        await Assert.That(response.Answers).All(static r => r.Type == DnsType.A);

        await Assert.That(response.AdditionalRecords).Any(static r => r.Name == "example.com" && r.Type == DnsType.AAAA);
    }

    [Test]
    public async Task AdditionalRecords_AAAA()
    {
        var resolver = new NameServer { Catalog = _dotcom };
        var request = new Message();
        request.Questions.Add(new Question { Name = "example.com", Type = DnsType.AAAA });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(1);
        await Assert.That(response.Answers).All(static r => r.Type == DnsType.AAAA);

        await Assert.That(response.AdditionalRecords).Any(static r => r.Name == "example.com" && r.Type == DnsType.A);
    }

    [Test]
    public async Task AdditionalRecords_NoDuplicates()
    {
        var resolver = new NameServer { Catalog = _dotorg,  AnswerAllQuestions = true };
        var request = new Message();
        request.Questions.Add(new Question { Name = "example.org", Type = DnsType.NS });
        request.Questions.Add(new Question { Name = "ns1.example.org", Type = DnsType.A });
        request.Questions.Add(new Question { Name = "ns2.example.org", Type = DnsType.A });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Answers).HasCount(4);

        await Assert.That(response.AdditionalRecords).HasCount().Zero();
    }

    [Test]
    public async Task EscapedDotDomainName()
    {
        var catalog = new Catalog
        {
            new ARecord { Name = "a.b", Address = IPAddress.Parse("127.0.0.2") },
            new ARecord { Name = @"a\\.b", Address = IPAddress.Parse("127.0.0.3") }
        };
        var resolver = new NameServer { Catalog = catalog };

        var request = new Message();
        request.Questions.Add(new Question { Name = "a.b", Type = DnsType.A });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        var answer = response.Answers.OfType<ARecord>().First();
        await Assert.That(answer.Address).IsNotNull();
        await Assert.That(answer.Address!.ToString()).IsEqualTo("127.0.0.2");

        request = new Message();
        request.Questions.Add(new Question { Name = @"a\\.b", Type = DnsType.A });
        response = await resolver.ResolveAsync(request, TestContext.Current.Execution.CancellationToken);
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        answer = response.Answers.OfType<ARecord>().First();
        await Assert.That(answer.Address).IsNotNull();
        await Assert.That(answer.Address!.ToString()).IsEqualTo("127.0.0.3");
    }

    [Test]
    public async Task RoundTrip_EscapedDotDomainName()
    {
        var catalog = new Catalog
        {
            new ARecord { Name = "a.b", Address = IPAddress.Parse("127.0.0.2") },
            new ARecord { Name = @"a\.b", Address = IPAddress.Parse("127.0.0.3") }
        };
        var resolver = new NameServer { Catalog = catalog };

        var request = new Message();
        request.Questions.Add(new Question { Name = @"a\.b", Type = DnsType.A });
        var bin = request.ToByteArray();
        var r1 = new Message();
        r1.Read(bin);

        var response = await resolver.ResolveAsync(r1, TestContext.Current!.Execution.CancellationToken);
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        var answer = response.Answers.OfType<ARecord>().First();
        await Assert.That(answer.Address).IsNotNull();
        await Assert.That(answer.Address!.ToString()).IsEqualTo("127.0.0.3");
    }
}
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Makaretu.Dns;
using Makaretu.Dns.Resolving;

namespace DnsTests.Resolving;

public class SecureNameServerTest
{
    private readonly Catalog _example = new();

    public SecureNameServerTest() => _example.IncludeZone(new PresentationReader(new StringReader(SecureCatalogTest.ExampleZoneText)));

    [Test]
    public async Task SupportDnssec()
    {
        var resolver = new NameServer { Catalog = _example };
        var request = new Message();
        request.Questions.Add(new Question { Name = "x.w.example", Type = DnsType.MX });

        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);
        await Assert.That(response.DO).IsFalse();

        request.UseDnsSecurity();
        response = await resolver.ResolveAsync(request, TestContext.Current.Execution.CancellationToken);
        await Assert.That(response.DO).IsTrue();
    }

    [Test]
    public async Task QueryWithoutDo()
    {
        var resolver = new NameServer { Catalog = _example };
        var request = new Message();
        request.Questions.Add(new Question { Name = "x.w.example", Type = DnsType.MX });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.DO).IsFalse();
    }

    [Test]
    public async Task QueryWithDo()
    {
        var resolver = new NameServer { Catalog = _example };
        var request = new Message().UseDnsSecurity();
        request.Questions.Add(new Question { Name = "x.w.example", Type = DnsType.MX });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.DO).IsTrue();
    }

    [Test]
    public async Task SecureQueryHasSignature()
    {
        // See https://tools.ietf.org/html/rfc4035#appendix-B.1

        var resolver = new NameServer { Catalog = _example };
        var request = new Message().UseDnsSecurity();
        request.Questions.Add(new Question { Name = "x.w.example", Type = DnsType.MX });
        var response = await resolver.ResolveAsync(request, TestContext.Current!.Execution.CancellationToken);

        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.DO).IsTrue();

        await Assert.That(response.Answers).HasCount(2);
        await Assert.That(response.Answers.OfType<MXRecord>()).HasCount(1);
        await Assert.That(response.Answers.OfType<RRSIGRecord>()).HasCount(1);

        await Assert.That(response.AuthorityRecords).HasCount(3);
        await Assert.That(response.AuthorityRecords.OfType<NSRecord>()).HasCount(2);
        await Assert.That(response.AuthorityRecords.OfType<RRSIGRecord>()).HasCount(1);
    }
}
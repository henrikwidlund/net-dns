using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Makaretu.Dns;
using Makaretu.Dns.Resolving;
using Shouldly;
using Xunit;

namespace DnsTests.Resolving;

public class SecureNameServerTest
{
    private readonly Catalog _example = new();

    public SecureNameServerTest() => _example.IncludeZone(new PresentationReader(new StringReader(SecureCatalogTest.ExampleZoneText)));

    [Fact]
    public async Task SupportDnssec()
    {
        var resolver = new NameServer { Catalog = _example };
        var request = new Message();
        request.Questions.Add(new Question { Name = "x.w.example", Type = DnsType.MX });

        var response = await resolver.ResolveAsync(request);
        response.DO.ShouldBeFalse();

        request.UseDnsSecurity();
        response = await resolver.ResolveAsync(request);
        response.DO.ShouldBeTrue();
    }

    [Fact]
    public async Task QueryWithoutDo()
    {
        var resolver = new NameServer { Catalog = _example };
        var request = new Message();
        request.Questions.Add(new Question { Name = "x.w.example", Type = DnsType.MX });
        var response = await resolver.ResolveAsync(request);

        response.IsResponse.ShouldBeTrue();
        response.Status.ShouldBe(MessageStatus.NoError);
        response.AA.ShouldBeTrue();
        response.DO.ShouldBeFalse();
    }

    [Fact]
    public async Task QueryWithDo()
    {
        var resolver = new NameServer { Catalog = _example };
        var request = new Message().UseDnsSecurity();
        request.Questions.Add(new Question { Name = "x.w.example", Type = DnsType.MX });
        var response = await resolver.ResolveAsync(request);

        response.IsResponse.ShouldBeTrue();
        response.Status.ShouldBe(MessageStatus.NoError);
        response.AA.ShouldBeTrue();
        response.DO.ShouldBeTrue();
    }

    [Fact]
    public async Task SecureQueryHasSignature()
    {
        // See https://tools.ietf.org/html/rfc4035#appendix-B.1

        var resolver = new NameServer { Catalog = _example };
        var request = new Message().UseDnsSecurity();
        request.Questions.Add(new Question { Name = "x.w.example", Type = DnsType.MX });
        var response = await resolver.ResolveAsync(request);

        response.IsResponse.ShouldBeTrue();
        response.Status.ShouldBe(MessageStatus.NoError);
        response.AA.ShouldBeTrue();
        response.DO.ShouldBeTrue();

        response.Answers.Count.ShouldBe(2);
        response.Answers.OfType<MXRecord>().Count().ShouldBe(1);
        response.Answers.OfType<RRSIGRecord>().Count().ShouldBe(1);

        response.AuthorityRecords.Count.ShouldBe(3);
        response.AuthorityRecords.OfType<NSRecord>().Count().ShouldBe(2);
        response.AuthorityRecords.OfType<RRSIGRecord>().Count().ShouldBe(1);
    }
}
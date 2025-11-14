using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Makaretu.Dns;
using Makaretu.Dns.Resolving;
using Shouldly;
using Xunit;

namespace DnsTests.Resolving;

public class CachedNameServerTest
{
    [Fact]
    public async Task Pruning()
    {
        var now = DateTime.Now;
        var cache = new CachedNameServer { Catalog = new Catalog(), AnswerAllQuestions = true };
        cache.Catalog.Add(new ARecord { Name = "a.foo.org", Address = IPAddress.Loopback, TTL = TimeSpan.FromSeconds(30) });
        cache.Catalog.Add(new ARecord { Name = "b.foo.org", Address = IPAddress.Loopback, TTL = TimeSpan.FromSeconds(60) });
        var query = new Message();
        query.Questions.Add(new Question { Name = "a.foo.org", Type = DnsType.A });
        query.Questions.Add(new Question { Name = "b.foo.org", Type = DnsType.A });

        var response = await cache.ResolveAsync(query, TestContext.Current.CancellationToken);
        response.Answers.Any(static a => a.Name == "a.foo.org").ShouldBeTrue();
        response.Answers.Any(static a => a.Name == "b.foo.org").ShouldBeTrue();

        cache.Prune(now);
        response = await cache.ResolveAsync(query, TestContext.Current.CancellationToken);
        response.Answers.Any(static a => a.Name == "a.foo.org").ShouldBeTrue();
        response.Answers.Any(static a => a.Name == "b.foo.org").ShouldBeTrue();

        cache.Prune(now + TimeSpan.FromSeconds(31));
        response = await cache.ResolveAsync(query, TestContext.Current.CancellationToken);
        response.Answers.Any(static a => a.Name == "a.foo.org").ShouldBeFalse();
        response.Answers.Any(static a => a.Name == "b.foo.org").ShouldBeTrue();

        cache.Prune(now + TimeSpan.FromSeconds(61));
        response = await cache.ResolveAsync(query, TestContext.Current.CancellationToken);
        response.Answers.Any(static a => a.Name == "a.foo.org").ShouldBeFalse();
        response.Answers.Any(static a => a.Name == "b.foo.org").ShouldBeFalse();
    }

    [Fact]
    public async Task AddingResponse()
    {
        var cache = new CachedNameServer { Catalog = new Catalog(), AnswerAllQuestions = true };
        var response = new Message
        {
            QR = true,
            Answers = { new ARecord { Name = "foo.org", Address = IPAddress.Loopback } },
            AdditionalRecords = { new AAAARecord { Name = "foo.org", Address = IPAddress.Loopback } }
        };
        cache.Add(response);

        var query = new Message
        {
            Questions =
            {
                new Question { Name = "foo.org", Type = DnsType.A },
                new Question { Name = "foo.org", Type = DnsType.AAAA }
            }
        };
        
        var res = await cache.ResolveAsync(query, TestContext.Current.CancellationToken);

        res.Answers.Any(static a => a.Name == "foo.org" && a.Type == DnsType.A).ShouldBeTrue();
        res.Answers.Any(static a => a.Name == "foo.org" && a.Type == DnsType.AAAA).ShouldBeTrue();
    }

    [Fact]
    public async Task AddingResponse_TTL0()
    {
        var cache = new CachedNameServer { Catalog = new Catalog(), AnswerAllQuestions = true };
        var response = new Message
        {
            QR = true,
            Answers = { new ARecord { Name = "foo.org", Address = IPAddress.Loopback, TTL = TimeSpan.Zero } },
            AdditionalRecords = { new AAAARecord { Name = "foo.org", Address = IPAddress.Loopback } }
        };
        cache.Add(response);

        var query = new Message
        {
            Questions =
            {
                new Question { Name = "foo.org", Type = DnsType.A },
                new Question { Name = "foo.org", Type = DnsType.AAAA }
            }
        };
        
        var res = await cache.ResolveAsync(query, TestContext.Current.CancellationToken);

        res.Answers.Any(static a => a.Name == "foo.org" && a.Type == DnsType.A).ShouldBeFalse();
        res.Answers.Any(static a => a.Name == "foo.org" && a.Type == DnsType.AAAA).ShouldBeTrue();
    }

    [Fact]
    public async Task Pruning_Background()
    {
        var cache = new CachedNameServer { Catalog = new Catalog() };
        cache.Catalog.Add(new ARecord
        {
            TTL = TimeSpan.FromMilliseconds(500),
            Name = "a.foo.org",
            Address = IPAddress.Loopback
        });

        var query = new Message
        {
            Questions =
            {
                new Question { Name = "a.foo.org", Type = DnsType.A }
            }
        };
        
        var res = await cache.ResolveAsync(query, TestContext.Current.CancellationToken);
        res.Answers.Count.ShouldBe(1);

        var cts = cache.PruneContinuously(TimeSpan.FromMilliseconds(200));
        await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
        await cts.CancelAsync();
        await Task.Delay(TimeSpan.FromMilliseconds(40), CancellationToken.None);
        res = await cache.ResolveAsync(query, CancellationToken.None);
        res.Answers.Count.ShouldBe(0);
    }
}
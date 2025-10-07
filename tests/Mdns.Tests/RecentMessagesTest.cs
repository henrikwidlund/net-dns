using System;
using System.Threading.Tasks;
using Makaretu.Dns;
using Moq;
using Shouldly;
using Xunit;

namespace Makaretu.Mdns;

public class RecentMessagesTest
{
    [Fact]
    public void Pruning()
    {
        var now = DateTimeOffset.UtcNow;
        var timeProviderMock = new Mock<TimeProvider>();
        timeProviderMock.Setup(static tp => tp.GetUtcNow()).Returns(now.AddSeconds(-2));
        timeProviderMock.Setup(static tp => tp.LocalTimeZone).Returns(TimeZoneInfo.Local);

        var messages = new RecentMessages(timeProviderMock.Object);
        messages.TryAdd("a"u8.ToArray());
        messages.TryAdd("b"u8.ToArray());
        timeProviderMock.Setup(static tp => tp.GetUtcNow()).Returns(now);
        byte[] cMessage = "c"u8.ToArray();
        messages.TryAdd(cMessage);

        messages.Count.ShouldBe(1);
        messages.HasMessage(cMessage).ShouldBeTrue();
    }

    [Fact]
    public void MessageId()
    {
        var a0 = RecentMessages.GetId([1]);
        var a1 = RecentMessages.GetId([1]);
        var b = RecentMessages.GetId([2]);

        a0.ShouldBe(a1);
        b.ShouldNotBe(a0);
    }

    [Fact]
    public async Task DuplicateCheck()
    {
        var r = new RecentMessages { Interval = TimeSpan.FromMilliseconds(100) };
        var a = new byte[] { 1 };
        var b = new byte[] { 2 };

        r.TryAdd(a).ShouldBeTrue();
        r.TryAdd(b).ShouldBeTrue();
        r.TryAdd(a).ShouldBeFalse();

        await Task.Delay(200);
        r.TryAdd(a).ShouldBeTrue();
    }
}
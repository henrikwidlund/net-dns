using System;
using System.Threading.Tasks;
using Makaretu.Dns;
using Moq;

namespace Makaretu.Mdns;

public class RecentMessagesTest
{
    [Test]
    public async Task Pruning()
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

        await Assert.That(messages.Count).IsEqualTo(1);
        await Assert.That(messages.HasMessage(cMessage)).IsTrue();
    }

    [Test]
    public async Task MessageId()
    {
        var a0 = RecentMessages.GetId([1]);
        var a1 = RecentMessages.GetId([1]);
        var b = RecentMessages.GetId([2]);

        await Assert.That(a0).IsEqualTo(a1);
        await Assert.That(b).IsNotEqualTo(a0);
    }

    [Test]
    public async Task DuplicateCheck()
    {
        var r = new RecentMessages { Interval = TimeSpan.FromMilliseconds(100) };
        var a = new byte[] { 1 };
        var b = new byte[] { 2 };

        await Assert.That(r.TryAdd(a)).IsTrue();
        await Assert.That(r.TryAdd(b)).IsTrue();
        await Assert.That(r.TryAdd(a)).IsFalse();

        await Task.Delay(200, TestContext.Current!.Execution.CancellationToken);
        await Assert.That(r.TryAdd(a)).IsTrue();
    }
}
using System;
using System.Threading.Tasks;

using Makaretu.Dns;

namespace DnsTests;

public class EdnsKeepaliveOptionTest
{
    [Test]
    public async Task Roundtrip()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsKeepaliveOption
        {
            Timeout = TimeSpan.FromSeconds(3)
        };

        await Assert.That(expected.Type).IsEqualTo(EdnsOptionType.Keepalive);

        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsKeepaliveOption)opt2.Options[0];

        await Assert.That(actual.Type).IsEqualTo(expected.Type);
        await Assert.That(actual.Timeout.HasValue).IsEqualTo(expected.Timeout.HasValue);
        await Assert.That(actual.Timeout!.Value).IsEqualTo(expected.Timeout!.Value);
    }

    [Test]
    public async Task Roundtrip_Empty()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsKeepaliveOption();

        await Assert.That(expected.Type).IsEqualTo(EdnsOptionType.Keepalive);
        await Assert.That(expected.Timeout.HasValue).IsFalse();

        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsKeepaliveOption)opt2.Options[0];

        await Assert.That(actual.Type).IsEqualTo(expected.Type);
        await Assert.That(actual.Timeout.HasValue).IsEqualTo(expected.Timeout.HasValue);
    }
}
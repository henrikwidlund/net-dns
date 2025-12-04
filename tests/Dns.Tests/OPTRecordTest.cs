using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class OPTRecordTest
{
    [Test]
    public async Task Defaults()
    {
        var opt = new OPTRecord();

        await Assert.That(opt.Name).IsEquatableOrEqualTo("");
        await Assert.That(opt.RequestorPayloadSize).IsEqualTo((ushort)1280);
        await Assert.That((ushort)opt.Class).IsEqualTo(opt.RequestorPayloadSize);
        await Assert.That(opt.Opcode8).IsEqualTo((byte)0);
        await Assert.That(opt.Version).IsEqualTo((byte)0);
        await Assert.That(opt.DO).IsFalse();
        await Assert.That(opt.Options).Count().IsEqualTo(0);
    }

    [Test]
    public async Task Roundtrip()
    {
        var a = new OPTRecord
        {
            RequestorPayloadSize = 512,
            Opcode8 = 2,
            Version = 3,
            DO = true
        };

        var b = (OPTRecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
        await Assert.That(a.RequestorPayloadSize).IsEqualTo(b.RequestorPayloadSize);
        await Assert.That(a.Opcode8).IsEqualTo(b.Opcode8);
        await Assert.That(a.Version).IsEqualTo(b.Version);
        await Assert.That(a.DO).IsEqualTo(b.DO);
    }

    [Test]
    public async Task Roundtrip_NoOptions()
    {
        var a = new OPTRecord();
        var b = (OPTRecord)new ResourceRecord().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
        await Assert.That(a.TTL).IsEqualTo(b.TTL);
    }

    [Test]
    public async Task Equality()
    {
        var a = new OPTRecord();

        var b = new OPTRecord
        {
            RequestorPayloadSize = 512
        };

        // ReSharper disable once EqualExpressionComparison
        await Assert.That(a.Equals(a)).IsTrue();
        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }
}
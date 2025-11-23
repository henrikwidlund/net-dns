using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class OPTRecordTest
{
    [Test]
    public void Defaults()
    {
        var opt = new OPTRecord();

        opt.Name.ShouldBe("");
        opt.RequestorPayloadSize.ShouldBe((ushort)1280);
        ((ushort)opt.Class).ShouldBe(opt.RequestorPayloadSize);
        opt.Opcode8.ShouldBe((byte)0);
        opt.Version.ShouldBe((byte)0);
        opt.DO.ShouldBeFalse();
        opt.Options.Count.ShouldBe(0);
    }

    [Test]
    public void Roundtrip()
    {
        var a = new OPTRecord
        {
            RequestorPayloadSize = 512,
            Opcode8 = 2,
            Version = 3,
            DO = true
        };

        var b = (OPTRecord)new ResourceRecord().Read(a.ToByteArray());

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.RequestorPayloadSize.ShouldBe(b.RequestorPayloadSize);
        a.Opcode8.ShouldBe(b.Opcode8);
        a.Version.ShouldBe(b.Version);
        a.DO.ShouldBe(b.DO);
    }

    [Test]
    public void Roundtrip_NoOptions()
    {
        var a = new OPTRecord();
        var b = (OPTRecord)new ResourceRecord().Read(a.ToByteArray());

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
    }

    [Test]
    public void Equality()
    {
        var a = new OPTRecord();

        var b = new OPTRecord
        {
            RequestorPayloadSize = 512
        };

        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }
}
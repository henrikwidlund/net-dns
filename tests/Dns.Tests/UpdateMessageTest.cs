using System.Linq;
using System.Net;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class UpdateMessageTest
{
    [Fact]
    public void Defaults()
    {
        var m = new UpdateMessage();
        
        m.AdditionalResources.Count.ShouldBe(0);
        m.Id.ShouldBe((ushort)0);
        m.IsResponse.ShouldBeFalse();
        m.IsUpdate.ShouldBeTrue();
        m.Opcode.ShouldBe(MessageOperation.Update);
        m.Prerequisites.Count.ShouldBe(0);
        m.QR.ShouldBeFalse();
        m.Status.ShouldBe(MessageStatus.NoError);
        m.Updates.Count.ShouldBe(0);
        m.Z.ShouldBe(0);
        m.Zone.ShouldNotBeNull();
        m.Zone.Type.ShouldBe(DnsType.SOA);
        m.Zone.Class.ShouldBe(DnsClass.IN);
    }

    [Fact]
    public void Flags()
    {
        var expected = new UpdateMessage
        {
            Id = 1234,
            Zone = new Question { Name = "erehwon.org", Type = DnsType.A },
            QR = true,
            Z = 0x7F,
            Status = MessageStatus.NotImplemented
        };
        
        var actual = new UpdateMessage();
        actual.Read(expected.ToByteArray());
        
        actual.Id.ShouldBe(expected.Id);
        actual.QR.ShouldBe(expected.QR);
        actual.Opcode.ShouldBe(expected.Opcode);
        actual.Z.ShouldBe(expected.Z);
        actual.Status.ShouldBe(expected.Status);
        actual.Zone.Name.ShouldBe(expected.Zone.Name);
        actual.Zone.Class.ShouldBe(expected.Zone.Class);
        actual.Zone.Type.ShouldBe(expected.Zone.Type);
    }

    [Fact]
    public void Response()
    {
        var update = new UpdateMessage { Id = 1234 };
        var response = update.CreateResponse();
        
        response.IsResponse.ShouldBeTrue();
        response.Id.ShouldBe(update.Id);
        response.Opcode.ShouldBe(update.Opcode);
    }

    [Fact]
    public void Roundtrip()
    {
        var expected = new UpdateMessage
        {
            Id = 1234,
            Zone =
            {
                Name = "emanon.org"
            }
        };
        
        expected.Prerequisites
            .MustExist("foo.emanon.org")
            .MustNotExist("bar.emanon.org");
        
        expected.Updates
            .AddResource(new ARecord { Name = "bar.emanon.org", Address = IPAddress.Parse("127.0.0.1") })
            .DeleteResource("foo.emanon.org");
        
        var actual = (UpdateMessage)new UpdateMessage().Read(expected.ToByteArray());
        
        actual.Id.ShouldBe(expected.Id);
        actual.IsUpdate.ShouldBe(expected.IsUpdate);
        actual.IsResponse.ShouldBe(expected.IsResponse);
        actual.Opcode.ShouldBe(expected.Opcode);
        actual.QR.ShouldBe(expected.QR);
        actual.Status.ShouldBe(expected.Status);
        actual.Zone.Name.ShouldBe(expected.Zone.Name);
        actual.Zone.Class.ShouldBe(expected.Zone.Class);
        actual.Zone.Type.ShouldBe(expected.Zone.Type);
        actual.Prerequisites.SequenceEqual(expected.Prerequisites).ShouldBeTrue();
        actual.Updates.SequenceEqual(expected.Updates).ShouldBeTrue();
        actual.AdditionalResources.SequenceEqual(expected.AdditionalResources).ShouldBeTrue();
    }
}
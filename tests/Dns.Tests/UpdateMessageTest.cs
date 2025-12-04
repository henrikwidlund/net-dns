using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class UpdateMessageTest
{
    [Test]
    public async Task Defaults()
    {
        var m = new UpdateMessage();
        
        await Assert.That(m.AdditionalResources).Count().IsEqualTo(0);
        await Assert.That(m.Id).IsEqualTo((ushort)0);
        await Assert.That(m.IsResponse).IsFalse();
        await Assert.That(m.IsUpdate).IsTrue();
        await Assert.That(m.Opcode).IsEqualTo(MessageOperation.Update);
        await Assert.That(m.Prerequisites).Count().IsEqualTo(0);
        await Assert.That(m.QR).IsFalse();
        await Assert.That(m.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(m.Updates).Count().IsEqualTo(0);
        await Assert.That(m.Z).IsEqualTo(0);
        await Assert.That(m.Zone).IsNotNull();
        await Assert.That(m.Zone.Type).IsEqualTo(DnsType.SOA);
        await Assert.That(m.Zone.Class).IsEqualTo(DnsClass.IN);
    }

    [Test]
    public async Task Flags()
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
        
        await Assert.That(actual.Id).IsEqualTo(expected.Id);
        await Assert.That(actual.QR).IsEqualTo(expected.QR);
        await Assert.That(actual.Opcode).IsEqualTo(expected.Opcode);
        await Assert.That(actual.Z).IsEqualTo(expected.Z);
        await Assert.That(actual.Status).IsEqualTo(expected.Status);
        await Assert.That(actual.Zone.Name).IsEqualTo(expected.Zone.Name);
        await Assert.That(actual.Zone.Class).IsEqualTo(expected.Zone.Class);
        await Assert.That(actual.Zone.Type).IsEqualTo(expected.Zone.Type);
    }

    [Test]
    public async Task Response()
    {
        var update = new UpdateMessage { Id = 1234 };
        var response = update.CreateResponse();
        
        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Id).IsEqualTo(update.Id);
        await Assert.That(response.Opcode).IsEqualTo(update.Opcode);
    }

    [Test]
    public async Task Roundtrip()
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
        
        await Assert.That(actual.Id).IsEqualTo(expected.Id);
        await Assert.That(actual.IsUpdate).IsEqualTo(expected.IsUpdate);
        await Assert.That(actual.IsResponse).IsEqualTo(expected.IsResponse);
        await Assert.That(actual.Opcode).IsEqualTo(expected.Opcode);
        await Assert.That(actual.QR).IsEqualTo(expected.QR);
        await Assert.That(actual.Status).IsEqualTo(expected.Status);
        await Assert.That(actual.Zone.Name).IsEqualTo(expected.Zone.Name);
        await Assert.That(actual.Zone.Class).IsEqualTo(expected.Zone.Class);
        await Assert.That(actual.Zone.Type).IsEqualTo(expected.Zone.Type);
        await Assert.That(actual.Prerequisites.SequenceEqual(expected.Prerequisites)).IsTrue();
        await Assert.That(actual.Updates.SequenceEqual(expected.Updates)).IsTrue();
        await Assert.That(actual.AdditionalResources.SequenceEqual(expected.AdditionalResources)).IsTrue();
    }
}
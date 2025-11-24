using System.Threading.Tasks;

using Makaretu.Dns;

namespace DnsTests;

public class DnsObjectTest
{
    [Test]
    public async Task Length_EmptyMessage()
    {
        var message = new Message();
        await Assert.That(message.Length()).IsEqualTo(Message.MinLength);
    }

    [Test]
    public async Task Clone()
    {
        var m1 = new Message
        {
            Questions = { new Question { Name = "example.com" } }
        };
        
        var m2 = (Message)m1.Clone();

        await Assert.That(m1.ToByteArray()).IsEquivalentTo(m2.ToByteArray());
    }

    [Test]
    public async Task Clone_Typed()
    {
        var m1 = new Message
        {
            Questions = { new Question { Name = "example.com" } }
        };

        var m2 = m1.Clone<Message>();

        await Assert.That(m1.ToByteArray()).IsEquivalentTo(m2.ToByteArray());
    }
}
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class DnsObjectTest
{
    [Test]
    public void Length_EmptyMessage()
    {
        var message = new Message();
        message.Length().ShouldBe(Message.MinLength);
    }

    [Test]
    public void Clone()
    {
        var m1 = new Message
        {
            Questions = { new Question { Name = "example.com" } }
        };
        
        var m2 = (Message)m1.Clone();

        m1.ToByteArray().ShouldBe(m2.ToByteArray());
    }

    [Test]
    public void Clone_Typed()
    {
        var m1 = new Message
        {
            Questions = { new Question { Name = "example.com" } }
        };

        var m2 = m1.Clone<Message>();

        m1.ToByteArray().ShouldBe(m2.ToByteArray());
    }
}
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class QuestionTest
{
    [Test]
    public async Task Roundtrip()
    {
        var a = new Question
        {
            Name = "emanon.org",
            Class = DnsClass.CH,
            Type = DnsType.MX
        };

        var b = (Question)new Question().Read(a.ToByteArray());

        await Assert.That(a.Name).IsEqualTo(b.Name);
        await Assert.That(a.Class).IsEqualTo(b.Class);
        await Assert.That(a.Type).IsEqualTo(b.Type);
    }
}
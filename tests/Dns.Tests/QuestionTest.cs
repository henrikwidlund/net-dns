using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class QuestionTest
{
    [Fact]
    public void Roundtrip()
    {
        var a = new Question
        {
            Name = "emanon.org",
            Class = DnsClass.CH,
            Type = DnsType.MX
        };

        var b = (Question)new Question().Read(a.ToByteArray());

        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
    }
}
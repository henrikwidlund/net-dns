using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class EdnsPaddingOptionTest
{
    [Test]
    public void Roundtrip()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsPaddingOption
        {
            Padding = "\0\0\0"u8.ToArray()
        };

        EdnsOptionType.Padding.ShouldBe(expected.Type);
        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsPaddingOption)opt2.Options[0];

        expected.Type.ShouldBe(actual.Type);
        expected.Padding.ShouldBe(actual.Padding);
    }
}
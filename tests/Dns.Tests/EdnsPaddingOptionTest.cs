using System.Threading.Tasks;

using Makaretu.Dns;

namespace DnsTests;

public class EdnsPaddingOptionTest
{
    [Test]
    public async Task Roundtrip()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsPaddingOption
        {
            Padding = "\0\0\0"u8.ToArray()
        };

        await Assert.That(expected.Type).IsEqualTo(EdnsOptionType.Padding);
        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsPaddingOption)opt2.Options[0];

        await Assert.That(actual.Type).IsEqualTo(expected.Type);
        await Assert.That(actual.Padding).IsEquivalentTo(expected.Padding);
    }
}
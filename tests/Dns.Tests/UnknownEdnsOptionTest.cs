using System.Threading.Tasks;

using Makaretu.Dns;

namespace DnsTests;

public class UnknownEdnsOptionTest
{
    [Test]
    public async Task Roundtrip()
    {
        var opt1 = new OPTRecord();
        var expected = new UnknownEdnsOption
        {
            Type = EdnsOptionType.ExperimentalMin,
            Data = [10, 11, 12]
        };
        opt1.Options.Add(expected);

        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (UnknownEdnsOption)opt2.Options[0];

        await Assert.That(expected.Type).IsEqualTo(actual.Type);
        await Assert.That(expected.Data).IsEquivalentTo(actual.Data!);
    }
}
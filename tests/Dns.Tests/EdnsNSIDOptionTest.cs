using System.Threading.Tasks;

using Makaretu.Dns;

namespace DnsTests;

public class EdnsNSIDOptionTest
{
    [Test]
    public async Task Roundtrip()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsNSIDOption
        {
            Id = [1, 2, 3, 4]
        };

        await Assert.That(expected.Type).IsEqualTo(EdnsOptionType.NSID);

        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsNSIDOption)opt2.Options[0];

        await Assert.That(actual.Type).IsEqualTo(expected.Type);
        await Assert.That(actual.Id).IsEquivalentTo(expected.Id);
    }
}
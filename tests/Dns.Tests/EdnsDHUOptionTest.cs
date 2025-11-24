using System.Linq;
using System.Threading.Tasks;

using Makaretu.Dns;

namespace DnsTests;

public class EdnsDHUOptionTest
{
    [Test]
    public async Task Roundtrip()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsDHUOption
        {
            Algorithms = { DigestType.GostR34_11_94, DigestType.Sha512 }
        };

        await Assert.That(expected.Type).IsEqualTo(EdnsOptionType.DHU);

        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsDHUOption)opt2.Options[0];

        await Assert.That(actual.Type).IsEqualTo(expected.Type);
        await Assert.That(actual.Algorithms).IsEquivalentTo(expected.Algorithms);
    }

    [Test]
    public async Task Create()
    {
        var option = EdnsDHUOption.Create();

        await Assert.That(option.Type).IsEqualTo(EdnsOptionType.DHU);
        await Assert.That(option.Algorithms).IsEquivalentTo(DigestRegistry.Digests.ToArray());
    }
}
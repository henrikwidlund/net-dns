using System.Linq;
using System.Threading.Tasks;

using Makaretu.Dns;

namespace DnsTests;

public class EdnsN3UOptionTest
{
    [Test]
    public async Task Roundtrip()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsN3UOption
        {
            Algorithms = { DigestType.GostR34_11_94, DigestType.Sha512 }
        };

        await Assert.That(expected.Type).IsEqualTo(EdnsOptionType.N3U);

        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsN3UOption)opt2.Options[0];

        await Assert.That(actual.Type).IsEqualTo(expected.Type);
        await Assert.That(actual.Algorithms).IsEquivalentTo(expected.Algorithms);
    }

    [Test]
    public async Task Create()
    {
        var option = EdnsN3UOption.Create();

        await Assert.That(option.Type).IsEqualTo(EdnsOptionType.N3U);
        await Assert.That(option.Algorithms).IsEquivalentTo(DigestRegistry.Digests.ToArray());
    }
}
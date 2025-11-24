using System.Linq;
using System.Threading.Tasks;

using Makaretu.Dns;

namespace DnsTests;

public class EdnsDAUOptionTest
{
    [Test]
    public async Task Roundtrip()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsDAUOption
        {
            Algorithms = { SecurityAlgorithm.ED25519, SecurityAlgorithm.ECCGOST }
        };

        await Assert.That(expected.Type).IsEqualTo(EdnsOptionType.DAU);

        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsDAUOption)opt2.Options[0];

        await Assert.That(actual.Type).IsEqualTo(expected.Type);
        await Assert.That(actual.Algorithms).IsEquivalentTo(expected.Algorithms);
    }

    [Test]
    public async Task Create()
    {
        var option = EdnsDAUOption.Create();

        await Assert.That(option.Type).IsEqualTo(EdnsOptionType.DAU);
        await Assert.That(option.Algorithms).IsEquivalentTo(SecurityAlgorithmRegistry.Algorithms.Keys.ToList());
    }
}
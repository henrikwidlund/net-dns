using System.Linq;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class EdnsDAUOptionTest
{
    [Fact]
    public void Roundtrip()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsDAUOption
        {
            Algorithms = { SecurityAlgorithm.ED25519, SecurityAlgorithm.ECCGOST }
        };
        
        expected.Type.ShouldBe(EdnsOptionType.DAU);

        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsDAUOption)opt2.Options[0];
        
        actual.Type.ShouldBe(expected.Type);
        actual.Algorithms.ShouldBe(expected.Algorithms);
    }

    [Fact]
    public void Create()
    {
        var option = EdnsDAUOption.Create();
        
        option.Type.ShouldBe(EdnsOptionType.DAU);
        option.Algorithms.ShouldBe(SecurityAlgorithmRegistry.Algorithms.Keys.ToList());
    }
}
using System.Linq;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class EdnsDHUOptionTest
{
    [Fact]
    public void Roundtrip()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsDHUOption
        {
            Algorithms = { DigestType.GostR34_11_94, DigestType.Sha512 }
        };
        
        expected.Type.ShouldBe(EdnsOptionType.DHU);

        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsDHUOption)opt2.Options[0];
        
        actual.Type.ShouldBe(expected.Type);
        actual.Algorithms.ShouldBe(expected.Algorithms);
    }

    [Fact]
    public void Create()
    {
        var option = EdnsDHUOption.Create();
        
        option.Type.ShouldBe(EdnsOptionType.DHU);
        option.Algorithms.ShouldBe(DigestRegistry.Digests.ToArray());
    }
}
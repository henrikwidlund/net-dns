using System.Linq;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class EdnsN3UOptionTest
{
    [Fact]
    public void Roundtrip()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsN3UOption
        {
            Algorithms = { DigestType.GostR34_11_94, DigestType.Sha512 }
        };
        
        expected.Type.ShouldBe(EdnsOptionType.N3U);

        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsN3UOption)opt2.Options[0];
        
        actual.Type.ShouldBe(expected.Type);
        actual.Algorithms.ShouldBe(expected.Algorithms);
    }

    [Fact]
    public void Create()
    {
        var option = EdnsN3UOption.Create();
        
        option.Type.ShouldBe(EdnsOptionType.N3U);
        option.Algorithms.ShouldBe(DigestRegistry.Digests.ToArray());
    }
}
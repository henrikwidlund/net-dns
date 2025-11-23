using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class EdnsNSIDOptionTest
{
    [Test]
    public void Roundtrip()
    {
        var opt1 = new OPTRecord();
        var expected = new EdnsNSIDOption
        {
            Id = [1, 2, 3, 4]
        };

        expected.Type.ShouldBe(EdnsOptionType.NSID);

        opt1.Options.Add(expected);
        var opt2 = (OPTRecord)new ResourceRecord().Read(opt1.ToByteArray());
        var actual = (EdnsNSIDOption)opt2.Options[0];

        actual.Type.ShouldBe(expected.Type);
        actual.Id.ShouldBe(expected.Id);
    }
}
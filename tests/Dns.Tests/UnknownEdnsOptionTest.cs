using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class UnknownEdnsOptionTest
{
    [Test]
    public void Roundtrip()
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

        expected.Type.ShouldBe(actual.Type);
        expected.Data.ShouldBe(actual.Data);
    }
}
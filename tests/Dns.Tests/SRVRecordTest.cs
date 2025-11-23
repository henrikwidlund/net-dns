using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class SRVRecordTest
{
    [Test]
    public void Roundtrip()
    {
        var a = new SRVRecord
        {
            Name = "_foobar._tcp",
            Priority = 1,
            Weight = 2,
            Port = 9,
            Target = "foobar.example.com"
        };
        
        var b = (SRVRecord)new ResourceRecord().Read(a.ToByteArray());
        
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Priority.ShouldBe(b.Priority);
        a.Weight.ShouldBe(b.Weight);
        a.Port.ShouldBe(b.Port);
        a.Target.ShouldBe(b.Target);
    }

    [Test]
    public void Roundtrip_Master()
    {
        var a = new SRVRecord
        {
            Name = "_foobar._tcp",
            Priority = 1,
            Weight = 2,
            Port = 9,
            Target = "foobar.example.com"
        };
        
        var b = (SRVRecord)new ResourceRecord().Read(a.ToString());
        
        b.ShouldNotBeNull();
        a.Name.ShouldBe(b.Name);
        a.Class.ShouldBe(b.Class);
        a.Type.ShouldBe(b.Type);
        a.TTL.ShouldBe(b.TTL);
        a.Priority.ShouldBe(b.Priority);
        a.Weight.ShouldBe(b.Weight);
        a.Port.ShouldBe(b.Port);
        a.Target.ShouldBe(b.Target);
    }

    [Test]
    public void Equality()
    {
        var a = new SRVRecord
        {
            Name = "_foobar._tcp",
            Priority = 1,
            Weight = 2,
            Port = 9,
            Target = "foobar.example.com"
        };
        
        var b = new SRVRecord
        {
            Name = "_foobar._tcp",
            Priority = 1,
            Weight = 2,
            Port = 9,
            Target = "foobar-x.example.com"
        };

        // ReSharper disable once EqualExpressionComparison
        a.Equals(a).ShouldBeTrue();
        a.Equals(b).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }
}
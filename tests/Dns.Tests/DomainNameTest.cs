using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class DomainNameTest
{
    [Fact]
    public void Standard()
    {
        var name = new DomainName("my.example.org");

        name.Labels.Count.ShouldBe(3);
        name.Labels[0].ShouldBe("my");
        name.Labels[1].ShouldBe("example");
        name.Labels[2].ShouldBe("org");

        name.ToString().ShouldBe("my.example.org");
    }

    [Fact]
    public void TopLevelDomain()
    {
        var name = new DomainName("org");

        name.Labels.Count.ShouldBe(1);
        name.Labels[0].ShouldBe("org");

        name.ToString().ShouldBe("org");
    }

    [Fact]
    public void Root()
    {
        var name = new DomainName("");

        name.Labels.Count.ShouldBe(0);
        name.ToString().ShouldBe("");
    }

    [Fact]
    public void EscapedDotCharacter()
    {
        var name = new DomainName(@"my\.example.org");

        name.Labels.Count.ShouldBe(2);
        name.Labels[0].ShouldBe("my.example");
        name.Labels[1].ShouldBe("org");
        name.ToString().ShouldBe(@"my\.example.org");
    }

    [Fact]
    public void EscapedDotDigits()
    {
        var name = new DomainName(@"my\046example.org");

        name.Labels.Count.ShouldBe(2);
        name.Labels[0].ShouldBe("my.example");
        name.Labels[1].ShouldBe("org");
        name.ToString().ShouldBe(@"my\.example.org");
    }

    [Fact]
    public void ImplicitParsingOfString()
    {
        DomainName name = @"my\046example.org";
        name.Labels.Count.ShouldBe(2);
        name.Labels[0].ShouldBe("my.example");
        name.Labels[1].ShouldBe("org");

        name = @"my\.example.org";
        name.Labels.Count.ShouldBe(2);
        name.Labels[0].ShouldBe("my.example");
        name.Labels[1].ShouldBe("org");

        name = "my.example.org";
        name.Labels.Count.ShouldBe(3);
        name.Labels[0].ShouldBe("my");
        name.Labels[1].ShouldBe("example");
        name.Labels[2].ShouldBe("org");
    }

    [Fact]
    public void FromLabels()
    {
        var name = new DomainName("my.example", "org");

        name.Labels.Count.ShouldBe(2);
        name.Labels[0].ShouldBe("my.example");
        name.Labels[1].ShouldBe("org");
        name.ToString().ShouldBe(@"my\.example.org");
    }

    [Fact]
    public void Equality()
    {
        var a = new DomainName(@"my\.example.org");
        var b = new DomainName("my.example", "org");
        var c = new DomainName(@"my\046example.org");
        var d = new DomainName(@"My\.EXAMPLe.ORg");
        var other1 = new DomainName("example.org");
        var other2 = new DomainName("org");

        a.ShouldBe(b);
        a.ShouldBe(c);
        a.ShouldBe(d);
        a.ShouldNotBe(other1);
        a.ShouldNotBe(other2);
        a.ShouldNotBe(null);

        (a == b).ShouldBeTrue();
        (a == c).ShouldBeTrue();
        (a == d).ShouldBeTrue();
        (a == other1).ShouldBeFalse();
        (a == other2).ShouldBeFalse();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        (a == null).ShouldBeFalse();

        (a != b).ShouldBeFalse();
        (a != c).ShouldBeFalse();
        (a != d).ShouldBeFalse();
        (a != other1).ShouldBeTrue();
        (a != other2).ShouldBeTrue();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        (a != null).ShouldBeTrue();

        a.Equals(b).ShouldBeTrue();
        a.Equals(c).ShouldBeTrue();
        a.Equals(d).ShouldBeTrue();
        a.Equals(other1).ShouldBeFalse();
        a.Equals(other2).ShouldBeFalse();
        a.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void HashEquality()
    {
        var a = new DomainName(@"my\.example.org");
        var b = new DomainName("my.example", "org");
        var c = new DomainName(@"my\046example.org");
        var d = new DomainName(@"My\.EXAMPLe.ORg");
        var other1 = new DomainName("example.org");
        var other2 = new DomainName("org");

        a.GetHashCode().ShouldBe(b.GetHashCode());
        a.GetHashCode().ShouldBe(c.GetHashCode());
        a.GetHashCode().ShouldBe(d.GetHashCode());
        a.GetHashCode().ShouldNotBe(other1.GetHashCode());
        a.GetHashCode().ShouldNotBe(other2.GetHashCode());
    }

    [Fact]
    public void ToCanonical()
    {
        var a = new DomainName("My.EXAMPLe.ORg");

        a.ToString().ShouldBe("My.EXAMPLe.ORg");
        a.ToCanonical().ToString().ShouldBe("my.example.org");
    }

    [Fact]
    public void IsSubdomainOf()
    {
        var zone = new DomainName("example.org");

        zone.IsSubdomainOf(zone).ShouldBeFalse();
        new DomainName("a.example.org").IsSubdomainOf(zone).ShouldBeTrue();
        new DomainName("a.b.example.org").IsSubdomainOf(zone).ShouldBeTrue();
        new DomainName("a.Example.org").IsSubdomainOf(zone).ShouldBeTrue();
        new DomainName("a.b.Example.ORG").IsSubdomainOf(zone).ShouldBeTrue();
        new DomainName(@"a\.example.org").IsSubdomainOf(zone).ShouldBeFalse();
        new DomainName(@"a\.b.example.org").IsSubdomainOf(zone).ShouldBeTrue();
        new DomainName(@"a\.b.example.ORG").IsSubdomainOf(zone).ShouldBeTrue();
        new DomainName("a.org").IsSubdomainOf(zone).ShouldBeFalse();
        new DomainName("a.b.org").IsSubdomainOf(zone).ShouldBeFalse();
    }

    [Fact]
    public void BelongsTo()
    {
        var zone = new DomainName("example.org");

        zone.BelongsTo(zone).ShouldBeTrue();
        new DomainName("ExamPLE.Org").BelongsTo(zone).ShouldBeTrue();
        new DomainName("A.ExamPLE.Org").BelongsTo(zone).ShouldBeTrue();
        new DomainName("a.example.org").BelongsTo(zone).ShouldBeTrue();
        new DomainName("a.b.example.org").BelongsTo(zone).ShouldBeTrue();
        new DomainName("a.Example.org").BelongsTo(zone).ShouldBeTrue();
        new DomainName("a.b.Example.ORG").BelongsTo(zone).ShouldBeTrue();
        new DomainName(@"a\.example.org").BelongsTo(zone).ShouldBeFalse();
        new DomainName(@"a\.b.example.org").BelongsTo(zone).ShouldBeTrue();
        new DomainName(@"a\.b.example.ORG").BelongsTo(zone).ShouldBeTrue();
        new DomainName("a.org").BelongsTo(zone).ShouldBeFalse();
        new DomainName("a.b.org").BelongsTo(zone).ShouldBeFalse();
    }

    [Fact]
    public void Parent()
    {
        var name = new DomainName(@"a.b\.c.example.org");
        var expected = new DomainName(@"b\.c.example.org");
        expected.ShouldBe(name.Parent());

        name = new DomainName("org");
        expected = new DomainName("");
        expected.ShouldBe(name.Parent());
        expected.Parent().ShouldBeNull();
    }

    [Fact]
    public void Joining()
    {
        var a = new DomainName(@"foo\.bar");
        var b = new DomainName("x.y.z");
        var c = DomainName.Join(a, b);

        c.Labels.Count.ShouldBe(4);
        c.Labels[0].ShouldBe("foo.bar");
        c.Labels[1].ShouldBe("x");
        c.Labels[2].ShouldBe("y");
        c.Labels[3].ShouldBe("z");
    }

    [Fact]
    public void Rfc4343_Section_2()
    {
        new DomainName("foo.example.net.").ShouldBe(new DomainName("Foo.ExamplE.net."));
        new DomainName("69.2.0.192.in-addr.arpa.").ShouldBe(new DomainName("69.2.0.192.in-ADDR.ARPA."));
    }

    [Fact]
    public void Rfc4343_Section_21_Backslash()
    {
        var aslashb = new DomainName(@"a\\b");

        aslashb.Labels.Count.ShouldBe(1);
        aslashb.Labels[0].ShouldBe(@"a\b");
        aslashb.ToString().ShouldBe(@"a\092b");
        aslashb.ShouldBe(new DomainName(@"a\092b"));
    }

    [Fact]
    public void Rfc4343_Section_21_4Digits()
    {
        var a = new DomainName(@"a\\4");
        var b = new DomainName(@"a\0924");

        a.ShouldBe(b);
    }

    [Fact]
    public void Rfc4343_Section_22_SpacesAndDots()
    {
        var a = new DomainName(@"Donald\032E\.\032Eastlake\0323rd.example");

        a.Labels.Count.ShouldBe(2);
        a.Labels[0].ShouldBe("Donald E. Eastlake 3rd");
        a.Labels[1].ShouldBe("example");
    }

    [Fact]
    public void Rfc4343_Section_22_Binary()
    {
        var a = new DomainName(@"a\000\\\255z.example");

        a.Labels.Count.ShouldBe(2);
        a.Labels[0][0].ShouldBe('a');
        a.Labels[0][1].ShouldBe((char)0);
        a.Labels[0][2].ShouldBe('\\');
        a.Labels[0][3].ShouldBe((char)0xff);
        a.Labels[0][4].ShouldBe('z');
        a.Labels[1].ShouldBe("example");
        a.ToString().ShouldBe(@"a\000\092\255z.example");
        a.ShouldBe(new DomainName(a.ToString()));
    }

    [Fact]
    public void FormattedString()
    {
        var name = new DomainName(@"foo ~ \.bar-12A.org");

        name.ToString().ShouldBe(@"foo\032~\032\.bar-12A.org");
    }
}
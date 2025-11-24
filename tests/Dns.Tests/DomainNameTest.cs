using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class DomainNameTest
{
    [Test]
    public async Task Standard()
    {
        var name = new DomainName("my.example.org");

        await Assert.That(name.Labels).HasCount(3);
        await Assert.That(name.Labels[0]).IsEqualTo("my");
        await Assert.That(name.Labels[1]).IsEqualTo("example");
        await Assert.That(name.Labels[2]).IsEqualTo("org");

        await Assert.That(name).IsEquatableOrEqualTo("my.example.org");
    }

    [Test]
    public async Task TopLevelDomain()
    {
        var name = new DomainName("org");

        await Assert.That(name.Labels).HasCount(1);
        await Assert.That(name.Labels[0]).IsEqualTo("org");

        await Assert.That(name).IsEquatableOrEqualTo("org");
    }

    [Test]
    public async Task Root()
    {
        var name = new DomainName("");

        await Assert.That(name.Labels).HasCount().Zero();
        await Assert.That(name).IsEquatableOrEqualTo("");
    }

    [Test]
    public async Task EscapedDotCharacter()
    {
        var name = new DomainName(@"my\.example.org");

        await Assert.That(name.Labels).HasCount(2);
        await Assert.That(name.Labels[0]).IsEqualTo("my.example");
        await Assert.That(name.Labels[1]).IsEqualTo("org");
        await Assert.That(name).IsEquatableOrEqualTo(@"my\.example.org");
    }

    [Test]
    public async Task EscapedDotDigits()
    {
        var name = new DomainName(@"my\046example.org");

        await Assert.That(name.Labels).HasCount(2);
        await Assert.That(name.Labels[0]).IsEqualTo("my.example");
        await Assert.That(name.Labels[1]).IsEqualTo("org");
        await Assert.That(name).IsEquatableOrEqualTo(@"my\.example.org");
    }

    [Test]
    public async Task ImplicitParsingOfString()
    {
        DomainName name = @"my\046example.org";
        await Assert.That(name.Labels).HasCount(2);
        await Assert.That(name.Labels[0]).IsEqualTo("my.example");
        await Assert.That(name.Labels[1]).IsEqualTo("org");

        name = @"my\.example.org";
        await Assert.That(name.Labels).HasCount(2);
        await Assert.That(name.Labels[0]).IsEqualTo("my.example");
        await Assert.That(name.Labels[1]).IsEqualTo("org");

        name = "my.example.org";
        await Assert.That(name.Labels).HasCount(3);
        await Assert.That(name.Labels[0]).IsEqualTo("my");
        await Assert.That(name.Labels[1]).IsEqualTo("example");
        await Assert.That(name.Labels[2]).IsEqualTo("org");
    }

    [Test]
    public async Task FromLabels()
    {
        var name = new DomainName("my.example", "org");

        await Assert.That(name.Labels).HasCount(2);
        await Assert.That(name.Labels[0]).IsEqualTo("my.example");
        await Assert.That(name.Labels[1]).IsEqualTo("org");
        await Assert.That(name).IsEquatableOrEqualTo(@"my\.example.org");
    }

    [Test]
    public async Task Equality()
    {
        var a = new DomainName(@"my\.example.org");
        var b = new DomainName("my.example", "org");
        var c = new DomainName(@"my\046example.org");
        var d = new DomainName(@"My\.EXAMPLe.ORg");
        var other1 = new DomainName("example.org");
        var other2 = new DomainName("org");

        await Assert.That(a).IsEqualTo(b);
        await Assert.That(a).IsEqualTo(c);
        await Assert.That(a).IsEqualTo(d);
        await Assert.That(a).IsNotEqualTo(other1);
        await Assert.That(a).IsNotEqualTo(other2);
        await Assert.That(a)!.IsNotEqualTo(null);

        await Assert.That(a == b).IsTrue();
        await Assert.That(a == c).IsTrue();
        await Assert.That(a == d).IsTrue();
        await Assert.That(a == other1).IsFalse();
        await Assert.That(a == other2).IsFalse();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        await Assert.That(a == null).IsFalse();

        await Assert.That(a != b).IsFalse();
        await Assert.That(a != c).IsFalse();
        await Assert.That(a != d).IsFalse();
        await Assert.That(a != other1).IsTrue();
        await Assert.That(a != other2).IsTrue();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        await Assert.That(a != null).IsTrue();

        await Assert.That(a!.Equals(b)).IsTrue();
        await Assert.That(a.Equals(c)).IsTrue();
        await Assert.That(a.Equals(d)).IsTrue();
        await Assert.That(a.Equals(other1)).IsFalse();
        await Assert.That(a.Equals(other2)).IsFalse();
        await Assert.That(a.Equals(null)).IsFalse();
    }

    [Test]
    public async Task HashEquality()
    {
        var a = new DomainName(@"my\.example.org");
        var b = new DomainName("my.example", "org");
        var c = new DomainName(@"my\046example.org");
        var d = new DomainName(@"My\.EXAMPLe.ORg");
        var other1 = new DomainName("example.org");
        var other2 = new DomainName("org");

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
        await Assert.That(a.GetHashCode()).IsEqualTo(c.GetHashCode());
        await Assert.That(a.GetHashCode()).IsEqualTo(d.GetHashCode());
        await Assert.That(a.GetHashCode()).IsNotEqualTo(other1.GetHashCode());
        await Assert.That(a.GetHashCode()).IsNotEqualTo(other2.GetHashCode());
    }

    [Test]
    public async Task ToCanonical()
    {
        var a = new DomainName("My.EXAMPLe.ORg");

        await Assert.That(a).IsEquatableOrEqualTo("My.EXAMPLe.ORg");
        await Assert.That(a.ToCanonical()).IsEquatableOrEqualTo("my.example.org");
    }

    [Test]
    public async Task IsSubdomainOf()
    {
        var zone = new DomainName("example.org");

        await Assert.That(zone.IsSubdomainOf(zone)).IsFalse();
        await Assert.That(new DomainName("a.example.org").IsSubdomainOf(zone)).IsTrue();
        await Assert.That(new DomainName("a.b.example.org").IsSubdomainOf(zone)).IsTrue();
        await Assert.That(new DomainName("a.Example.org").IsSubdomainOf(zone)).IsTrue();
        await Assert.That(new DomainName("a.b.Example.ORG").IsSubdomainOf(zone)).IsTrue();
        await Assert.That(new DomainName(@"a\.example.org").IsSubdomainOf(zone)).IsFalse();
        await Assert.That(new DomainName(@"a\.b.example.org").IsSubdomainOf(zone)).IsTrue();
        await Assert.That(new DomainName(@"a\.b.example.ORG").IsSubdomainOf(zone)).IsTrue();
        await Assert.That(new DomainName("a.org").IsSubdomainOf(zone)).IsFalse();
        await Assert.That(new DomainName("a.b.org").IsSubdomainOf(zone)).IsFalse();
    }

    [Test]
    public async Task BelongsTo()
    {
        var zone = new DomainName("example.org");

        await Assert.That(zone.BelongsTo(zone)).IsTrue();
        await Assert.That(new DomainName("ExamPLE.Org").BelongsTo(zone)).IsTrue();
        await Assert.That(new DomainName("A.ExamPLE.Org").BelongsTo(zone)).IsTrue();
        await Assert.That(new DomainName("a.example.org").BelongsTo(zone)).IsTrue();
        await Assert.That(new DomainName("a.b.example.org").BelongsTo(zone)).IsTrue();
        await Assert.That(new DomainName("a.Example.org").BelongsTo(zone)).IsTrue();
        await Assert.That(new DomainName("a.b.Example.ORG").BelongsTo(zone)).IsTrue();
        await Assert.That(new DomainName(@"a\.example.org").BelongsTo(zone)).IsFalse();
        await Assert.That(new DomainName(@"a\.b.example.org").BelongsTo(zone)).IsTrue();
        await Assert.That(new DomainName(@"a\.b.example.ORG").BelongsTo(zone)).IsTrue();
        await Assert.That(new DomainName("a.org").BelongsTo(zone)).IsFalse();
        await Assert.That(new DomainName("a.b.org").BelongsTo(zone)).IsFalse();
    }

    [Test]
    public async Task Parent()
    {
        var name = new DomainName(@"a.b\.c.example.org");
        var expected = new DomainName(@"b\.c.example.org");
        await Assert.That(expected).IsEqualTo(name.Parent());

        name = new DomainName("org");
        expected = new DomainName("");
        await Assert.That(expected).IsEqualTo(name.Parent());
        await Assert.That(expected.Parent()).IsNull();
    }

    [Test]
    public async Task Joining()
    {
        var a = new DomainName(@"foo\.bar");
        var b = new DomainName("x.y.z");
        var c = DomainName.Join(a, b);

        await Assert.That(c.Labels.Count).IsEqualTo(4);
        await Assert.That(c.Labels[0]).IsEqualTo("foo.bar");
        await Assert.That(c.Labels[1]).IsEqualTo("x");
        await Assert.That(c.Labels[2]).IsEqualTo("y");
        await Assert.That(c.Labels[3]).IsEqualTo("z");
    }

    [Test]
    public async Task Rfc4343_Section_2()
    {
        await Assert.That(new DomainName("foo.example.net.")).IsEqualTo(new DomainName("Foo.ExamplE.net."));
        await Assert.That(new DomainName("69.2.0.192.in-addr.arpa.")).IsEqualTo(new DomainName("69.2.0.192.in-ADDR.ARPA."));
    }

    [Test]
    public async Task Rfc4343_Section_21_Backslash()
    {
        var aslashb = new DomainName(@"a\\b");

        await Assert.That(aslashb.Labels.Count).IsEqualTo(1);
        await Assert.That(aslashb.Labels[0]).IsEqualTo(@"a\b");
        await Assert.That(aslashb).IsEquatableOrEqualTo(@"a\092b");
        await Assert.That(aslashb).IsEqualTo(new DomainName(@"a\092b"));
    }

    [Test]
    public async Task Rfc4343_Section_21_4Digits()
    {
        var a = new DomainName(@"a\\4");
        var b = new DomainName(@"a\0924");

        await Assert.That(a).IsEqualTo(b);
    }

    [Test]
    public async Task Rfc4343_Section_22_SpacesAndDots()
    {
        var a = new DomainName(@"Donald\032E\.\032Eastlake\0323rd.example");

        await Assert.That(a.Labels.Count).IsEqualTo(2);
        await Assert.That(a.Labels[0]).IsEqualTo("Donald E. Eastlake 3rd");
        await Assert.That(a.Labels[1]).IsEqualTo("example");
    }

    [Test]
    public async Task Rfc4343_Section_22_Binary()
    {
        var a = new DomainName(@"a\000\\\255z.example");

        await Assert.That(a.Labels.Count).IsEqualTo(2);
        await Assert.That(a.Labels[0][0]).IsEqualTo('a');
        await Assert.That(a.Labels[0][1]).IsEqualTo((char)0);
        await Assert.That(a.Labels[0][2]).IsEqualTo('\\');
        await Assert.That(a.Labels[0][3]).IsEqualTo((char)0xff);
        await Assert.That(a.Labels[0][4]).IsEqualTo('z');
        await Assert.That(a.Labels[1]).IsEqualTo("example");
        await Assert.That(a).IsEquatableOrEqualTo(@"a\000\092\255z.example");
        await Assert.That(a).IsEqualTo(new DomainName(a.ToString()));
    }

    [Test]
    public async Task FormattedString()
    {
        var name = new DomainName(@"foo ~ \.bar-12A.org");

        await Assert.That(name).IsEquatableOrEqualTo(@"foo\032~\032\.bar-12A.org");
    }
}
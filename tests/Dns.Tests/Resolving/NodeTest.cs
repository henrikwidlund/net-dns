using Makaretu.Dns;
using Makaretu.Dns.Resolving;
using Shouldly;
using Xunit;

namespace DnsTests.Resolving;

public class NodeTest
{
    [Test]
    public void Defaults()
    {
        var node = new Node();

        node.Name.ShouldBe(DomainName.Root);
        node.Resources.Count.ShouldBe(0);
        node.ToString().ShouldBe("");
    }

    [Test]
    public void DuplicateResources()
    {
        var node = new Node();
        var a = new PTRRecord { Name = "a", DomainName = "alpha" };
        var b = new PTRRecord { Name = "a", DomainName = "alpha" };
        a.ShouldBe(b);

        node.Resources.Add(a);
        node.Resources.Add(b);
        node.Resources.Add(a);
        node.Resources.Add(b);
        node.Resources.Count.ShouldBe(1);
    }
}
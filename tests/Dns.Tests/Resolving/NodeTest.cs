using System.Threading.Tasks;
using Makaretu.Dns;
using Makaretu.Dns.Resolving;

namespace DnsTests.Resolving;

public class NodeTest
{
    [Test]
    public async Task Defaults()
    {
        var node = new Node();

        await Assert.That(node.Name).IsEqualTo(DomainName.Root);
        await Assert.That(node.Resources).Count().IsEqualTo(0);
        await Assert.That(node.ToString()).IsEqualTo("");
    }

    [Test]
    public async Task DuplicateResources()
    {
        var node = new Node();
        var a = new PTRRecord { Name = "a", DomainName = "alpha" };
        var b = new PTRRecord { Name = "a", DomainName = "alpha" };

        await Assert.That(a).IsEqualTo(b);

        node.Resources.Add(a);
        node.Resources.Add(b);
        node.Resources.Add(a);
        node.Resources.Add(b);
        await Assert.That(node.Resources).Count().IsEqualTo(1);
    }
}
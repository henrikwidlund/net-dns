using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class DigestRegistryTest
{
    [Test]
    public async Task Exists() => await Assert.That(DigestRegistry.Digests).HasCount().NotEqualTo(0);
}
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class DigestRegistryTest
{
    [Test]
    public void Exists() => DigestRegistry.Digests.Count.ShouldNotBe(0);
}
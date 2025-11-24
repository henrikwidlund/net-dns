using System;
using System.Threading.Tasks;

using Makaretu.Dns;

namespace DnsTests;

public class SecurityAlgorithmRegistryTest
{
    [Test]
    public async Task Exists() => await Assert.That(SecurityAlgorithmRegistry.Algorithms).HasCount().NotEqualTo(0);

    [Test]
    public async Task RSASHA1()
    {
        var metadata = SecurityAlgorithmRegistry.GetMetadata(SecurityAlgorithm.RSASHA1);
        await Assert.That(metadata).IsNotNull();
    }

    [Test]
    public void UnknownAlgorithm() => Assert.Throws<NotImplementedException>(static () => SecurityAlgorithmRegistry.GetMetadata((SecurityAlgorithm)0xBA));
}
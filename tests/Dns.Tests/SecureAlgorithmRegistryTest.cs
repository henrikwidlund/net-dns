using System;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class SecurityAlgorithmRegistryTest
{
    [Test]
    public void Exists() => SecurityAlgorithmRegistry.Algorithms.Count.ShouldNotBe(0);

    [Test]
    public void RSASHA1()
    {
        var metadata = SecurityAlgorithmRegistry.GetMetadata(SecurityAlgorithm.RSASHA1);
        metadata.ShouldNotBeNull();
    }

    [Test]
    public void UnknownAlgorithm() => Should.Throw<NotImplementedException>(static () => SecurityAlgorithmRegistry.GetMetadata((SecurityAlgorithm)0xBA));
}
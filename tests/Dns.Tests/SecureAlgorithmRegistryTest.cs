using System;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class SecurityAlgorithmRegistryTest
{
    [Fact]
    public void Exists() => SecurityAlgorithmRegistry.Algorithms.Count.ShouldNotBe(0);

    [Fact]
    public void RSASHA1()
    {
        var metadata = SecurityAlgorithmRegistry.GetMetadata(SecurityAlgorithm.RSASHA1);
        metadata.ShouldNotBeNull();
    }

    [Fact]
    public void UnknownAlgorithm() => Should.Throw<NotImplementedException>(static () => SecurityAlgorithmRegistry.GetMetadata((SecurityAlgorithm)0xBA));
}
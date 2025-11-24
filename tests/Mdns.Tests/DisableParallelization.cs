using TUnit.Core.Interfaces;

namespace Makaretu.Mdns;

internal sealed record SingleTestRateLimit : IParallelLimit
{
    public int Limit => 1;
}
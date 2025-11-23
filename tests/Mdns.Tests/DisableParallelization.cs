using Makaretu.Mdns;

using TUnit.Core.Interfaces;

[assembly: ParallelLimiter<SingleTestRateLimit>]

namespace Makaretu.Mdns;

internal record SingleTestRateLimit : IParallelLimit
{
    public int Limit => 1;
}
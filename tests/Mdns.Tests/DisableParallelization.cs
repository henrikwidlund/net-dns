using Makaretu.Mdns;

using TUnit.Core.Interfaces;

[assembly: ParallelLimiter<MyParallelLimit>]

namespace Makaretu.Mdns;

internal record MyParallelLimit : IParallelLimit
{
    public int Limit => 1;
}
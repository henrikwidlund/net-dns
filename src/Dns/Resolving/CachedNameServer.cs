namespace Makaretu.Dns.Resolving;

/// <summary>
///   A caching name server.
/// </summary>
public class CachedNameServer : NameServer
{
    /// <summary>
    ///   Removes any expired resource record from the cache.
    /// </summary>
    /// <param name="now">
    ///   The time to use to determine if a resource record is expired.
    ///   Defaults to <see cref="DateTime.UtcNow"/>.
    /// </param>
    /// <remarks>
    ///   Authoritative nodes are not pruned.
    /// </remarks>
    public void Prune(DateTime? now = null)
    {
        now ??= DateTime.UtcNow;

        var nodes = Catalog?.Values.Where(static node => !node.Authoritative) ?? [];
        foreach (var resources in nodes.Select(static node => node.Resources))
        {
            foreach (var resource in resources.Where(r => r.IsExpired(now)))
                resources.Remove(resource);
        }
    }

    /// <summary>
    ///  Prune the cache in the background.
    /// </summary>
    /// <param name="interval">
    ///  The delay between pruning.
    /// </param>
    /// <returns>
    ///   Allows cancellation of the background task.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="interval"/> is zero or negative.</exception>
    /// <seealso cref="Prune"/>
    public CancellationTokenSource PruneContinuously(TimeSpan interval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(interval, TimeSpan.Zero);

        var cts = new CancellationTokenSource();
        _ = PruneContinuouslyAsync(interval, cts.Token);
        return cts;
    }

    private async Task PruneContinuouslyAsync(TimeSpan interval, CancellationToken cancellationToken)
    {
        // Prune in the background, not on the caller's thread
        await Task.Yield();

        using var timer = new PeriodicTimer(interval);
        try
        {
            do
            {
                Prune();
            }
            while (await timer.WaitForNextTickAsync(cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Stopped by the caller
        }
    }

    /// <summary>
    ///   Cache the response.
    /// </summary>
    /// <param name="response">
    ///   A response from a name server.
    /// </param>
    /// <remarks>
    ///   Both the <see cref="Message.Answers"/> and
    ///   the <see cref="Message.AdditionalRecords"/> are added to the cache.
    ///   Only resources records with a positive <see cref="ResourceRecord.TTL"/>
    ///   are added.
    /// </remarks>
    public void Add(Message response)
    {
        var resources = response
            .Answers.Concat(response.AdditionalRecords)
            .Where(static r => r.TTL > TimeSpan.Zero);

        foreach (var resource in resources)
            Catalog?.Add(resource);
    }
}

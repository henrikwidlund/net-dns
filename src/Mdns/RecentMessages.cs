using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Makaretu.Dns;

/// <summary>
///   Maintains a sequence of recent messages.
/// </summary>
/// <remarks>
///   <b>RecentMessages</b> is used to determine if a message has already been
///   processed within the specified <see cref="Interval"/>.
/// </remarks>
public class RecentMessages(TimeProvider timeProvider)
{
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>
    ///   Recent messages.
    /// </summary>
    /// <value>
    ///   The key is the Base64 encoding of the SHA-1 hash of
    ///   a message and the value is when the message was seen.
    /// </value>
    /// <remarks>
    ///   Base64 is case-sensitive, so the keys must be compared ordinally.
    /// </remarks>
    private readonly ConcurrentDictionary<UInt128, DateTimeOffset> _messages = [];

    public RecentMessages() : this(TimeProvider.System) { }

    /// <summary>
    /// The number of messages.
    /// </summary>
    public int Count => _messages.Count;

    /// <summary>
    ///   The time interval used to determine if a message is recent.
    /// </summary>
    public TimeSpan Interval { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Checks if a message has been added to the recent message list.
    /// </summary>
    /// <param name="message">The message to look for.</param>
    public bool HasMessage(ReadOnlySpan<byte> message) => _messages.ContainsKey(GetId(message));

    /// <summary>
    ///   Try adding a message to the recent message list.
    /// </summary>
    /// <param name="message">
    ///   The binary representation of a message.
    /// </param>
    /// <returns>
    ///   <b>true</b> if the message, did not already exist; otherwise,
    ///   <b>false</b> the message exists within the <see cref="Interval"/>.
    /// </returns>
    public bool TryAdd(ReadOnlySpan<byte> message)
    {
        Prune();
        return _messages.TryAdd(GetId(message), _timeProvider.GetUtcNow());
    }

    /// <summary>
    ///   Remove any messages that are stale.
    /// </summary>
    /// <returns>
    ///   The number messages that were pruned.
    /// </returns>
    /// <remarks>
    ///   Anything older than an <see cref="Interval"/> ago is removed.
    /// </remarks>
    public int Prune()
    {
        // UTC, local time would jump around daylight saving time changes
        var dead = _timeProvider.GetUtcNow() - Interval;

        return _messages.Count(x => x.Value < dead && _messages.TryRemove(x.Key, out _));
    }

    /// <summary>
    ///   Gets a unique ID for a message.
    /// </summary>
    /// <param name="message">
    ///   The binary representation of a message.
    /// </param>
    /// <returns>
    ///   The Base64 encoding of the SHA-1 hash of the <paramref name="message"/>.
    /// </returns>
    public static UInt128 GetId(ReadOnlySpan<byte> message)
    {
        Span<byte> hash = stackalloc byte[SHA1.HashSizeInBytes];
        SHA1.HashData(message, hash);
        // The first 128 bits are plenty to tell recent messages apart
        return BinaryPrimitives.ReadUInt128LittleEndian(hash);
    }
}

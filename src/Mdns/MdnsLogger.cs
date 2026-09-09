using System.Net;

using Microsoft.Extensions.Logging;

namespace Makaretu.Dns;

internal static partial class MdnsLogger
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Will send via {localEndpoint}")]
    public static partial void WillSendVia(this ILogger logger, IPEndPoint localEndpoint);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Cannot setup send socket for {Address}")]
    public static partial void SocketSetupFail(this ILogger logger, IPAddress address, Exception exception);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Sender {Key} failure.")]
    public static partial void SenderKeyFailure(this ILogger logger, IPAddress key, Exception exception);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Receiver failure.")]
    public static partial void ReceiverFailure(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Finding network interfaces")]
    public static partial void FindingNetworkInterfaces(this ILogger logger);

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Removed nic '{NicName}'.")]
    public static partial void RemovedNic(this ILogger logger, string nicName);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Found nic '{NicName}")]
    public static partial void FoundNic(this ILogger logger, string nicName);

    [LoggerMessage(EventId = 8, Level = LogLevel.Error, Message = "Find Nics failed")]
    public static partial void FindNicsFailed(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 9, Level = LogLevel.Warning, Message = "Received malformed message")]
    public static partial void ReceivedMalformedMessage(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 16, Level = LogLevel.Error, Message = "Receive handler failed")]
    public static partial void ReceiveHandlerFailed(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 10, Level = LogLevel.Debug, Message = "Answer from {RemoteEndPoint}")]
    public static partial void AnswerFromRemoteEndpoint(this ILogger logger, IPEndPoint remoteEndpoint);

    [LoggerMessage(EventId = 11, Level = LogLevel.Trace, Message = "{@Message}")]
    public static partial void AnswerMessageReceived(this ILogger logger, Message message);

    [LoggerMessage(EventId = 12, Level = LogLevel.Debug, Message = "Query from {RemoteEndPoint}")]
    public static partial void QueryFromRemoteEndpoint(this ILogger logger, IPEndPoint remoteEndpoint);

    [LoggerMessage(EventId = 13, Level = LogLevel.Trace, Message = "{@Message}")]
    public static partial void QueryMessageReceived(this ILogger logger, Message message);

    [LoggerMessage(EventId = 14, Level = LogLevel.Debug, Message = "Sending answer")]
    public static partial void SendingAnswer(this ILogger logger);

    [LoggerMessage(EventId = 15, Level = LogLevel.Trace, Message = "{@Message}")]
    public static partial void SendingQueryAnswer(this ILogger logger, Message message);
}

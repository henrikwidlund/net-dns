using System.Net;

using Microsoft.Extensions.Logging;

namespace Makaretu.Dns;

internal static partial class MdnsLogger
{
    [LoggerMessage(EventId = 1, EventName = nameof(WillSendVia), Level = LogLevel.Debug,
        Message = "Will send via {localEndpoint}")]
    public static partial void WillSendVia(this ILogger logger, IPEndPoint localEndpoint);

    private static readonly Action<ILogger, IPAddress, Exception> _socketSetupFail = LoggerMessage.Define<IPAddress>(
        LogLevel.Error,
        new EventId(2, nameof(SocketSetupFail)),
        "Cannot setup send socket for {Address}");

    public static void SocketSetupFail(this ILogger logger, Exception exception, IPAddress address) =>
        _socketSetupFail(logger, address, exception);

    private static readonly Action<ILogger, IPAddress, Exception> _senderKeyFailure = LoggerMessage.Define<IPAddress>(
        LogLevel.Information,
        new EventId(3, nameof(SocketSetupFail)),
        "Sender {Key} failure.");

    public static void SenderKeyFailure(this ILogger logger, Exception exception, IPAddress address) =>
        _senderKeyFailure(logger, address, exception);

    private static readonly Action<ILogger, Exception> _receiverFailure = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(4, nameof(ReceiverFailure)),
        "Receiver failure.");

    public static void ReceiverFailure(this ILogger logger, Exception exception) =>
        _receiverFailure(logger, exception);

    [LoggerMessage(EventId = 5, EventName = nameof(FindingNetworkInterfaces), Level = LogLevel.Debug,
        Message = "Finding network interfaces")]
    public static partial void FindingNetworkInterfaces(this ILogger logger);

    [LoggerMessage(EventId = 6, EventName = nameof(RemovedNic), Level = LogLevel.Debug,
        Message = "Removed nic '{NicName}'.")]
    public static partial void RemovedNic(this ILogger logger, string nicName);

    [LoggerMessage(EventId = 7, EventName = nameof(FoundNic), Level = LogLevel.Debug,
        Message = "Found nic '{NicName}")]
    public static partial void FoundNic(this ILogger logger, string nicName);

    private static readonly Action<ILogger, Exception> _findNicsFailed = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(8, nameof(FindNicsFailed)),
        "Find Nics failed");

    public static void FindNicsFailed(this ILogger logger, Exception exception) =>
        _findNicsFailed(logger, exception);

    private static readonly Action<ILogger, Exception> _receivedMalformedMessage = LoggerMessage.Define(
        LogLevel.Warning,
        new EventId(9, nameof(ReceivedMalformedMessage)),
        "Received malformed message");

    public static void ReceivedMalformedMessage(this ILogger logger, Exception exception) =>
        _receivedMalformedMessage(logger, exception);

    private static readonly Action<ILogger, Exception> _receiveHandlerFailed = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(9, nameof(ReceiveHandlerFailed)),
        "Receive handler failed");

    public static void ReceiveHandlerFailed(this ILogger logger, Exception exception) =>
        _receiveHandlerFailed(logger, exception);

    [LoggerMessage(EventId = 10, EventName = nameof(AnswerFromRemoteEndpoint), Level = LogLevel.Debug,
        Message = "Answer from {RemoteEndPoint}")]
    public static partial void AnswerFromRemoteEndpoint(this ILogger logger, IPEndPoint remoteEndpoint);

    [LoggerMessage(EventId = 11, EventName = nameof(AnswerMessageReceived), Level = LogLevel.Trace,
        Message = "{@Message}")]
    public static partial void AnswerMessageReceived(this ILogger logger, Message message);

    [LoggerMessage(EventId = 12, EventName = nameof(QueryFromRemoteEndpoint), Level = LogLevel.Debug,
        Message = "Query from {RemoteEndPoint}")]
    public static partial void QueryFromRemoteEndpoint(this ILogger logger, IPEndPoint remoteEndpoint);

    [LoggerMessage(EventId = 13, EventName = nameof(QueryMessageReceived), Level = LogLevel.Trace,
        Message = "{@Message}")]
    public static partial void QueryMessageReceived(this ILogger logger, Message message);

    [LoggerMessage(EventId = 14, EventName = nameof(SendingAnswer), Level = LogLevel.Debug,
        Message = "Sending answer")]
    public static partial void SendingAnswer(this ILogger logger);

    [LoggerMessage(EventId = 15, EventName = nameof(SendingQueryAnswer), Level = LogLevel.Trace,
        Message = "{@Message}")]
    public static partial void SendingQueryAnswer(this ILogger logger, Message message);
}
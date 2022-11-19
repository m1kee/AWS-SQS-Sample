using Messages;
using Microsoft.Extensions.Logging;

namespace Handlers;

public class GenerateReportHandler : IMessageHandler
{
    private readonly ILogger<GenerateReportHandler> _logger;

    public GenerateReportHandler(ILogger<GenerateReportHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(IMessage message) 
    {
        var messageAsType = Convert.ChangeType(message, MessageType);
        _logger.LogInformation(messageAsType.ToString());
        return Task.CompletedTask;
    }

    public static Type MessageType { get; } = typeof(GenerateReportMessage);
}
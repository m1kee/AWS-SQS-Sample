
using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Handlers;
using Messages;

namespace ReportsWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IAmazonSQS _sqs;
    private readonly MessageDispatcher _dispatcher;
    private const string QueueName = "Reports";
    private readonly List<string> _messageAttributeNames = new() { "All" };

    public Worker(ILogger<Worker> logger, IAmazonSQS sqs, MessageDispatcher dispatcher)
    {
        _logger = logger;
        _sqs = sqs;
        _dispatcher = dispatcher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var queue = await _sqs.GetQueueUrlAsync(QueueName, stoppingToken);
        var receiveRequest = new ReceiveMessageRequest
        {
            QueueUrl = queue.QueueUrl,
            MessageAttributeNames = _messageAttributeNames,
            AttributeNames = _messageAttributeNames
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await _sqs.ReceiveMessageAsync(receiveRequest, stoppingToken);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK) {
                _logger.LogError("Can't get messages from AWS");
                continue;
            }

            // waiting 3 seconds after processing messages
            await Task.Delay(3000);

            foreach (Message message in response.Messages)
            {
                try
                {
                    var messageTypeName = message.MessageAttributes
                    .GetValueOrDefault(nameof(IMessage.MessageTypeName))?.StringValue;

                    if (messageTypeName is null)
                    {
                        throw new Exception("Missing MessageTypeName metadata, message will be ignored");
                    }

                    _logger.LogInformation($"Check if worker can handle message with MessageTypeName: {messageTypeName}.");
                    if (!_dispatcher.CanHandleMessageType(messageTypeName))
                    {
                        throw new Exception("This worker can't handle the message, message will be skiped");
                    }

                    // this should return a GenerateReportMessage type
                    var messageType = _dispatcher.GetMessageTypeByName(messageTypeName)!;

                    IMessage messageAsType = (IMessage)JsonSerializer.Deserialize(message.Body, messageType)!;
                    await _dispatcher.DispatchAsync(messageAsType);

                    // delete after processing the message
                    await _sqs.DeleteMessageAsync(queue.QueueUrl, message.ReceiptHandle, stoppingToken);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    continue;
                }
            }
        }
    }
}

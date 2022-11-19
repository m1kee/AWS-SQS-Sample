
using System.Text.Json;
using AmazonServices;
using Amazon.SQS.Model;

namespace ReportsWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ISimpleQueueService _sqs;
    private readonly HandlerManager _handlerManager;
    private const string QueueName = "Reports";
    private readonly List<string> _messageAttributeNames = new () { "All" };
    private readonly List<string> _attributeNames = new () { "All" };

    public Worker(ILogger<Worker> logger, ISimpleQueueService sqs, HandlerManager handlerManager)
    {
        _logger = logger;
        _sqs = sqs;
        _handlerManager = handlerManager;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueUrl = await _sqs.GetQueueUrlAsync(QueueName, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var response = await _sqs.ReceiveMessagesAsync(queueUrl.QueueUrl, _messageAttributeNames, _attributeNames, null, stoppingToken);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("Can't get messages from AWS");
                continue;
            }

            await ProcessMessages(queueUrl, response, stoppingToken);
        }
    }
    private async Task ProcessMessages(GetQueueUrlResponse queue, ReceiveMessageResponse response, CancellationToken stoppingToken)
    {
        int retrievedMessages = response.Messages.Count;
        _logger.LogInformation($"{retrievedMessages} message{(retrievedMessages > 1 ? "s" : "")} retrieved.");

        foreach (var message in response.Messages)
        {
            try
            {
                await ProcessMessage(queue, message, stoppingToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                continue;
            }
        }
    }
    private async Task ProcessMessage(GetQueueUrlResponse queue, Message message, CancellationToken stoppingToken)
    {
        var messageTypeName = message.MessageAttributes
            .GetValueOrDefault(nameof(IMessage.MessageTypeName))?
            .StringValue;

        if (messageTypeName is null)
            throw new Exception("Missing MessageTypeName metadata, message will be ignored");

        
        if (!_handlerManager.CanHandleMessageType(messageTypeName))
            throw new Exception("This worker can't handle the message, message will be skipped");

        var messageType = _handlerManager.GetMessageTypeByName(messageTypeName)!;

        IMessage messageAsType = (IMessage)JsonSerializer.Deserialize(message.Body, messageType)!;
        await _handlerManager.HandleAsync(messageAsType);

        // fake delay to see if the message is take by other worker
        //_logger.LogInformation("Delaying the message...");
        //await Task.Delay(20000);

        // delete after processing the message
        await _sqs.DeleteMessageAsync(queue.QueueUrl, message.ReceiptHandle, stoppingToken);
    }
}

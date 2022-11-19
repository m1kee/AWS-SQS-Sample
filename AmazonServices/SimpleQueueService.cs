using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;

namespace AmazonServices;

public class SimpleQueueService : ISimpleQueueService, IAmazonService
{
    private readonly ILogger<SimpleQueueService> _logger;
    private readonly IAmazonSQS _client;
    private readonly int DEFAULT_WAIT_TIME_SECONDS = 20;
    public SimpleQueueService(ILogger<SimpleQueueService> logger, IAmazonSQS client)
    {
        this._logger = logger;
        this._client = client;
    }

    public async Task SendMessageAsync<TMessage>(string queueUrl, TMessage message, CancellationToken cancellationToken) where TMessage : IMessage
    {   
        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(message),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    nameof(IMessage.MessageTypeName), new MessageAttributeValue
                    {
                        StringValue = message.MessageTypeName,
                        DataType = "String"
                    }
                }
            }
        };

        _logger.LogInformation($"Sending message...");
        SendMessageResponse response = await _client.SendMessageAsync(request);
        _logger.LogInformation($"Message was sent: { JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true })}");
    }

    public async Task<GetQueueUrlResponse> GetQueueUrlAsync(string queueName, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Getting queue url by name: {queueName}");
        return await _client.GetQueueUrlAsync(queueName, cancellationToken);
    }

    public async Task DeleteMessageAsync(string queueUrl, string receiptHandle, CancellationToken cancellationToken) 
    {
        _logger.LogInformation($"Deleting message: {receiptHandle} from the {queueUrl} queue");
        await _client.DeleteMessageAsync(queueUrl, receiptHandle, cancellationToken);
    }

    public async Task<ReceiveMessageResponse> ReceiveMessagesAsync(string queueUrl, List<string> messageAttributeNames, List<string> attributeNames, int? waitTimeSeconds, CancellationToken cancellationToken) 
    {
        var receiveRequest = new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MessageAttributeNames = messageAttributeNames,
            AttributeNames = attributeNames,
            WaitTimeSeconds = waitTimeSeconds ?? DEFAULT_WAIT_TIME_SECONDS
        };

        _logger.LogInformation($"Getting messages...");
        return await _client.ReceiveMessageAsync(receiveRequest, cancellationToken);
    }
}

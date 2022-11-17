using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Messages;

namespace SimpleQueueService;

public class Dispatcher
{
    private readonly IAmazonSQS _sqs;

    public Dispatcher(IAmazonSQS sqs)
    {
        _sqs = sqs;
    }

    public async Task PublishAsync<TMessage>(string queueName, TMessage message)
        where TMessage : IMessage
    {
        var queueUrl = await _sqs.GetQueueUrlAsync(queueName);
        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl.QueueUrl,
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
        await _sqs.SendMessageAsync(request);
    }
}
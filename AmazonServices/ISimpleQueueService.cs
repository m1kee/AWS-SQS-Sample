using Amazon.SQS.Model;

namespace AmazonServices;

public interface ISimpleQueueService {
    Task DeleteMessageAsync(string queueUrl, string receiptHandle, CancellationToken cancellationToken);
    Task SendMessageAsync<TMessage>(string queueUrl, TMessage message, CancellationToken cancellationToken) where TMessage : IMessage;
    Task<GetQueueUrlResponse> GetQueueUrlAsync(string queueName, CancellationToken cancellationToken);
    Task<ReceiveMessageResponse> ReceiveMessagesAsync(string queueUrl, List<string> messageAttributeNames, List<string> attributeNames, int? waitTimeSeconds, CancellationToken cancellationToken);
}
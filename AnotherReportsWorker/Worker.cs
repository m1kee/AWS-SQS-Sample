
using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Handlers;
using Messages;

namespace AnotherReportsWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IAmazonSQS _sqs;
    private readonly GenerateReportHandler _handler;
    private const string QueueName = "Reports";
    private readonly List<string> _messageAttributeNames = new() { "All" };

    public Worker(ILogger<Worker> logger, IAmazonSQS sqs, GenerateReportHandler handler)
    {
        _logger = logger;
        _sqs = sqs;
        _handler = handler;
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

            foreach (Message message in response.Messages)
            {
                try
                {
                    IMessage generateReportMessage = (IMessage)JsonSerializer.Deserialize<GenerateReport>(message.Body)!;
                    await _handler.HandleAsync(generateReportMessage);

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

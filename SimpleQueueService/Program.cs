using Amazon;
using Amazon.SQS;
using AmazonServices;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var serviceProvider = new ServiceCollection()
    .AddLogging(builder => {
        builder
            .SetMinimumLevel(LogLevel.Trace)
            .AddConsole();
    })
    .AddSingleton<ISimpleQueueService, SimpleQueueService>()
    .AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient(RegionEndpoint.USEast1))
    .BuildServiceProvider();

var sqsService = serviceProvider.GetService<ISimpleQueueService>()!;
var logger = serviceProvider.GetService<ILoggerFactory>()!
    .CreateLogger<Program>();
var cancellationToken = new CancellationTokenSource();

try
{
    logger.LogInformation($"Getting reports queue");
    string reportsQueueName = "Reports";
    var queueUrl = await sqsService.GetQueueUrlAsync(reportsQueueName, cancellationToken.Token);
    logger.LogInformation($"Reports queue url: { queueUrl.QueueUrl }");

    logger.LogInformation($"Sending GenerateReportMessage...");
    var guid = Guid.NewGuid();
    await sqsService.SendMessageAsync(queueUrl.QueueUrl, new GenerateReportMessage {  
        Guid = guid,
        Name = "Product Release",
        RequestedUser = "mnunez"
    }, cancellationToken.Token);
    logger.LogInformation($"GenerateReportMessage Sent with Id: {guid}");

    logger.LogInformation($"Waiting 5 seconds to send another message...");
    await Task.Delay(5000);

    logger.LogInformation($"Sending GenerateReportMessage...");
    guid = Guid.NewGuid();
    await sqsService.SendMessageAsync(queueUrl.QueueUrl, new GenerateReportMessage {  
        Guid = guid,
        Name = "Product Release",
        RequestedUser = "djhon"
    }, cancellationToken.Token);
    logger.LogInformation($"GenerateReportMessage Sent with Id: {guid}");
}
catch (Exception ex) {
    logger.LogInformation(ex, ex.Message);
}
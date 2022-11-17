using Amazon;
using Amazon.SQS;
using Messages;
using SimpleQueueService;

var client = new AmazonSQSClient(RegionEndpoint.USEast1);
var dispatcher = new Dispatcher(client);

Console.WriteLine($"Sending GenerateReportMessage...");
var guid = Guid.NewGuid();
await dispatcher.PublishAsync("Reports", new GenerateReportMessage {  
    Guid = Guid.NewGuid(),
    Name = "Product Release",
    RequestedUser = "Michael Núñez"
});
Console.WriteLine($"GenerateReportMessage Sent with Id: {guid}");

Console.WriteLine($"Waiting 5 seconds to send another message...");
await Task.Delay(5000);

Console.WriteLine($"Sending GenerateReportMessage...");
guid = Guid.NewGuid();
await dispatcher.PublishAsync("Reports", new GenerateReportMessage {  
    Guid = Guid.NewGuid(),
    Name = "Product Release",
    RequestedUser = "Jhon Doe"
});
Console.WriteLine($"GenerateReportMessage Sent with Id: {guid}");
using Amazon;
using Amazon.SQS;
using Messages;
using SimpleQueueService;


var client = new AmazonSQSClient(RegionEndpoint.USEast1);
var dispatcher = new Dispatcher(client);

await dispatcher.PublishAsync("Reports", new GenerateReport {  
    Guid = Guid.NewGuid(),
    Name = "Product Release",
    RequestedUser = "Michael Núñez"
});

await Task.Delay(5000);

await dispatcher.PublishAsync("Reports", new GenerateReport {  
    Guid = Guid.NewGuid(),
    Name = "Product Release",
    RequestedUser = "Jhon Doe"
});
using Amazon;
using Amazon.SQS;
using ReportsWorker;
using ReportsWorker.Extensions;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.Configure<HostOptions>(options => {
            options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });
        services.AddHostedService<Worker>();
        services.AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient(RegionEndpoint.USEast1));
        services.AddSingleton<HandlerManager>();

        services.AddMessageHandlers();
    })
    .Build();

await host.RunAsync();

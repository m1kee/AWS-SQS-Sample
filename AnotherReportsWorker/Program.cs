using Amazon.SQS;
using AnotherReportsWorker;
using Handlers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.Configure<HostOptions>(options => {
            options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });
        services.AddHostedService<Worker>();
        services.AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient(Amazon.RegionEndpoint.USEast1));
        services.AddSingleton<GenerateReportHandler>();
    })
    .Build();

await host.RunAsync();

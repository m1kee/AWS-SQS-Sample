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
        services.AddSingleton<MessageDispatcher>();

        services.AddMessageHandlers();
    })
    .Build();

await host.RunAsync();

// var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddHostedService<Worker>();
// builder.Services.AddSingleton<IAmazonSQS>(_ => new AmazonSQSClient(RegionEndpoint.EUWest2));

// builder.Services.AddSingleton<MessageDispatcher>();

// // builder.Services.AddScoped<CustomerCreatedHandler>();
// // builder.Services.AddScoped<CustomerDeletedHandler>();
// builder.Services.AddMessageHandlers();

// var app = builder.Build();

// app.Run();

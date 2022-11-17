using System.Reflection;
using System.Text.Json;
using Handlers;
using Messages;


namespace ReportsWorker;

public class MessageDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MessageDispatcher> _logger;

    private readonly Dictionary<string, Type> _messageMappings;/* = new()
    {
        { nameof(GenerateReportMessage), typeof(GenerateReportMessage) },
        { nameof(GenerateReportMessage), typeof(GenerateReportMessage) },
    };*/
    
    private readonly Dictionary<string, Func<IServiceProvider, IMessageHandler>> _handlers;/* = new()
    {
        { nameof(GenerateReportMessage), provider => provider.GetRequiredService<GenerateReportHandler>() },
        { nameof(GenerateReportMessage), provider => provider.GetRequiredService<GenerateReportHandler>() },
    };*/

    public MessageDispatcher(IServiceScopeFactory scopeFactory, ILogger<MessageDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _messageMappings = default!;
        _handlers = default!;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            var mappings = assembly.DefinedTypes!
                .Where(x => typeof(IMessage).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToDictionary(info => info.Name, info => info.AsType());

            var handlers = assembly.DefinedTypes!
                .Where(x => typeof(IMessageHandler).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToDictionary<TypeInfo, string, Func<IServiceProvider, IMessageHandler>> (
                    info => ((Type)info.GetProperty(nameof(IMessageHandler.MessageType))!.GetValue(null)!)!.Name,
                    info => provider => (IMessageHandler)provider.GetRequiredService(info.AsType())
                );

            foreach (var mapping in mappings)
            {
                if (_messageMappings == null)
                    _messageMappings = new Dictionary<string, Type>();

                _messageMappings.Add(mapping.Key, mapping.Value);
            }

            foreach (var handler in handlers)
            {
                if (_handlers == null)
                    _handlers = new Dictionary<string, Func<IServiceProvider, IMessageHandler>>();

                _handlers.Add(handler.Key, handler.Value);
            }
        }        

        _logger.LogInformation($"handlers: {string.Join(Environment.NewLine, _handlers)}");
        _logger.LogInformation($"mappings: {string.Join(Environment.NewLine, _messageMappings)}");
    }

    public async Task DispatchAsync<TMessage>(TMessage message)
        where TMessage : IMessage
    {
        using var scope = _scopeFactory.CreateScope();
        IMessageHandler handler = _handlers[message.MessageTypeName](scope.ServiceProvider);
        await handler.HandleAsync(message);
    }

    public bool CanHandleMessageType(string messageTypeName)
    {
        return _handlers.ContainsKey(messageTypeName);
    }

    public Type? GetMessageTypeByName(string messageTypeName)
    {
        return _messageMappings.GetValueOrDefault(messageTypeName);
    }
}
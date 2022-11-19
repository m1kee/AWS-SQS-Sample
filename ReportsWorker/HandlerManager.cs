using System.Reflection;
using AmazonServices;
using Handlers;

namespace ReportsWorker;

public class HandlerManager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HandlerManager> _logger;
    private readonly Dictionary<string, Type> _messageMappings;
    private readonly Dictionary<string, Func<IServiceProvider, IMessageHandler>> _handlers;

    public HandlerManager(IServiceScopeFactory scopeFactory, ILogger<HandlerManager> logger)
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

    public async Task HandleAsync<TMessage>(TMessage message)
        where TMessage : IMessage
    {
        using var scope = _scopeFactory.CreateScope();
        IMessageHandler handler = _handlers[message.MessageTypeName](scope.ServiceProvider);
        await handler.HandleAsync(message);
    }

    public bool CanHandleMessageType(string messageTypeName)
    {
        _logger.LogInformation($"Check if worker can handle message with MessageTypeName: {messageTypeName}.");
        return _handlers.ContainsKey(messageTypeName);
    }

    public Type? GetMessageTypeByName(string messageTypeName)
    {
        _logger.LogInformation($"Getting type of: {messageTypeName}.");
        return _messageMappings.GetValueOrDefault(messageTypeName);
    }
}

using System.Reflection;
using Handlers;

namespace ReportsWorker.Extensions;

public static class HandlerExtensions
{
    public static IServiceCollection AddMessageHandlers(this IServiceCollection services)
    {
        var messageHandlerType = typeof(IMessageHandler);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            var handlers = assembly.DefinedTypes
                .Where(x => messageHandlerType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => x.AsType())
                .ToList();
            
            foreach (var handler in handlers)
            {
                services.AddScoped(handler);
            }
        }

        return services;
    }
}
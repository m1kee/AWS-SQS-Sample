using Messages;

namespace Handlers;

public interface IMessageHandler 
{
    public Task HandleAsync(IMessage message);

    public static Type MessageType { get; } = typeof(IMessageHandler);
}
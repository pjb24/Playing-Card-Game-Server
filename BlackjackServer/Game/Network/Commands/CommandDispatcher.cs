using Newtonsoft.Json.Linq;

public class CommandDispatcher
{
    private readonly Dictionary<string, Func<string, CommandContext, Task>> _handlers = new();

    public void Register<T>(string commandName, ICommandHandler<T> handler)
    {
        _handlers[commandName] = async (payload, context) => 
        {
            var command = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(payload);
            await handler.HandleAsync(command, context);
        };
    }

    public async Task Dispatch(string commandName, string payload, CommandContext context)
    {
        if (_handlers.TryGetValue(commandName, out var handler))
        {
            Console.WriteLine($"[Dispatcher] 명령 수행: {commandName}, ConnectionID: {context.ConnectionId}");
            await handler(payload, context);
        }
        else
        {
            Console.WriteLine($"[Dispatcher] 알 수 없는 명령: {commandName}");
        }
    }
}

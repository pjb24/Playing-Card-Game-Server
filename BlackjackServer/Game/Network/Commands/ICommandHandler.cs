using Microsoft.AspNetCore.SignalR;

public class CommandContext
{
    public string ConnectionId { get; set; }
    public HubCallerContext CallerContext { get; set; }
}

public interface ICommandHandler<T>
{
    Task HandleAsync(T command, CommandContext context);
}
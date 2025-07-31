using Microsoft.AspNetCore.SignalR;

public class LeaveGameCommandHandler : ICommandHandler<LeaveGameDTO>
{
    private IHubContext<BlackjackHub> _hubContext;

    public LeaveGameCommandHandler(IHubContext<BlackjackHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task HandleAsync(LeaveGameDTO command, CommandContext context)
    {
        await _hubContext.Groups.RemoveFromGroupAsync(context.ConnectionId, command.gameId);

        UserLeftDTO userLeftDTO = new();
        userLeftDTO.connectionId = context.ConnectionId;
        string userLeftJson = Newtonsoft.Json.JsonConvert.SerializeObject(userLeftDTO);
        await _hubContext.Clients.Group(command.gameId).SendAsync("ReceiveCommand", "UserLeft", userLeftJson);
    }
}
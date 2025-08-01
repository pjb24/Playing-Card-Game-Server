using Microsoft.AspNetCore.SignalR;

public class JoinGameCommandHandler : ICommandHandler<JoinGameDTO>
{
    private readonly IHubContext<BlackjackHub> _hubContext;
    private readonly UserManager _userManager;
    private readonly GameRoomManager _gameRoomManager;

    public JoinGameCommandHandler(IHubContext<BlackjackHub> hubContext, UserManager userManager, GameRoomManager gameRoomManager)
    {
        _hubContext = hubContext;
        _userManager = userManager;
        _gameRoomManager = gameRoomManager;
    }

    public async Task HandleAsync(JoinGameDTO command, CommandContext context)
    {
        var user = new User(command.userName, context.ConnectionId, command.userName);
        _userManager.AddUser(user);

        _gameRoomManager.CreateRoom("defaultRoom", _hubContext, _userManager);

        user = _userManager.GetUserByConnectionId(context.ConnectionId);
        if (user == null)
        {
            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "플레이어를 찾을 수 없습니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        var player = _userManager.GetPlayer(user.Id);
        if (player == null)
        {
            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "플레이어를 찾을 수 없습니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        _gameRoomManager.AddPlayerToRoom("defaultRoom", player);
        await _hubContext.Groups.AddToGroupAsync(context.ConnectionId, "defaultRoom");

        OnJoinSuccessDTO onJoinSuccessDTO = new();
        onJoinSuccessDTO.userName = command.userName;
        onJoinSuccessDTO.playerGuid = player.Guid.ToString();
        string onJoinSuccessJson = Newtonsoft.Json.JsonConvert.SerializeObject(onJoinSuccessDTO);
        await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnJoinSuccess", onJoinSuccessJson);

        OnUserJoinedDTO onUserJoinedDTO = new();
        onUserJoinedDTO.userName = command.userName;
        string onUserJoinedJson = Newtonsoft.Json.JsonConvert.SerializeObject(onUserJoinedDTO);
        await _hubContext.Clients.AllExcept(context.ConnectionId).SendAsync("ReceiveCommand", "OnUserJoined", onUserJoinedJson);

        OnPlayerRemainChipsDTO onPlayerRemainChipsDTO = new();
        onPlayerRemainChipsDTO.playerGuid = player.Guid.ToString();
        onPlayerRemainChipsDTO.chips = player.Chips.ToString();
        string onPlayerRemainChipsJson = Newtonsoft.Json.JsonConvert.SerializeObject(onPlayerRemainChipsDTO);
        await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnPlayerRemainChips", onPlayerRemainChipsJson);

        Console.WriteLine($"{command.userName} joined with connection ID: {context.ConnectionId}");
    }
}
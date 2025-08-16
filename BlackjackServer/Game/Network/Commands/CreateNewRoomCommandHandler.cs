using Microsoft.AspNetCore.SignalR;

public class CreateNewRoomCommandHandler : ICommandHandler<CreateNewRoomDTO>
{
    private IHubContext<BlackjackHub> _hubContext;
    private readonly UserManager _userManager;
    private readonly GameRoomManager _gameRoomManager;

    public CreateNewRoomCommandHandler(IHubContext<BlackjackHub> hubContext, UserManager userManager, GameRoomManager gameRoomManager)
    {
        _hubContext = hubContext;
        _userManager = userManager;
        _gameRoomManager = gameRoomManager;
    }

    public async Task HandleAsync(CreateNewRoomDTO command, CommandContext context)
    {
        var user = _userManager.GetUserByConnectionId(context.ConnectionId);
        if (user == null)
        {
            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "유저 데이터를 찾을 수 없습니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        var player = _userManager.GetPlayerByUserId(user.Id);
        if (player == null)
        {
            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "플레이어를 찾을 수 없습니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        bool created = _gameRoomManager.CreateRoom(command.roomName, _hubContext, _userManager);
        if (created)
        {
            OnRoomCreateSuccessDTO onRoomCreateSuccessDTO = new();
            onRoomCreateSuccessDTO.roomName = command.roomName;
            string onRoomCreateSuccessJson = Newtonsoft.Json.JsonConvert.SerializeObject(onRoomCreateSuccessDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnRoomCreateSuccess", onRoomCreateSuccessJson);

            player.GrantRoomMaster();
        }
    }
}
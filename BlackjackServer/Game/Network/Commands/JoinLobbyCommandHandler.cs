using Microsoft.AspNetCore.SignalR;

public class JoinLobbyCommandHandler : ICommandHandler<JoinLobbyDTO>
{
    private readonly IHubContext<BlackjackHub> _hubContext;
    private readonly UserManager _userManager;
    private readonly GameRoomManager _gameRoomManager;

    public JoinLobbyCommandHandler(IHubContext<BlackjackHub> hubContext, UserManager userManager, GameRoomManager gameRoomManager)
    {
        _hubContext = hubContext;
        _userManager = userManager;
        _gameRoomManager = gameRoomManager;
    }

    public async Task HandleAsync(JoinLobbyDTO command, CommandContext context)
    {
        var user = _userManager.GetUserByUserId(command.userId);
        if (user == null)
        {
            user = new User(command.userName, context.ConnectionId, command.userId);
            _userManager.AddUser(user);
        }
        else
        {
            user.SetUserConnectionId(context.ConnectionId);
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

        OnJoinLobbySuccessDTO onJoinLobbySuccessDTO = new();
        onJoinLobbySuccessDTO.playerGuid = player.Guid.ToString();
        onJoinLobbySuccessDTO.playerChips = player.Chips;
        string onJoinLobbySuccessJson = Newtonsoft.Json.JsonConvert.SerializeObject(onJoinLobbySuccessDTO);
        await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnJoinLobbySuccess", onJoinLobbySuccessJson);

        OnFullExistRoomListDTO onFullExistRoomListDTO = new();
        List<RoomInfoDTO> listRoomInfoDTO = new();
        foreach (var room in _gameRoomManager.GetAllRooms())
        {
            RoomInfoDTO roomInfoDTO = new();
            roomInfoDTO.roomName = room.RoomId;
            listRoomInfoDTO.Add(roomInfoDTO);
        }
        onFullExistRoomListDTO.rooms = listRoomInfoDTO;
        string onFullExistRoomListJson = Newtonsoft.Json.JsonConvert.SerializeObject(onFullExistRoomListDTO);
        await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnFullExistRoomList", onFullExistRoomListJson);
    }
}

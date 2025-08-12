using Microsoft.AspNetCore.SignalR;

public class JoinRoomCommandHandler : ICommandHandler<JoinRoomDTO>
{
    private readonly IHubContext<BlackjackHub> _hubContext;
    private readonly UserManager _userManager;
    private readonly GameRoomManager _gameRoomManager;

    public JoinRoomCommandHandler(IHubContext<BlackjackHub> hubContext, UserManager userManager, GameRoomManager gameRoomManager)
    {
        _hubContext = hubContext;
        _userManager = userManager;
        _gameRoomManager = gameRoomManager;
    }

    public async Task HandleAsync(JoinRoomDTO command, CommandContext context)
    {
        var user = _userManager.GetUserByUserId(command.userId);
        if (user == null)
        {
            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "플레이어를 찾을 수 없습니다.";
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

        _gameRoomManager.AddPlayerToRoom(command.roomName, player);
        await _hubContext.Groups.AddToGroupAsync(context.ConnectionId, command.roomName);

        var room = _gameRoomManager.GetRoomByRoomId(command.roomName);
        if (room == null)
        {
            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "게임 방을 찾을 수 없습니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        OnExistingPlayerListDTO onExistingPlayerListDTO = new();
        List<PlayerInfoDTO> listPlayerInfo = new();
        foreach (var item in room.PlayersInRoom)
        {
            if (item.Id == player.Id)
            {
                continue;
            }

            PlayerInfoDTO playerInfoDTO = new();
            playerInfoDTO.playerGuid = item.Guid.ToString();
            playerInfoDTO.userName = item.DisplayName;
            listPlayerInfo.Add(playerInfoDTO);
        }
        onExistingPlayerListDTO.players = listPlayerInfo;
        string onExistingPlayerListJson = Newtonsoft.Json.JsonConvert.SerializeObject(onExistingPlayerListDTO);
        await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnExistingPlayerList", onExistingPlayerListJson);

        OnJoinSuccessDTO onJoinSuccessDTO = new();
        onJoinSuccessDTO.userName = user.Name;
        onJoinSuccessDTO.playerGuid = player.Guid.ToString();
        string onJoinSuccessJson = Newtonsoft.Json.JsonConvert.SerializeObject(onJoinSuccessDTO);
        await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnJoinSuccess", onJoinSuccessJson);

        if (player.IsRoomMaster)
        {
            OnGrantRoomMasterDTO onGrantRoomMasterDTO = new();
            string onGrantRoomMasterJson = Newtonsoft.Json.JsonConvert.SerializeObject(onGrantRoomMasterDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnGrantRoomMaster", onGrantRoomMasterJson);
        }

        OnUserJoinedDTO onUserJoinedDTO = new();
        onUserJoinedDTO.userName = user.Name;
        onUserJoinedDTO.playerGuid = player.Guid.ToString();
        string onUserJoinedJson = Newtonsoft.Json.JsonConvert.SerializeObject(onUserJoinedDTO);
        await _hubContext.Clients.AllExcept(context.ConnectionId).SendAsync("ReceiveCommand", "OnUserJoined", onUserJoinedJson);

        OnPlayerRemainChipsDTO onPlayerRemainChipsDTO = new();
        onPlayerRemainChipsDTO.playerGuid = player.Guid.ToString();
        onPlayerRemainChipsDTO.chips = player.Chips.ToString();
        string onPlayerRemainChipsJson = Newtonsoft.Json.JsonConvert.SerializeObject(onPlayerRemainChipsDTO);
        await _hubContext.Clients.Group(room.RoomId).SendAsync("ReceiveCommand", "OnPlayerRemainChips", onPlayerRemainChipsJson);

        foreach (var item in room.PlayersInRoom)
        {
            if (item.Id == player.Id)
            {
                continue;
            }

            OnPlayerRemainChipsDTO onPlayerRemainChipsDTOOther = new();
            onPlayerRemainChipsDTOOther.playerGuid = item.Guid.ToString();
            onPlayerRemainChipsDTOOther.chips = item.Chips.ToString();
            string onPlayerRemainChipsJsonOther = Newtonsoft.Json.JsonConvert.SerializeObject(onPlayerRemainChipsDTOOther);
            await _hubContext.Clients.Group(room.RoomId).SendAsync("ReceiveCommand", "OnPlayerRemainChips", onPlayerRemainChipsJsonOther);
        }

        Console.WriteLine($"{user.Name} joined with connection ID: {context.ConnectionId}");
    }
}
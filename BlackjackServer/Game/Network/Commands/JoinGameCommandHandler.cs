using Microsoft.AspNetCore.Components.Endpoints;
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

        bool created = _gameRoomManager.CreateRoom("defaultRoom", _hubContext, _userManager);

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

        var room = _gameRoomManager.GetRoomByPlayerId(player.Id);
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
        onJoinSuccessDTO.userName = command.userName;
        onJoinSuccessDTO.playerGuid = player.Guid.ToString();
        string onJoinSuccessJson = Newtonsoft.Json.JsonConvert.SerializeObject(onJoinSuccessDTO);
        await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnJoinSuccess", onJoinSuccessJson);

        OnUserJoinedDTO onUserJoinedDTO = new();
        onUserJoinedDTO.userName = command.userName;
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

        if (created)
        {
            player.GrantRoomMaster();

            OnGrantRoomMasterDTO onGrantRoomMasterDTO = new();
            string onGrantRoomMasterJson = Newtonsoft.Json.JsonConvert.SerializeObject(onGrantRoomMasterDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnGrantRoomMaster", onGrantRoomMasterJson);
        }

        Console.WriteLine($"{command.userName} joined with connection ID: {context.ConnectionId}");
    }
}
using Microsoft.AspNetCore.SignalR;

public class LeaveGameCommandHandler : ICommandHandler<LeaveGameDTO>
{
    private IHubContext<BlackjackHub> _hubContext;
    private readonly UserManager _userManager;
    private readonly GameRoomManager _gameRoomManager;

    public LeaveGameCommandHandler(IHubContext<BlackjackHub> hubContext, UserManager userManager, GameRoomManager gameRoomManager)
    {
        _hubContext = hubContext;
        _userManager = userManager;
        _gameRoomManager = gameRoomManager;
    }

    public async Task HandleAsync(LeaveGameDTO command, CommandContext context)
    {
        var user = _userManager.GetUserByConnectionId(context.ConnectionId);
        if (user == null)
        {
            Console.WriteLine("유저 데이터를 찾을 수 없습니다.");

            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "유저 데이터를 찾을 수 없습니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        var player = _userManager.GetPlayerByUserId(user.Id);
        if (player == null)
        {
            Console.WriteLine("플레이어를 찾을 수 없습니다.");

            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "플레이어를 찾을 수 없습니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        var room = _gameRoomManager.GetRoomByRoomId(command.roomId);
        if (room == null)
        {
            Console.WriteLine("게임 방을 찾을 수 없습니다.");

            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "게임 방을 찾을 수 없습니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        await _hubContext.Groups.RemoveFromGroupAsync(context.ConnectionId, command.roomId);

        player.RevokeRoomMaster();
        
        player.ClearHand();
        player.SetAllHandDoneReset();
        player.ResetPlayerReadyToResult();
        _gameRoomManager.RemovePlayerFromRoom(command.roomId, player);

        if (room.PlayersInRoom.Count == 0)
        {
            _gameRoomManager.RemoveRoom(command.roomId);
        }
        else
        {
            var p = room.PlayersInRoom.Values.FirstOrDefault(p => p.IsRoomMaster);
            User? u;
            if (p != null)
            {
                u = _userManager.GetUserByPlayerId(p.Id);
            }
            else
            {
                p = room.PlayersInRoom.Values.FirstOrDefault();
                p.GrantRoomMaster();
                u = _userManager.GetUserByPlayerId(p.Id);
            }

            if (u != null)
            {
                OnGrantRoomMasterDTO onGrantRoomMasterDTO = new();
                string onGrantRoomMasterJson = Newtonsoft.Json.JsonConvert.SerializeObject(onGrantRoomMasterDTO);
                await _hubContext.Clients.Client(u.ConnectionId).SendAsync("ReceiveCommand", "OnGrantRoomMaster", onGrantRoomMasterJson);
            }
        }

        UserLeftDTO userLeftDTO = new();
        userLeftDTO.playerGuid = player.Guid.ToString();
        string userLeftJson = Newtonsoft.Json.JsonConvert.SerializeObject(userLeftDTO);
        await _hubContext.Clients.Group(command.roomId).SendAsync("ReceiveCommand", "UserLeft", userLeftJson);
    }
}
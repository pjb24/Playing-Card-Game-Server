using Microsoft.AspNetCore.SignalR;

public class ReadyToNextRoundCommandHandler : ICommandHandler<ReadyToNextRoundDTO>
{
    private IHubContext<BlackjackHub> _hubContext;
    private readonly UserManager _userManager;
    private readonly GameRoomManager _gameRoomManager;

    public ReadyToNextRoundCommandHandler(IHubContext<BlackjackHub> hubContext, UserManager userManager, GameRoomManager gameRoomManager)
    {
        _hubContext = hubContext;
        _userManager = userManager;
        _gameRoomManager = gameRoomManager;
    }

    public async Task HandleAsync(ReadyToNextRoundDTO command, CommandContext context)
    {
        var user = _userManager.GetUserByConnectionId(context.ConnectionId);
        if (user == null)
        {
            Console.WriteLine("플레이어를 찾을 수 없습니다.");

            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "플레이어를 찾을 수 없습니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        var player = _userManager.GetPlayer(user.Id);
        if (player == null)
        {
            Console.WriteLine("플레이어를 찾을 수 없습니다.");

            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "플레이어를 찾을 수 없습니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        var room = _gameRoomManager.GetRoomByPlayerId(player.Id);
        if (room == null)
        {
            Console.WriteLine("게임 방을 찾을 수 없습니다.");

            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "게임 방을 찾을 수 없습니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        player.SetReadyToNextRound();

        if (room.PlayersInGame.All(p => p.IsReadyToNextRound))
        {
            var targetPlayer = room.PlayersInGame.FirstOrDefault(p => p.IsRoomMaster);
            var targetConnection = _userManager.GetUserByPlayerId(targetPlayer.Id);

            OnGrantRoomMasterDTO onGrantRoomMasterDTO = new();
            string onGrantRoomMasterJson = Newtonsoft.Json.JsonConvert.SerializeObject(onGrantRoomMasterDTO);
            await _hubContext.Clients.Client(targetConnection.ConnectionId).SendAsync("ReceiveCommand", "OnGrantRoomMaster", onGrantRoomMasterJson);

            foreach (var item in room.PlayersInGame)
            {
                item.ResetPlayerReadyToResult();
            }
        }
    }
}
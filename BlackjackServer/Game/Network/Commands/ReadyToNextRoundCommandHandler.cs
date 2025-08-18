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

        if (room.PlayersInGame.Values.All(p => p.IsReadyToNextRound))
        {
            var targetPlayer = room.PlayersInGame.Values.FirstOrDefault(p => p.IsRoomMaster);
            if (targetPlayer == null)
            {
                Console.WriteLine("방장 플레이어를 찾을 수 없습니다.");
                return;
            }
            var targetConnection = _userManager.GetUserByPlayerId(targetPlayer.Id);
            if (targetConnection == null)
            {
                Console.WriteLine("방장 유저를 찾을 수 없습니다.");
                return;
            }

            OnGrantRoomMasterDTO onGrantRoomMasterDTO = new();
            string onGrantRoomMasterJson = Newtonsoft.Json.JsonConvert.SerializeObject(onGrantRoomMasterDTO);
            await _hubContext.Clients.Client(targetConnection.ConnectionId).SendAsync("ReceiveCommand", "OnGrantRoomMaster", onGrantRoomMasterJson);

            foreach (var item in room.PlayersInRoom.Values)
            {
                if (item.Hands.Count == 0)
                {
                    PlayerHand hand = new();
                    item.AddHand(hand);

                    OnAddHandToPlayerDTO onAddHandToPlayerDTO = new();
                    onAddHandToPlayerDTO.playerGuid = item.Guid.ToString();
                    onAddHandToPlayerDTO.handId = hand.HandId.ToString();
                    string onAddHandToPlayerJson = Newtonsoft.Json.JsonConvert.SerializeObject(onAddHandToPlayerDTO);
                    _ = room.SendToAll("OnAddHandToPlayer", onAddHandToPlayerJson);
                }
            }
        }
    }
}
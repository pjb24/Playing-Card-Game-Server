using Microsoft.AspNetCore.SignalR;

public class DealerBehaviorDoneCommandHandler : ICommandHandler<DealerBehaviorDoneDTO>
{
    private IHubContext<BlackjackHub> _hubContext;
    private readonly UserManager _userManager;
    private readonly GameRoomManager _gameRoomManager;

    public DealerBehaviorDoneCommandHandler(IHubContext<BlackjackHub> hubContext, UserManager userManager, GameRoomManager gameRoomManager)
    {
        _hubContext = hubContext;
        _userManager = userManager;
        _gameRoomManager = gameRoomManager;
    }

    public async Task HandleAsync(DealerBehaviorDoneDTO command, CommandContext context)
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

        var room = _gameRoomManager.GetRoomByPlayerId(player.Id);
        if (room == null)
        {
            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "게임 방을 찾을 수 없습니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        room.SetPlayerReadyToResult(player);

        bool isReadyToResult = room.CheckAllPlayerReadyToResult();
        if (isReadyToResult)
        {
            room.ChangeState(new ResultState(room));
        }
    }
}
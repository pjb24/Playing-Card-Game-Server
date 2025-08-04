using Microsoft.AspNetCore.SignalR;

public class PlaceBetCommandHandler : ICommandHandler<PlaceBetDTO>
{
    private IHubContext<BlackjackHub> _hubContext;
    private readonly UserManager _userManager;
    private readonly GameRoomManager _gameRoomManager;

    public PlaceBetCommandHandler(IHubContext<BlackjackHub> hubContext, UserManager userManager, GameRoomManager gameRoomManager)
    {
        _hubContext = hubContext;
        _userManager = userManager;
        _gameRoomManager = gameRoomManager;
    }

    public async Task HandleAsync(PlaceBetDTO command, CommandContext context)
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

        var guid = Guid.TryParse(command.handId, out var parsedGuid) ? parsedGuid : Guid.Empty;
        if (guid == Guid.Empty)
        {
            Console.WriteLine("유효하지 않은 핸드 ID입니다.");

            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "유효하지 않은 핸드 ID입니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        var currentHand = player.GetHandById(guid);
        if (currentHand == null)
        {
            Console.WriteLine("해당 핸드를 찾을 수 없습니다.");

            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "해당 핸드를 찾을 수 없습니다.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
            return;
        }

        bool success = room.PlaceBet(player, command.amount, currentHand);
        if (success)
        {
            OnBetPlacedDTO onBetPlacedDTO = new();
            onBetPlacedDTO.playerGuid = player.Guid.ToString();
            onBetPlacedDTO.playerName = player.DisplayName;
            onBetPlacedDTO.betAmount = command.amount;
            onBetPlacedDTO.handId = currentHand.HandId.ToString();
            string onBetPlacedJson = Newtonsoft.Json.JsonConvert.SerializeObject(onBetPlacedDTO);
            await _hubContext.Clients.Group(room.RoomId).SendAsync("ReceiveCommand", "OnBetPlaced", onBetPlacedJson);

            OnPlayerRemainChipsDTO onPlayerRemainChipsDTO = new();
            onPlayerRemainChipsDTO.playerGuid = player.Guid.ToString();
            onPlayerRemainChipsDTO.chips = player.Chips.ToString();
            string onPlayerRemainChipsJson = Newtonsoft.Json.JsonConvert.SerializeObject(onPlayerRemainChipsDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnPlayerRemainChips", onPlayerRemainChipsJson);
        }
        else
        {
            Console.WriteLine("베팅에 실패했습니다. 충분한 금액이 있는지 확인하세요.");

            OnErrorDTO onErrorDTO = new();
            onErrorDTO.message = "베팅에 실패했습니다. 충분한 금액이 있는지 확인하세요.";
            string onErrorJson = Newtonsoft.Json.JsonConvert.SerializeObject(onErrorDTO);
            await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnError", onErrorJson);
        }
    }
}
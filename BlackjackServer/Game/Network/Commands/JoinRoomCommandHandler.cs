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

        // 플레이어를 게임 방에 추가
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

        // 방에 있는 플레이어 목록을 클라이언트에 전송
        OnExistingPlayerListDTO onExistingPlayerListDTO = new();
        List<PlayerInfoDTO> listPlayerInfo = new();
        foreach (var item in room.PlayersInRoom.Values)
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
        _ = room.SendToPlayer(player, "OnExistingPlayerList", onExistingPlayerListJson);

        // 방에 있는 플레이어의 핸드들을 클라이언트에 전송
        foreach (var item in room.PlayersInRoom.Values)
        {
            if (item.Id == player.Id)
            {
                continue;
            }

            foreach (var itemHand in item.Hands)
            {
                OnAddHandToPlayerDTO onAddHandToPlayerDTOItem = new();
                onAddHandToPlayerDTOItem.playerGuid = item.Guid.ToString();
                onAddHandToPlayerDTOItem.handId = itemHand.HandId.ToString();
                string onAddHandToPlayerJsonItem = Newtonsoft.Json.JsonConvert.SerializeObject(onAddHandToPlayerDTOItem);
                _ = room.SendToPlayer(player, "OnAddHandToPlayer", onAddHandToPlayerJsonItem);

                foreach (var card in itemHand.Cards)
                {
                    OnAddCardToHandDTO onAddCardToHandDTO = new();
                    onAddCardToHandDTO.playerGuid = item.Guid.ToString();
                    onAddCardToHandDTO.handId = itemHand.HandId.ToString();
                    onAddCardToHandDTO.cardRank = card.GetRank();
                    onAddCardToHandDTO.cardSuit = card.GetSuit();
                    string onAddCardToHandJson = Newtonsoft.Json.JsonConvert.SerializeObject(onAddCardToHandDTO);
                    _ = room.SendToPlayer(player, "OnAddCardToHand", onAddCardToHandJson);
                }
            }

            int index = 0;
            foreach (var card in room.Dealer.Hand.Cards)
            {
                OnAddCardToDealerHandDTO onAddCardToDealerHandDTO = new();
                onAddCardToDealerHandDTO.cardRank = card.GetRank();
                onAddCardToDealerHandDTO.cardSuit = card.GetSuit();

                if (index == 1 && !room.Dealer.IsHoleCardRevealed)
                {
                    onAddCardToDealerHandDTO.cardRank = E_CardRank.Back;
                    onAddCardToDealerHandDTO.cardSuit = E_CardSuit.Back;
                }
                string onAddCardToDealerHandJson = Newtonsoft.Json.JsonConvert.SerializeObject(onAddCardToDealerHandDTO);
                _ = room.SendToPlayer(player, "OnAddCardToDealerHand", onAddCardToDealerHandJson);

                index++;
            }
        }

        // 플레이어가 방에 성공적으로 들어갔음을 알리는 메시지를 클라이언트에 전송
        OnJoinSuccessDTO onJoinSuccessDTO = new();
        onJoinSuccessDTO.userName = user.Name;
        onJoinSuccessDTO.playerGuid = player.Guid.ToString();
        string onJoinSuccessJson = Newtonsoft.Json.JsonConvert.SerializeObject(onJoinSuccessDTO);
        _ = room.SendToPlayer(player, "OnJoinSuccess", onJoinSuccessJson);

        // 방에 있는 플레이어에게 새로운 플레이어가 들어왔음을 알림
        OnUserJoinedDTO onUserJoinedDTO = new();
        onUserJoinedDTO.userName = user.Name;
        onUserJoinedDTO.playerGuid = player.Guid.ToString();
        string onUserJoinedJson = Newtonsoft.Json.JsonConvert.SerializeObject(onUserJoinedDTO);
        _ = room.SendToAllExceptPlayer("OnUserJoined", onUserJoinedJson, player);

        // 플레이어가 방에 들어갈 때 핸드를 추가하고, 관련 정보를 모든 클라이언트에 전송
        PlayerHand hand = new();
        player.AddHand(hand);

        OnAddHandToPlayerDTO onAddHandToPlayerDTO = new();
        onAddHandToPlayerDTO.playerGuid = player.Guid.ToString();
        onAddHandToPlayerDTO.handId = hand.HandId.ToString();
        string onAddHandToPlayerJson = Newtonsoft.Json.JsonConvert.SerializeObject(onAddHandToPlayerDTO);
        _ = room.SendToAll("OnAddHandToPlayer", onAddHandToPlayerJson);

        if (player.IsRoomMaster)
        {
            OnGrantRoomMasterDTO onGrantRoomMasterDTO = new();
            string onGrantRoomMasterJson = Newtonsoft.Json.JsonConvert.SerializeObject(onGrantRoomMasterDTO);
            _ = room.SendToPlayer(player, "OnGrantRoomMaster", onGrantRoomMasterJson);
        }

        OnPlayerRemainChipsDTO onPlayerRemainChipsDTO = new();
        onPlayerRemainChipsDTO.playerGuid = player.Guid.ToString();
        onPlayerRemainChipsDTO.chips = player.Chips;
        string onPlayerRemainChipsJson = Newtonsoft.Json.JsonConvert.SerializeObject(onPlayerRemainChipsDTO);
        _ = room.SendToAll("OnPlayerRemainChips", onPlayerRemainChipsJson);

        foreach (var item in room.PlayersInRoom.Values)
        {
            if (item.Id == player.Id)
            {
                continue;
            }

            OnPlayerRemainChipsDTO onPlayerRemainChipsDTOOther = new();
            onPlayerRemainChipsDTOOther.playerGuid = item.Guid.ToString();
            onPlayerRemainChipsDTOOther.chips = item.Chips;
            string onPlayerRemainChipsJsonOther = Newtonsoft.Json.JsonConvert.SerializeObject(onPlayerRemainChipsDTOOther);
            _ = room.SendToPlayer(player, "OnPlayerRemainChips", onPlayerRemainChipsJsonOther);
        }

        Console.WriteLine($"{user.Name} joined with connection ID: {context.ConnectionId}");
    }
}
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

public class GameRoom
{
    private readonly IHubContext<BlackjackHub> _hubContext;

    public string RoomId { get; }
    private CustomOrderedDictionary<string, Player> _playersInRoom = new();
    public IReadOnlyDictionary<string, Player> PlayersInRoom => _playersInRoom.AsReadOnly();
    private CustomOrderedDictionary<string, Player> _playersInGame = new();
    public IReadOnlyDictionary<string, Player> PlayersInGame => _playersInGame.AsReadOnly();
    private Dealer _dealer = new();
    public Dealer Dealer => _dealer;

    private GameStateMachine _fsm;
    private Deck _deck;
    public Deck Deck => _deck;

    public string CurrentState => _fsm.CurrentState != null ? _fsm.CurrentState.GetType().ToString() : "";

    private UserManager _userManager;

    public GameRoom(string roomId, IHubContext<BlackjackHub> hubContext, UserManager userManager)
    {
        _userManager = userManager;

        _hubContext = hubContext;

        RoomId = roomId;

        _fsm = new();
        _deck = new();
    }

    public async Task SendToPlayer(Player player, string methodName, string json)
    {
        try
        {
            var user = _userManager.GetUserByPlayerId(player.Id);
            if (user == null)
            {
                Console.WriteLine($"User with ID {player.Id} not found.");
                return;
            }

            await _hubContext.Clients.Client(user.ConnectionId).SendAsync("ReceiveCommand", methodName, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to player {player.Id}: {ex.Message}");
        }
    }

    public async Task SendToAll(string methodName, string json)
    {
        try
        {
            await _hubContext.Clients.Group(RoomId).SendAsync("ReceiveCommand", methodName, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to all players in room {RoomId}: {ex.Message}");
        }
    }

    public async Task SendToAllExcept(string methodName, string json, string excludedConnectionId)
    {
        try
        {
            await _hubContext.Clients.GroupExcept(RoomId, excludedConnectionId).SendAsync("ReceiveCommand", methodName, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to all players in room {RoomId} except {excludedConnectionId}: {ex.Message}");
        }
    }

    public async Task SendToAllExceptPlayer(string methodName, string json, Player player)
    {
        try
        {
            var user = _userManager.GetUserByPlayerId(player.Id);
            if (user == null)
            {
                Console.WriteLine($"User with ID {player.Id} not found.");
                return;
            }

            await _hubContext.Clients.GroupExcept(RoomId, user.ConnectionId).SendAsync("ReceiveCommand", methodName, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to all players in room {RoomId} except {player.Id}: {ex.Message}");
        }
    }

    public void AddPlayerToRoom(Player player)
    {
        if (!_playersInRoom.ContainsKey(player.Id))
        {
            _playersInRoom.TryAdd(player.Id, player);
        }
    }

    public void RemovePlayerFromRoom(Player player)
    {
        if (_playersInRoom.ContainsKey(player.Id))
        {
            _playersInRoom.Remove(player.Id, out _);
        }

        if (_playersInGame.ContainsKey(player.Id))
        {
            _playersInGame.Remove(player.Id, out _);
        }
    }

    public void AddPlayerToGame(Player player)
    {
        if (!_playersInGame.ContainsKey(player.Id))
        {
            _playersInGame.TryAdd(player.Id, player);
        }
    }

    public void RemovePlayerFromGame(Player player)
    {
        if (_playersInGame.ContainsKey(player.Id))
        {
            _playersInGame.Remove(player.Id, out _);
        }
    }

    public void ChangeState(IGameState state)
    {
        _fsm.ChangeState(state);
    }

    public Player? GetNextBettingPlayer()
    {
        return _playersInGame.Values.FirstOrDefault(player => !player.IsAllHandBettingDone);
    }

    public Player? GetNextActionPlayer()
    {
        return _playersInGame.Values.FirstOrDefault(player => !player.IsAllHandActionDone);
    }

    public bool CheckAllPlayerBettingDone()
    {
        return _playersInGame.Values.All(p => p.IsAllHandBettingDone == true);
    }

    public void StartGame()
    {
        _fsm.ChangeState(new GameStartState(this));
    }

    public bool PlaceBet(Player player, int amount, PlayerHand currentHand)
    {
        if (_fsm.CurrentState == null)
        {
            return false;
        }
        if (_fsm.CurrentState.GetType() != typeof(BettingState))
        {
            return false; // Cannot place bet if not in betting state
        }

        if (player.Chips < amount)
        {
            return false; // Not enough chips to place bet
        }

        player.PlaceBet(amount, currentHand);

        return true;
    }

    public bool Hit(Player player, PlayerHand hand)
    {
        if (_fsm.CurrentState == null)
        {
            return false;
        }
        if (_fsm.CurrentState.GetType() != typeof(PlayerTurnState))
        {
            return false;
        }

        var card = _deck.DrawCard();
        hand.AddCard(card);

        OnCardDealtDTO onCardDealtDTO = new();
        onCardDealtDTO.playerGuid = player.Guid.ToString();
        onCardDealtDTO.playerName = player.DisplayName;
        onCardDealtDTO.cardRank = card.GetRank();
        onCardDealtDTO.cardSuit = card.GetSuit();
        onCardDealtDTO.handId = hand.HandId.ToString();
        string onCardDealtJson = Newtonsoft.Json.JsonConvert.SerializeObject(onCardDealtDTO);
        _ = SendToAll("OnCardDealt", onCardDealtJson);

        if (hand.IsBust())
        {
            OnPlayerBustedDTO onPlayerBustedDTO = new();
            onPlayerBustedDTO.playerGuid = player.Guid.ToString();
            onPlayerBustedDTO.playerName = player.DisplayName;
            onPlayerBustedDTO.handId = hand.HandId.ToString();
            string onPlayerBustedJson = Newtonsoft.Json.JsonConvert.SerializeObject(onPlayerBustedDTO);
            _ = SendToAll("OnPlayerBusted", onPlayerBustedJson);

            hand.SetActionDone();

            OnActionDoneDTO onActionDoneDTO = new();
            onActionDoneDTO.playerGuid = player.Guid.ToString();
            onActionDoneDTO.playerName = player.DisplayName;
            onActionDoneDTO.handId = hand.HandId.ToString();
            string onActionDoneJson = Newtonsoft.Json.JsonConvert.SerializeObject(onActionDoneDTO);
            _ = SendToAll("OnActionDone", onActionDoneJson);
        }

        ChangeState(new PlayerTurnState(this));

        return true;
    }

    public bool Stand(Player player, PlayerHand hand)
    {
        if (_fsm.CurrentState == null)
        {
            return false;
        }
        if (_fsm.CurrentState.GetType() != typeof(PlayerTurnState))
        {
            return false;
        }

        hand.SetActionDone();

        OnActionDoneDTO onActionDoneDTO = new();
        onActionDoneDTO.playerGuid = player.Guid.ToString();
        onActionDoneDTO.playerName = player.DisplayName;
        onActionDoneDTO.handId = hand.HandId.ToString();
        string onActionDoneJson = Newtonsoft.Json.JsonConvert.SerializeObject(onActionDoneDTO);
        _ = SendToAll("OnActionDone", onActionDoneJson);

        ChangeState(new PlayerTurnState(this));

        return true;
    }

    public bool Split(Player player, PlayerHand hand)
    {
        if (_fsm.CurrentState == null)
        {
            return false;
        }
        if (_fsm.CurrentState.GetType() != typeof(PlayerTurnState))
        {
            return false;
        }

        if (!hand.CanSplit())
        {
            //return false;
        }

        if (player.Chips < hand.BetAmount)
        {
            return false;
        }

        // 새로운 핸드를 현재 핸드의 다음 index로 insert
        var index = player.IndexOfHand(hand);
        PlayerHand newHand = player.InsertHand(index + 1);

        OnHandSplitDTO onHandSplitDTO = new();
        onHandSplitDTO.playerGuid = player.Guid.ToString();
        onHandSplitDTO.playerName = player.DisplayName;
        onHandSplitDTO.handId = hand.HandId.ToString();
        onHandSplitDTO.newHandId = newHand.HandId.ToString();
        string onHandSplitJson = Newtonsoft.Json.JsonConvert.SerializeObject(onHandSplitDTO);
        _ = SendToAll("OnHandSplit", onHandSplitJson);

        // 새로운 핸드에 베팅 입력
        player.PlaceBet(hand.BetAmount, newHand);

        OnBetPlacedDTO onBetPlacedDTO = new();
        onBetPlacedDTO.playerGuid = player.Guid.ToString();
        onBetPlacedDTO.playerName = player.DisplayName;
        onBetPlacedDTO.betAmount = hand.BetAmount;
        onBetPlacedDTO.handId = newHand.HandId.ToString();
        string onBetPlacedJson = Newtonsoft.Json.JsonConvert.SerializeObject(onBetPlacedDTO);
        _ = SendToAll("OnBetPlaced", onBetPlacedJson);

        OnPlayerRemainChipsDTO onPlayerRemainChipsDTO = new();
        onPlayerRemainChipsDTO.playerGuid = player.Guid.ToString();
        onPlayerRemainChipsDTO.chips = player.Chips;
        string onPlayerRemainChipsJson = Newtonsoft.Json.JsonConvert.SerializeObject(onPlayerRemainChipsDTO);
        _ = SendToAll("OnPlayerRemainChips", onPlayerRemainChipsJson);

        // 현재 핸드의 2번째 카드를 새로운 핸드로 나눔
        var Cards = hand.Cards;
        var splitCard = Cards.ElementAt(1);
        hand.RemoveCard(splitCard);
        newHand.AddCard(splitCard);

        // PlayerTurnState 다시 진입
        ChangeState(new PlayerTurnState(this));

        return true;
    }

    public bool DoubleDown(Player player, PlayerHand hand)
    {
        if (_fsm.CurrentState == null)
        {
            return false;
        }
        if (_fsm.CurrentState.GetType() != typeof(PlayerTurnState))
        {
            return false;
        }

        if (!hand.CanDoubleDown())
        {
            return false;
        }

        if (player.Chips < hand.BetAmount)
        {
            return false;
        }

        // Increase bet
        player.DoubleDown(hand);

        OnBetPlacedDTO onBetPlacedDTO = new();
        onBetPlacedDTO.playerGuid = player.Guid.ToString();
        onBetPlacedDTO.playerName = player.DisplayName;
        onBetPlacedDTO.betAmount = hand.BetAmount;
        onBetPlacedDTO.handId = hand.HandId.ToString();
        string onBetPlacedJson = Newtonsoft.Json.JsonConvert.SerializeObject(onBetPlacedDTO);
        _ = SendToAll("OnBetPlaced", onBetPlacedJson);

        // Draw card
        var card = _deck.DrawCard();
        hand.AddCard(card);

        OnCardDealtDTO onCardDealtDTO = new();
        onCardDealtDTO.playerGuid = player.Guid.ToString();
        onCardDealtDTO.playerName = player.DisplayName;
        onCardDealtDTO.cardRank = card.GetRank();
        onCardDealtDTO.cardSuit = card.GetSuit();
        onCardDealtDTO.handId = hand.HandId.ToString();
        string onCardDealtJson = Newtonsoft.Json.JsonConvert.SerializeObject(onCardDealtDTO);
        _ = SendToAll("OnCardDealt", onCardDealtJson);

        // Check bust
        if (hand.IsBust())
        {
            OnPlayerBustedDTO onPlayerBustedDTO = new();
            onPlayerBustedDTO.playerGuid = player.Guid.ToString();
            onPlayerBustedDTO.playerName = player.DisplayName;
            onPlayerBustedDTO.handId = hand.HandId.ToString();
            string onPlayerBustedJson = Newtonsoft.Json.JsonConvert.SerializeObject(onPlayerBustedDTO);
            _ = SendToAll("OnPlayerBusted", onPlayerBustedJson);
        }

        // Stand hand
        hand.SetActionDone();

        OnActionDoneDTO onActionDoneDTO = new();
        onActionDoneDTO.playerGuid = player.Guid.ToString();
        onActionDoneDTO.playerName = player.DisplayName;
        onActionDoneDTO.handId = hand.HandId.ToString();
        string onActionDoneJson = Newtonsoft.Json.JsonConvert.SerializeObject(onActionDoneDTO);
        _ = SendToAll("OnActionDone", onActionDoneJson);

        ChangeState(new PlayerTurnState(this));

        return true;
    }

    public void RevealHoleCard()
    {
        if (_fsm.CurrentState == null)
        {
            return;
        }
        if (_fsm.CurrentState.GetType() != typeof(DealerTurnState))
        {
            return;
        }

        var dealerHiddenCard = _dealer.GetHiddenCard();
        if (dealerHiddenCard == null)
        {

        }
        else
        {
            _dealer.SetHoleCardRevealed();

            OnDealerHoleCardRevealedDTO onDealerHoleCardRevealedDTO = new();
            onDealerHoleCardRevealedDTO.cardRank = dealerHiddenCard.GetRank();
            onDealerHoleCardRevealedDTO.cardSuit = dealerHiddenCard.GetSuit();
            string onDealerHoleCardRevealedJson = Newtonsoft.Json.JsonConvert.SerializeObject(onDealerHoleCardRevealedDTO);
            _ = SendToAll("OnDealerHoleCardRevealed", onDealerHoleCardRevealedJson);
        }
    }

    public void SetPlayerReadyToResult(Player player)
    {
        player.SetPlayerReadyToResult();
    }

    public bool CheckAllPlayerReadyToResult()
    {
        return PlayersInGame.Values.All(p => p.IsReadyToResult == true);
    }
}
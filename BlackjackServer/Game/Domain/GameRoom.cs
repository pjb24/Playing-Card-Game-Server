using Microsoft.AspNetCore.SignalR;

public class GameRoom
{
    private readonly IHubContext<BlackjackHub> _hubContext;

    public string RoomId { get; }
    private List<Player> _playersInRoom = new();
    public IReadOnlyList<Player> PlayersInRoom => _playersInRoom.AsReadOnly();
    private List<Player> _playersInGame = new();
    public IReadOnlyList<Player> PlayersInGame => _playersInGame.AsReadOnly();
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

    public async Task SendToPlayer(Player player, string methodName, object args)
    {
        try
        {
            var user = _userManager.GetUserByPlayerId(player.Id);
            if (user == null)
            {
                Console.WriteLine($"User with ID {player.Id} not found.");
                return;
            }

            await _hubContext.Clients.Client(user.ConnectionId).SendAsync(methodName, args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to player {player.Id}: {ex.Message}");
        }
    }

    public async Task SendToAll(string methodName, object args)
    {
        try
        {
            await _hubContext.Clients.Group(RoomId).SendAsync(methodName, args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to all players in room {RoomId}: {ex.Message}");
        }
    }

    public void AddPlayerToRoom(Player player)
    {
        if (!_playersInRoom.Contains(player))
        {
            _playersInRoom.Add(player);
        }
    }

    public void RemovePlayerFromRoom(Player player)
    {
        if (_playersInRoom.Contains(player))
        {
            _playersInRoom.Remove(player);
        }
    }

    public void AddPlayerToGame(Player player)
    {
        if (!_playersInGame.Contains(player))
        {
            _playersInGame.Add(player);
        }
    }

    public void RemovePlayerFromGame(Player player)
    {
        if (_playersInGame.Contains(player))
        {
            _playersInGame.Remove(player);
        }
    }

    public void ChangeState(IGameState state)
    {
        _fsm.ChangeState(state);
    }

    public Player? GetNextBettingPlayer()
    {
        return _playersInGame.FirstOrDefault(player => !player.IsAllHandBettingDone);
    }

    public Player? GetNextActionPlayer()
    {
        return _playersInGame.FirstOrDefault(player => !player.IsAllHandActionDone);
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

        _fsm.ChangeState(new BettingState(this));

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
        _ = SendToAll("OnCardDealt", new
        {
            playerGuid = player.Guid.ToString(),
            playerName = player.DisplayName,
            cardString = card.ToString(),
            handId = hand.HandId.ToString()
        });

        if (hand.IsBust())
        {
            _ = SendToAll("OnPlayerBusted", new
            {
                playerGuid = player.Guid.ToString(),
                playerName = player.DisplayName,
                handId = hand.HandId.ToString()
            });
            hand.SetActionDone();
            _ = SendToAll("OnActionDone", new
            {
                playerGuid = player.Guid.ToString(),
                playerName = player.DisplayName,
                handId = hand.HandId.ToString()
            });
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
        _ = SendToAll("OnActionDone", new
        {
            playerGuid = player.Guid.ToString(),
            playerName = player.DisplayName,
            handId = hand.HandId.ToString()
        });


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
            return false;
        }

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
            _ = SendToAll("OnDealerHoleCardRevealed", new
            {
                card = dealerHiddenCard.ToString()
            });
        }
    }
}
public enum GameState
{
    GameStart,
    Betting,
    Dealing,
    PlayerTurn,
    DealerTurn,
    Evaluating,
    GameEnd
}

public class GameRoom
{
    public string RoomId { get; }
    private List<Player> playersInRoom = new List<Player>();
    public IReadOnlyList<Player> PlayersInRoom => playersInRoom.AsReadOnly();
    private List<Player> playersInGame = new List<Player>();
    public IReadOnlyList<Player> PlayersInGame => playersInGame.AsReadOnly();
    private GameState state = GameState.GameStart;
    public GameState State => state;

    public GameRoom(string roomId)
    {
        RoomId = roomId;
    }

    public void AddPlayerToRoom(Player player)
    {
        if (!playersInRoom.Contains(player))
        {
            playersInRoom.Add(player);
        }
    }

    public void RemovePlayerFromRoom(Player player)
    {
        if (playersInRoom.Contains(player))
        {
            playersInRoom.Remove(player);
        }
    }

    public void UpdateGameState(GameState newState)
    {
        state = newState;
    }

    public void AddPlayerToGame(Player player)
    {
        if (!playersInGame.Contains(player))
        {
            playersInGame.Add(player);
        }
    }

    public void RemovePlayerFromGame(Player player)
    {
        if (playersInGame.Contains(player))
        {
            playersInGame.Remove(player);
        }
    }

    public bool PlaceBet(Player player, int amount)
    {
        if (state != GameState.Betting)
        {
            return false; // Cannot place bet if not in betting state
        }

        player.BetAmount += amount;

        return true;
    }
}

public class GameRoomManager
{
    private readonly Dictionary<string, GameRoom> _gameRooms = new();

    public void CreateRoom(string roomId)
    {
        if (!_gameRooms.ContainsKey(roomId))
        {
            _gameRooms[roomId] = new GameRoom(roomId);
        }
    }

    public void AddPlayerToRoom(string roomId, Player player)
    {
        if (_gameRooms.TryGetValue(roomId, out var room))
        {
            room.AddPlayerToRoom(player);
        }
    }

    public void RemovePlayerFromRoom(string roomId, Player player)
    {
        if (_gameRooms.TryGetValue(roomId, out var room))
        {
            room.RemovePlayerFromRoom(player);
        }
    }

    public GameRoom? GetRoom(string roomId)
    {
        return _gameRooms.TryGetValue(roomId, out var room) ? room : null;
    }

    public IEnumerable<GameRoom> GetAllRooms()
    {
        return _gameRooms.Values;
    }

    public GameRoom? GetRoomByPlayerConnectionId(string connectionId)
    {
        return _gameRooms.Values.FirstOrDefault(room => room.PlayersInRoom.Any(p => p.ConnectionId == connectionId));
    }
}
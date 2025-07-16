public class Player
{
    public string Name { get; set; }
    public string ConnectionId { get; set; }

    public Player(string name, string connectionId)
    {
        Name = name;
        ConnectionId = connectionId;
    }
}

public class PlayerManager
{
    private readonly Dictionary<string, Player> _players = new();

    public void AddPlayer(Player player)
    {
        if (!_players.ContainsKey(player.ConnectionId))
        {
            _players[player.ConnectionId] = player;
        }
    }

    public void RemovePlayer(string connectionId)
    {
        if (_players.ContainsKey(connectionId))
        {
            _players.Remove(connectionId);
        }
    }

    public Player? GetPlayer(string connectionId)
    {
        return _players.TryGetValue(connectionId, out var player) ? player : null;
    }

    public IEnumerable<Player> GetAllPlayers()
    {
        return _players.Values;
    }
}
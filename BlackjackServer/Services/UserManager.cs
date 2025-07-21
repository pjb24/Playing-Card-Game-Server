public class UserManager
{
    private readonly Dictionary<string, User> _users = new();
    private readonly Dictionary<string, Player> _players = new();

    private int _initialChips = 10_000_000;

    public void AddUser(User user)
    {
        if (!_users.ContainsKey(user.ConnectionId))
        {
            _users[user.ConnectionId] = user;
        }

        if (!_players.ContainsKey(user.Id))
        {
            _players[user.Id] = new Player(user.Id, user.Name, _initialChips);
        }
    }

    public void RemoveUser(string connectionId)
    {
        if (_users.ContainsKey(connectionId))
        {
            _users.Remove(connectionId);
        }
    }

    public User? GetUserByConnectionId(string connectionId)
    {
        return _users.TryGetValue(connectionId, out var user) ? user : null;
    }

    public User? GetUserByPlayerId(string playerId)
    {
        return _users.Values.FirstOrDefault(user => user.Id == playerId);
    }

    public IEnumerable<User> GetAllUsers()
    {
        return _users.Values;
    }

    public Player? GetPlayer(string id)
    {
        return _players.TryGetValue(id, out var player) ? player : null;
    }
}
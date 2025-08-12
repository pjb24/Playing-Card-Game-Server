public class UserManager
{
    private readonly Dictionary<string, User> _users = new();
    private readonly Dictionary<string, Player> _players = new();

    private int _initialChips = 10_000_000;

    public void AddUser(User user)
    {
        if (!_users.ContainsKey(user.Id))
        {
            _users[user.Id] = user;
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

    public User? GetUserByUserId(string userId)
    {
        return _users.TryGetValue(userId, out var user) ? user : null;
    }

    public User? GetUserByConnectionId(string connectionId)
    {
        return _users.Values.FirstOrDefault(user => user.ConnectionId == connectionId);
    }

    public User? GetUserByPlayerId(string playerId)
    {
        return _users.Values.FirstOrDefault(user => user.Id == playerId);
    }

    public IEnumerable<User> GetAllUsers()
    {
        return _users.Values;
    }

    public Player? GetPlayerByUserId(string userId)
    {
        return _players.TryGetValue(userId, out var player) ? player : null;
    }
}
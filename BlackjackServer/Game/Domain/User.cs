public class User
{
    private string _name;
    public string Name => _name;

    private string _connectionId;
    public string ConnectionId => _connectionId;

    private string _id;
    public string Id => _id;

    public User(string name, string connectionId, string id)
    {
        _name = name;
        _connectionId = connectionId;
        _id = id;
    }
}
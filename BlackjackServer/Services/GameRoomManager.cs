using Microsoft.AspNetCore.SignalR;

public class GameRoomManager
{
    private readonly Dictionary<string, GameRoom> _gameRooms = new();

    public bool CreateRoom(string roomId, IHubContext<BlackjackHub> hubContext, UserManager userManager)
    {
        if (!_gameRooms.ContainsKey(roomId))
        {
            _gameRooms[roomId] = new GameRoom(roomId, hubContext, userManager);
            return true;
        }
        else
        {
            return false;
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

    public GameRoom? GetRoomByRoomId(string roomId)
    {
        return _gameRooms.TryGetValue(roomId, out var room) ? room : null;
    }

    public IEnumerable<GameRoom> GetAllRooms()
    {
        return _gameRooms.Values;
    }

    public GameRoom? GetRoomByPlayerId(string id)
    {
        return _gameRooms.Values.FirstOrDefault(room => room.PlayersInRoom.Values.Any(p => p.Id == id));
    }

    public IEnumerable<string> GetAllRoomKeys()
    {
        return _gameRooms.Keys;
    }

    public void RemoveRoom(string roomId)
    {
        if (_gameRooms.ContainsKey(roomId))
        {
            _gameRooms.Remove(roomId);
        }
    }
}
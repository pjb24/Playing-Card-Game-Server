using Microsoft.AspNetCore.SignalR;

public class RequestFullRoomListCommandHandler : ICommandHandler<RequestFullRoomListDTO>
{
    private IHubContext<BlackjackHub> _hubContext;
    private readonly GameRoomManager _gameRoomManager;

    public RequestFullRoomListCommandHandler(IHubContext<BlackjackHub> hubContext, GameRoomManager gameRoomManager)
    {
        _hubContext = hubContext;
        _gameRoomManager = gameRoomManager;
    }

    public async Task HandleAsync(RequestFullRoomListDTO command, CommandContext context)
    {
        OnFullExistRoomListDTO onFullExistRoomListDTO = new();
        List<RoomInfoDTO> listRoomInfoDTO = new();
        foreach (var room in _gameRoomManager.GetAllRooms())
        {
            RoomInfoDTO roomInfoDTO = new();
            roomInfoDTO.roomName = room.RoomId;
            listRoomInfoDTO.Add(roomInfoDTO);
        }
        onFullExistRoomListDTO.rooms = listRoomInfoDTO;
        string onFullExistRoomListJson = Newtonsoft.Json.JsonConvert.SerializeObject(onFullExistRoomListDTO);
        await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnFullExistRoomList", onFullExistRoomListJson);
    }
}
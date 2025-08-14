using System.Linq;
using Microsoft.AspNetCore.SignalR;

public class RequestRoomChangesCommandHandler : ICommandHandler<RequestRoomChangesDTO>
{
    private readonly IHubContext<BlackjackHub> _hubContext;
    private readonly GameRoomManager _gameRoomManager;

    public RequestRoomChangesCommandHandler(IHubContext<BlackjackHub> hubContext, GameRoomManager gameRoomManager)
    {
        _hubContext = hubContext;
        _gameRoomManager = gameRoomManager;
    }

    public async Task HandleAsync(RequestRoomChangesDTO command, CommandContext context)
    {
        List<RoomInfoDTO> currentRooms = _gameRoomManager.GetAllRooms()
            .Select(room => new RoomInfoDTO { roomName = room.RoomId })
            .ToList();

        // Process room changes here (e.g., update room settings, notify players, etc.)
        List<RoomInfoDTO> roomsToRemove = command.roomList.Except(currentRooms).ToList();
        List<RoomInfoDTO> roomsToAdd = currentRooms.Except(command.roomList).ToList();

        OnChangedRoomListDTO onChangedRoomListDTO = new();
        onChangedRoomListDTO.roomsRemove = roomsToRemove;
        onChangedRoomListDTO.roomsAdd = roomsToAdd;
        string onChangedRoomListJson = Newtonsoft.Json.JsonConvert.SerializeObject(onChangedRoomListDTO);
        await _hubContext.Clients.Client(context.ConnectionId).SendAsync("ReceiveCommand", "OnChangedRoomList", onChangedRoomListJson);
    }
}
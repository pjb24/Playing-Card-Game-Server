using Microsoft.AspNetCore.SignalR;

public class BlackjackHub : Hub
{
    private readonly IHubContext<BlackjackHub> _hubContext;
    private readonly UserManager _userManager;
    private readonly GameRoomManager _gameRoomManager;

    public BlackjackHub(IHubContext<BlackjackHub> hubContext, UserManager userManager, GameRoomManager gameRoomManager)
    {
        _hubContext = hubContext;
        _userManager = userManager;
        _gameRoomManager = gameRoomManager;
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"User connected: {Context.ConnectionId}");

        // UI를 업데이트하거나 게임 상태를 관리하는 데 사용할 수 있습니다.
        // 예를 들어, 환영 메시지를 보내거나 사용자 목록을 업데이트할 수 있습니다.
        // 필요한 경우 사용자를 특정 게임 그룹에 추가할 수도 있습니다.
        // await Groups.AddToGroupAsync(Context.ConnectionId, "GameGroupName"); // 예시

        // 연결이 등록되고 통신 준비가 되었는지 확인합니다.
        // 이는 연결이 완전히 설정되기 전에 실행해야 하는 기본 클래스에 사용자 지정 로직이 있는 경우 특히 유용합니다.
        // 게임 상태나 사용자별 데이터를 초기화하기에 좋은 위치입니다.
        // 예를 들어, 사용자의 게임 기록이나 환경 설정을 로드할 수 있습니다.
        // 허브에서 사용할 필요한 리소스나 서비스를 설정할 수도 있습니다.

        // 새로 연결된 사용자에게 메시지를 보낼 수도 있습니다.
        await Clients.Caller.SendAsync("Welcome", "Welcome to the Blackjack game!");

        // 모든 클라이언트에게 메시지를 보낼 수 있습니다.
        await Clients.All.SendAsync("UserConnected", Context.ConnectionId);

        // 기본 클래스의 OnConnectedAsync 메서드를 호출하여 SignalR이 연결 수명 주기를 올바르게 관리하도록 합니다.
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"User disconnected: {Context.ConnectionId}");

        // 다른 사용자에게 연결이 끊어진 사용자가 있음을 알리는 메시지를 보낼 수 있습니다.
        await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);

        // 사용자가 특정 게임 그룹에 속해 있다면 해당 그룹에서 제거할 수도 있습니다.
        // await Groups.RemoveFromGroupAsync(Context.ConnectionId, "GameGroupName"); // 예시

        // 연결이 끊어진 후 SignalR이 필요한 정리 작업을 수행하도록 보장합니다.
        // 예를 들어, 연결이 끊어진 사용자의 세션 데이터를 정리하거나, 게임 상태를 업데이트하는 등의 작업을 수행할 수 있습니다.
        // 예를 들어, 게임 상태를 업데이트하거나 사용자 목록에서 제거할 수 있습니다.
        // 또한, 연결이 끊어진 후에도 허브가 여전히 활성 상태를 유지하도록 보장합니다.

        _userManager.RemoveUser(Context.ConnectionId); // 연결이 끊어진 사용자를 제거합니다.

        // 기본 클래스의 OnDisconnectedAsync 메서드를 호출하여 SignalR이 연결 수명 주기를 올바르게 관리하도록 합니다.
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGame(string userName)
    {
        var user = new User(userName, Context.ConnectionId, userName);
        _userManager.AddUser(user);

        _gameRoomManager.CreateRoom("defaultRoom", _hubContext, _userManager);

        user = _userManager.GetUserByConnectionId(Context.ConnectionId);
        if (user == null)
        {
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        var player = _userManager.GetPlayer(user.Id);
        if (player == null)
        {
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        _gameRoomManager.AddPlayerToRoom("defaultRoom", player);
        await Groups.AddToGroupAsync(Context.ConnectionId, "defaultRoom");

        await Clients.Caller.SendAsync("OnJoinSuccess", userName);
        await Clients.Others.SendAsync("OnUserJoined", userName);

        Console.WriteLine($"{userName} joined with connection ID: {Context.ConnectionId}");
    }

    public async Task StartGame()
    {
        var user = _userManager.GetUserByConnectionId(Context.ConnectionId);
        if (user == null)
        {
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        var player = _userManager.GetPlayer(user.Id);
        if (player == null)
        {
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        var room = _gameRoomManager.GetRoomByPlayerId(player.Id);
        if (room == null)
        {
            await Clients.Caller.SendAsync("OnError", "게임 방을 찾을 수 없습니다.");
            return;
        }

        room.StartGame();
        await Clients.Group(room.RoomId).SendAsync("OnGameStateChanged", room.CurrentState);
    }

    public async Task PlaceBet(int amount, string currentHandGuid)
    {
        var user = _userManager.GetUserByConnectionId(Context.ConnectionId);
        if (user == null)
        {
            Console.WriteLine("플레이어를 찾을 수 없습니다.");
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        var player = _userManager.GetPlayer(user.Id);
        if (player == null)
        {
            Console.WriteLine("플레이어를 찾을 수 없습니다.");
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        var room = _gameRoomManager.GetRoomByPlayerId(player.Id);
        if (room == null)
        {
            Console.WriteLine("게임 방을 찾을 수 없습니다.");
            await Clients.Caller.SendAsync("OnError", "게임 방을 찾을 수 없습니다.");
            return;
        }

        var guid = Guid.TryParse(currentHandGuid, out var parsedGuid) ? parsedGuid : Guid.Empty;
        if (guid == Guid.Empty)
        {
            Console.WriteLine("유효하지 않은 핸드 ID입니다.");
            await Clients.Caller.SendAsync("OnError", "유효하지 않은 핸드 ID입니다.");
            return;
        }

        var currentHand = player.GetHandById(guid);
        if (currentHand == null)
        {
            Console.WriteLine("해당 핸드를 찾을 수 없습니다.");
            await Clients.Caller.SendAsync("OnError", "해당 핸드를 찾을 수 없습니다.");
            return;
        }

        bool success = room.PlaceBet(player, amount, currentHand);
        if (success)
        {
            await Clients.Group(room.RoomId).SendAsync("OnBetPlaced", new
            {
                playerName = player.DisplayName,
                betAmount = amount,
                handId = currentHand.HandId.ToString()
            });
        }
        else
        {
            Console.WriteLine("베팅에 실패했습니다. 충분한 금액이 있는지 확인하세요.");
            await Clients.Caller.SendAsync("OnError", "베팅에 실패했습니다. 충분한 금액이 있는지 확인하세요.");
        }
    }

    public async Task Hit(string handGuid)
    {
        var user = _userManager.GetUserByConnectionId(Context.ConnectionId);
        if (user == null)
        {
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        var player = _userManager.GetPlayer(user.Id);
        if (player == null)
        {
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        var room = _gameRoomManager.GetRoomByPlayerId(player.Id);
        if (room == null)
        {
            await Clients.Caller.SendAsync("OnError", "게임 방을 찾을 수 없습니다.");
            return;
        }

        var guid = Guid.TryParse(handGuid, out var parsedGuid) ? parsedGuid : Guid.Empty;
        if (guid == Guid.Empty)
        {
            await Clients.Caller.SendAsync("OnError", "유효하지 않은 핸드 ID입니다.");
            return;
        }

        var hand = player.GetHandById(guid);
        if (hand == null)
        {
            await Clients.Caller.SendAsync("OnError", "해당 핸드를 찾을 수 없습니다.");
            return;
        }

        room.Hit(player, hand);
    }

    public async Task Stand(string handGuid)
    {
        var user = _userManager.GetUserByConnectionId(Context.ConnectionId);
        if (user == null)
        {
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        var player = _userManager.GetPlayer(user.Id);
        if (player == null)
        {
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        var room = _gameRoomManager.GetRoomByPlayerId(player.Id);
        if (room == null)
        {
            await Clients.Caller.SendAsync("OnError", "게임 방을 찾을 수 없습니다.");
            return;
        }

        var guid = Guid.TryParse(handGuid, out var parsedGuid) ? parsedGuid : Guid.Empty;
        if (guid == Guid.Empty)
        {
            await Clients.Caller.SendAsync("OnError", "유효하지 않은 핸드 ID입니다.");
            return;
        }

        var hand = player.GetHandById(guid);
        if (hand == null)
        {
            await Clients.Caller.SendAsync("OnError", "해당 핸드를 찾을 수 없습니다.");
            return;
        }

        room.Stand(player, hand);
    }

    public async Task Split(string handGuid)
    {
        var user = _userManager.GetUserByConnectionId(Context.ConnectionId);
        if (user == null)
        {
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        var player = _userManager.GetPlayer(user.Id);
        if (player == null)
        {
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        var room = _gameRoomManager.GetRoomByPlayerId(player.Id);
        if (room == null)
        {
            await Clients.Caller.SendAsync("OnError", "게임 방을 찾을 수 없습니다.");
            return;
        }

        var guid = Guid.TryParse(handGuid, out var parsedGuid) ? parsedGuid : Guid.Empty;
        if (guid == Guid.Empty)
        {
            await Clients.Caller.SendAsync("OnError", "유효하지 않은 핸드 ID입니다.");
            return;
        }

        var hand = player.GetHandById(guid);
        if (hand == null)
        {
            await Clients.Caller.SendAsync("OnError", "해당 핸드를 찾을 수 없습니다.");
            return;
        }

        room.Split(player, hand);
    }

    public async Task DoubleDown(string handGuid)
    {
        var user = _userManager.GetUserByConnectionId(Context.ConnectionId);
        if (user == null)
        {
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        var player = _userManager.GetPlayer(user.Id);
        if (player == null)
        {
            await Clients.Caller.SendAsync("OnError", "플레이어를 찾을 수 없습니다.");
            return;
        }

        var room = _gameRoomManager.GetRoomByPlayerId(player.Id);
        if (room == null)
        {
            await Clients.Caller.SendAsync("OnError", "게임 방을 찾을 수 없습니다.");
            return;
        }

        var guid = Guid.TryParse(handGuid, out var parsedGuid) ? parsedGuid : Guid.Empty;
        if (guid == Guid.Empty)
        {
            await Clients.Caller.SendAsync("OnError", "유효하지 않은 핸드 ID입니다.");
            return;
        }

        var hand = player.GetHandById(guid);
        if (hand == null)
        {
            await Clients.Caller.SendAsync("OnError", "해당 핸드를 찾을 수 없습니다.");
            return;
        }

        room.DoubleDown(player, hand);
    }

    public async Task LeaveGame(string gameId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
        await Clients.Group(gameId).SendAsync("UserLeft", Context.ConnectionId);
    }
}
using Microsoft.AspNetCore.SignalR;

public class BlackjackHub : Hub
{
    private readonly PlayerManager _playerManager;

    public BlackjackHub(PlayerManager playerManager)
    {
        _playerManager = playerManager;
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

        _playerManager.RemovePlayer(Context.ConnectionId); // 연결이 끊어진 사용자를 제거합니다.

        // 기본 클래스의 OnDisconnectedAsync 메서드를 호출하여 SignalR이 연결 수명 주기를 올바르게 관리하도록 합니다.
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task JoinGame(string playerName)
    {
        var player = new Player(playerName, Context.ConnectionId);
        _playerManager.AddPlayer(player);

        await Clients.Caller.SendAsync("OnJoinSuccess", playerName);
        await Clients.Others.SendAsync("OnPlayerJoined", playerName);

        Console.WriteLine($"{playerName} joined with connection ID: {Context.ConnectionId}");

        // await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        // await Clients.Group(gameId).SendAsync("UserJoined", Context.ConnectionId);
    }

    public async Task LeaveGame(string gameId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
        await Clients.Group(gameId).SendAsync("UserLeft", Context.ConnectionId);
    }
}
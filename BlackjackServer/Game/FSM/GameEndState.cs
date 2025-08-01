public class GameEndState : IGameState
{
    private readonly GameRoom _gameRoom;

    public GameEndState(GameRoom gameRoom)
    {
        _gameRoom = gameRoom;
    }

    public void Enter()
    {
        // 게임 종료 상태로 전환될 때 호출됩니다.
        // 필요한 초기화 작업을 수행할 수 있습니다.

        foreach (var player in _gameRoom.PlayersInGame)
        {
            player.ClearHand();
            player.SetAllHandDoneReset();
        }

        _gameRoom.Dealer.ResetHand();

        OnGameEndDTO onGameEndDTO = new();
        string onGameEndJson = Newtonsoft.Json.JsonConvert.SerializeObject(onGameEndDTO);
        _ = _gameRoom.SendToAll("OnGameEnd", onGameEndJson);
    }

    public void Exit()
    {
        // 게임 종료 상태를 벗어날 때 호출됩니다.
        // 필요한 정리 작업을 수행할 수 있습니다.
    }

    public void Update()
    {
        // 게임 상태가 업데이트될 때 호출됩니다.
        // 게임 종료 로직을 처리하거나, 플레이어의 상태를 확인하는 등의 작업을 수행할 수 있습니다.
    }
}
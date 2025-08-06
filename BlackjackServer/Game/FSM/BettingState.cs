public class BettingState : IGameState
{
    private readonly GameRoom _gameRoom;

    public BettingState(GameRoom gameRoom)
    {
        _gameRoom = gameRoom;
    }

    public void Enter()
    {
        // 게임 상태가 베팅 상태로 전환될 때 호출됩니다.
        // 필요한 초기화 작업을 수행할 수 있습니다.

        // 현재 플레이어의 핸드에 대해 베팅을 대기중임을 Client에 알림
        OnTimeToBettingDTO onTimeToBettingDTO = new();
        string onTimeToBettingJson = Newtonsoft.Json.JsonConvert.SerializeObject(onTimeToBettingDTO);
        _ = _gameRoom.SendToAll("OnTimeToBetting", onTimeToBettingJson);
    }

    public void Exit()
    {
        // 게임 상태가 베팅 상태를 벗어날 때 호출됩니다.
        // 필요한 정리 작업을 수행할 수 있습니다.
    }

    public void Update()
    {
        // 게임 상태가 업데이트될 때 호출됩니다.
        // 베팅 로직을 처리하거나, 플레이어의 베팅을 확인하는 등의 작업을 수행할 수 있습니다.
    }
}
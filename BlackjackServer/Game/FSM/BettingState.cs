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

        var currentPlayer = _gameRoom.GetNextBettingPlayer();
        if (currentPlayer == null)
        {
            // 모든 플레이어가 베팅을 완료한 경우
            // DealingState로 전이
            _gameRoom.ChangeState(new DealingState(_gameRoom));

            return;
        }

        var currentHand = currentPlayer.GetNextBettingHand();
        if (currentHand == null)
        {
            // 현재 플레이어의 모든 핸드가 베팅을 완료한 경우
            currentPlayer.SetAllHandBettingDone();

            // 다음 플레이어로 전환
            _gameRoom.ChangeState(new BettingState(_gameRoom));

            return;
        }

        // 현재 플레이어의 핸드에 대해 베팅을 대기중임을 Client에 알림
        _ = _gameRoom.SendToPlayer(currentPlayer, "OnTimeToBetting", new
        {
            handId = currentHand.HandId.ToString()
        });
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
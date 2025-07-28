public class GameStartState : IGameState
{
    private readonly GameRoom _gameRoom;

    public GameStartState(GameRoom gameRoom)
    {
        _gameRoom = gameRoom;
    }

    public void Enter()
    {
        // 게임 시작 상태로 전환될 때 호출됩니다.
        // 필요한 초기화 작업을 수행할 수 있습니다.

        // playersInRoom과 playersInGame을 비교하여
        // playersInGame에 없지만 playersInRoom에는 있는 player를
        // playersInGame에 추가
        foreach (var player in _gameRoom.PlayersInRoom)
        {
            if (!_gameRoom.PlayersInGame.Contains(player))
            {
                _gameRoom.AddPlayerToGame(player);
            }
        }

        foreach (var player in _gameRoom.PlayersInGame)
        {
            if (player.Hands.Count == 0)
            {
                player.AddHand(new PlayerHand());
            }
        }

        _gameRoom.ChangeState(new BettingState(_gameRoom));
    }

    public void Exit()
    {
        // 게임 시작 상태를 벗어날 때 호출됩니다.
        // 필요한 정리 작업을 수행할 수 있습니다.
    }

    public void Update()
    {
        // 게임 상태가 업데이트될 때 호출됩니다.
        // 게임 시작 로직을 처리하거나, 플레이어의 준비 상태를 확인하는 등의 작업을 수행할 수 있습니다.
    }
}
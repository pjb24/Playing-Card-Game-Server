public class PlayerTurnState : IGameState
{
    private readonly GameRoom _gameRoom;

    private Player? _currentPlayer;
    private PlayerHand? _currentHand;

    public PlayerTurnState(GameRoom gameRoom)
    {
        _gameRoom = gameRoom;
    }

    public void Enter()
    {
        // 플레이어 턴 상태로 전환될 때 호출됩니다.
        // 필요한 초기화 작업을 수행할 수 있습니다.

        _currentPlayer = _gameRoom.GetNextActionPlayer();
        if (_currentPlayer == null)
        {
            // 모든 플레이어의 턴이 끝났다면 게임 상태를 변경합니다.
            _gameRoom.ChangeState(new DealerTurnState(_gameRoom));

            return;
        }

        _currentHand = _currentPlayer.GetNextActionHand();
        if (_currentHand == null)
        {
            // 현재 플레이어의 모든 핸드 액션이 끝났다면 다음 플레이어로 넘어갑니다.
            _currentPlayer.SetAllHandActionDone();
            _gameRoom.ChangeState(new PlayerTurnState(_gameRoom));

            return;
        }

        if (_currentHand.GetCardCount() < 2)
        {
            var card = _gameRoom.Deck.DrawCard();
            _currentHand.AddCard(card);
            _ = _gameRoom.SendToAll("OnCardDealt", new
            {
                playerGuid = _currentPlayer.Guid.ToString(),
                playerName = _currentPlayer.DisplayName,
                cardString = card.ToString(),
                handId = _currentHand.HandId.ToString()
            });
        }

        _ = _gameRoom.SendToAll("OnTimeToAction", new
        {
            handId = _currentHand.HandId.ToString(),
            playerGuid = _currentPlayer.Guid.ToString(),
            playerName = _currentPlayer.DisplayName
        });
    }

    public void Exit()
    {
        // 플레이어 턴 상태를 벗어날 때 호출됩니다.
        // 필요한 정리 작업을 수행할 수 있습니다.
    }

    public void Update()
    {
        // 게임 상태가 업데이트될 때 호출됩니다.
        // 플레이어의 턴 로직을 처리하거나, 딜러의 상태를 확인하는 등의 작업을 수행할 수 있습니다.
    }
}
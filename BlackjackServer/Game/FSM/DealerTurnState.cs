public class DealerTurnState : IGameState
{
    private readonly GameRoom _gameRoom;

    public DealerTurnState(GameRoom gameRoom)
    {
        _gameRoom = gameRoom;
    }

    public void Enter()
    {
        // 딜러 턴 상태로 전환될 때 호출됩니다.
        // 필요한 초기화 작업을 수행할 수 있습니다.

        DealerBehavior();

        _gameRoom.ChangeState(new ResultState(_gameRoom));
    }

    public void Exit()
    {
        // 딜러 턴 상태를 벗어날 때 호출됩니다.
        // 필요한 정리 작업을 수행할 수 있습니다.
    }

    public void Update()
    {
        // 게임 상태가 업데이트될 때 호출됩니다.
        // 딜러의 턴 로직을 처리하거나, 플레이어의 상태를 확인하는 등의 작업을 수행할 수 있습니다.
    }

    private void DealerBehavior()
    {
        _gameRoom.RevealHoleCard();

        var dealer = _gameRoom.Dealer;

        while (dealer.ShouldHit())
        {
            var card = _gameRoom.Deck.DrawCard();
            dealer.AddCard(card);
            _ = _gameRoom.SendToAll("OnDealerCardDealt", new
            {
                cardString = card.ToString()
            });
        }
    }
}
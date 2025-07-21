public class DealingState : IGameState
{
    private readonly GameRoom _gameRoom;

    public DealingState(GameRoom gameRoom)
    {
        _gameRoom = gameRoom;
    }

    public void Enter()
    {
        // 딜링 상태로 전환될 때 호출됩니다.
        // 필요한 초기화 작업을 수행할 수 있습니다.

        DealCards();

        _gameRoom.ChangeState(new PlayerTurnState(_gameRoom));
    }

    public void Exit()
    {
        // 딜링 상태를 벗어날 때 호출됩니다.
        // 필요한 정리 작업을 수행할 수 있습니다.
    }

    public void Update()
    {
        // 게임 상태가 업데이트될 때 호출됩니다.
        // 딜링 로직을 처리하거나, 플레이어의 상태를 확인하는 등의 작업을 수행할 수 있습니다.
    }

    private void DealCards()
    {
        var players = _gameRoom.PlayersInGame;
        var dealer = _gameRoom.Dealer;

        // 플레이어들에게 첫번째 카드를 분배
        foreach (var player in players)
        {
            foreach (var hand in player.Hands)
            {
                var card = _gameRoom.Deck.DrawCard();
                hand.AddCard(card);
                _ = _gameRoom.SendToAll("OnCardDealt", new
                {
                    playerGuid = player.Guid.ToString(),
                    playerName = player.DisplayName,
                    cardString = card.ToString(),
                    handId = hand.HandId
                });
            }
        }

        // 딜러에게 첫번째 카드 분배
        var dealerCard1 = _gameRoom.Deck.DrawCard();
        dealer.AddCard(dealerCard1);
        _ = _gameRoom.SendToAll("OnDealerCardDealt", new
        {
            cardString = dealerCard1.ToString()
        });

        // 플레이어들에게 두번째 카드를 분배
        foreach (var player in players)
        {
            foreach (var hand in player.Hands)
            {
                var card = _gameRoom.Deck.DrawCard();
                hand.AddCard(card);
                _ = _gameRoom.SendToAll("OnCardDealt", new
                {
                    playerGuid = player.Guid.ToString(),
                    playerName = player.DisplayName,
                    cardString = card.ToString(),
                    handId = hand.HandId.ToString()
                });
            }
        }

        // 딜러에게 두번째 카드 분배
        var dealerCard2 = _gameRoom.Deck.DrawCard();
        dealer.AddCard(dealerCard2);
        _ = _gameRoom.SendToAll("OnDealerHiddenCardDealt", new { });
    }
}
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

                OnCardDealtDTO onCardDealtDTO = new();
                onCardDealtDTO.playerGuid = player.Guid.ToString();
                onCardDealtDTO.playerName = player.DisplayName;
                onCardDealtDTO.cardString = card.ToString();
                onCardDealtDTO.handId = hand.HandId.ToString();
                string onCardDealtJson = Newtonsoft.Json.JsonConvert.SerializeObject(onCardDealtDTO);
                _ = _gameRoom.SendToAll("OnCardDealt", onCardDealtJson);
            }
        }

        // 딜러에게 첫번째 카드 분배
        var dealerCard1 = _gameRoom.Deck.DrawCard();
        dealer.AddCard(dealerCard1);

        OnDealerCardDealtDTO onDealerCardDealtDTO = new();
        onDealerCardDealtDTO.cardString = dealerCard1.ToString();
        string onDealerCardDealtJson = Newtonsoft.Json.JsonConvert.SerializeObject(onDealerCardDealtDTO);
        _ = _gameRoom.SendToAll("OnDealerCardDealt", onDealerCardDealtJson);

        // 플레이어들에게 두번째 카드를 분배
        foreach (var player in players)
        {
            foreach (var hand in player.Hands)
            {
                var card = _gameRoom.Deck.DrawCard();
                hand.AddCard(card);

                OnCardDealtDTO onCardDealtDTO = new();
                onCardDealtDTO.playerGuid = player.Guid.ToString();
                onCardDealtDTO.playerName = player.DisplayName;
                onCardDealtDTO.cardString = card.ToString();
                onCardDealtDTO.handId = hand.HandId.ToString();
                string onCardDealtJson = Newtonsoft.Json.JsonConvert.SerializeObject(onCardDealtDTO);
                _ = _gameRoom.SendToAll("OnCardDealt", onCardDealtJson);
            }
        }

        // 딜러에게 두번째 카드 분배
        var dealerCard2 = _gameRoom.Deck.DrawCard();
        dealer.AddCard(dealerCard2);

        OnDealerHiddenCardDealtDTO onDealerHiddenCardDealtDTO = new();
        string onDealerHiddenCardDealtJson = Newtonsoft.Json.JsonConvert.SerializeObject(onDealerHiddenCardDealtDTO);
        _ = _gameRoom.SendToAll("OnDealerHiddenCardDealt", onDealerHiddenCardDealtJson);
    }
}
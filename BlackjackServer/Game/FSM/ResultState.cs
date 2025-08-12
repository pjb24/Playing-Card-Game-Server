using System.Linq.Expressions;

public class ResultState : IGameState
{
    private readonly GameRoom _gameRoom;

    public ResultState(GameRoom gameRoom)
    {
        _gameRoom = gameRoom;
    }

    public void Enter()
    {
        // 게임 상태가 결과 상태로 전환될 때 호출됩니다.
        // 필요한 초기화 작업을 수행할 수 있습니다.

        EvaluateResults();

        _gameRoom.ChangeState(new GameEndState(_gameRoom));
    }

    public void Exit()
    {
        // 게임 상태가 결과 상태를 벗어날 때 호출됩니다.
        // 필요한 정리 작업을 수행할 수 있습니다.
    }

    public void Update()
    {
        // 게임 상태가 업데이트될 때 호출됩니다.
        // 결과 처리 로직을 수행하거나, 플레이어의 상태를 확인하는 등의 작업을 수행할 수 있습니다.
    }

    private void EvaluateResults()
    {
        foreach (var player in _gameRoom.PlayersInGame)
        {
            foreach (var hand in player.Hands)
            {
                E_EvaluationResult result = EvaluateHand(hand, _gameRoom.Dealer.Hand);

                ApplyPayout(player, hand, result);

                OnPayoutDTO onPayoutDTO = new();
                onPayoutDTO.playerGuid = player.Guid.ToString();
                onPayoutDTO.handId = hand.HandId.ToString();
                onPayoutDTO.evaluationResult = result;
                string onPayoutJson = Newtonsoft.Json.JsonConvert.SerializeObject(onPayoutDTO);
                _ = _gameRoom.SendToPlayer(player, "OnPayout", onPayoutJson);

                OnPlayerRemainChipsDTO onPlayerRemainChipsDTO = new();
                onPlayerRemainChipsDTO.playerGuid = player.Guid.ToString();
                onPlayerRemainChipsDTO.chips = player.Chips.ToString();
                string onPlayerRemainChipsJson = Newtonsoft.Json.JsonConvert.SerializeObject(onPlayerRemainChipsDTO);
                _ = _gameRoom.SendToAll("OnPlayerRemainChips", onPlayerRemainChipsJson);
            }
        }
    }

    private E_EvaluationResult EvaluateHand(PlayerHand playerHand, DealerHand dealerHand)
    {
        E_EvaluationResult result;

        bool isBlackjackPlayer = playerHand.IsBlackjack();
        bool isBlackjackDealer = dealerHand.IsBlackjack();

        bool isBustPlayer = playerHand.IsBust();
        bool isBustDealer = dealerHand.IsBust();

        int playerTotal = playerHand.GetTotalValue();
        int dealerTotal = dealerHand.GetTotalValue();

        if (isBlackjackPlayer && isBlackjackDealer)
        {
            result = E_EvaluationResult.Push; // Both have blackjack
        }
        else if (isBlackjackPlayer)
        {
            result = E_EvaluationResult.Blackjack; // Player has blackjack
        }
        else if (isBlackjackDealer)
        {
            result = E_EvaluationResult.Lose; // Dealer has blackjack
        }
        else if (isBustPlayer)
        {
            result = E_EvaluationResult.Lose; // Player busts
        }
        else if (isBustDealer)
        {
            result = E_EvaluationResult.Win; // Dealer busts
        }
        else if (playerTotal > dealerTotal)
        {
            result = E_EvaluationResult.Win; // Player wins
        }
        else if (playerTotal < dealerTotal)
        {
            result = E_EvaluationResult.Lose; // Dealer wins
        }
        else
        {
            result = E_EvaluationResult.Push; // Tie
        }

        return result;
    }

    private void ApplyPayout(Player player, PlayerHand hand, E_EvaluationResult result)
    {
        switch (result)
        {
            case E_EvaluationResult.Win:
                player.Win(hand);
                break;
            case E_EvaluationResult.Lose:
                player.Lose(hand);
                break;
            case E_EvaluationResult.Push:
                player.Push(hand);
                break;
            case E_EvaluationResult.Blackjack:
                player.Blackjack(hand);
                break;
        }
    }
}
public class DealerHand : Hand
{
    private bool _dealerHitOnSoftCount = true;
    private int _softCount = 17;

    public DealerHand() : base()
    {
    }

    public bool ShouldHit()
    {
        int value = GetTotalValue();

        if (value < _softCount)
        {
            return true;
        }

        if (value == _softCount && _dealerHitOnSoftCount)
        {
            return IsSoftCount();
        }

        return false;
    }

    private bool IsSoftCount()
    {
        int total = 0;
        int aceCount = 0;

        foreach (var card in _cards)
        {
            int value = card.GetValue();
            total += value;

            if (card.Rank == E_CardRank.Ace)
            {
                aceCount++;
            }
        }

        return total == _softCount && aceCount > 0;
    }

    public Card? GetHiddenCard()
    {
        if (_cards.Count > 1)
        {
            return _cards[1]; // 두번째 카드를 반환 (딜러의 숨겨진 카드)
        }

        return null; // 숨겨진 카드가 없을 경우
    }
}
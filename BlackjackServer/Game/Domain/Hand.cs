public class Hand
{
    protected List<Card> _cards;
    public IReadOnlyList<Card> Cards => _cards.AsReadOnly();

    public Hand()
    {
        _cards = new List<Card>();
    }

    public void AddCard(Card card)
    {
        _cards.Add(card);
    }

    public void RemoveCard(Card card)
    {
        if (_cards.Contains(card))
        {
            _cards.Remove(card);
        }
    }

    public void Clear()
    {
        _cards.Clear();
    }

    public int GetTotalValue()
    {
        // 카드의 총 가치를 계산하는 로직을 구현합니다.

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

        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }

    public bool IsBust()
    {
        return GetTotalValue() > 21;
    }

    public bool IsBlackjack()
    {
        return _cards.Count == 2 && GetTotalValue() == 21;
    }
}
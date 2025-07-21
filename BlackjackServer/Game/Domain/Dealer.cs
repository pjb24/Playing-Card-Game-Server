public class Dealer
{
    private DealerHand _hand = new();
    public DealerHand Hand => _hand;

    public bool ShouldHit()
    {
        return _hand.ShouldHit();
    }

    public void ResetHand()
    {
        _hand.Clear();
    }

    public void AddCard(Card card)
    {
        _hand.AddCard(card);
    }

    public Card? GetHiddenCard()
    {
        return _hand.GetHiddenCard();
    }
}
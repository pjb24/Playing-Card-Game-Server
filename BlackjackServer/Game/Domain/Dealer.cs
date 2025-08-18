public class Dealer
{
    private DealerHand _hand = new();
    public DealerHand Hand => _hand;

    private bool _isHoleCardRevealed = false;
    public bool IsHoleCardRevealed => _isHoleCardRevealed;

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

    public void SetHoleCardRevealed()
    {
        _isHoleCardRevealed = true;
    }

    public void ResetHoleCardRevealed()
    {
        _isHoleCardRevealed = false;
    }
}
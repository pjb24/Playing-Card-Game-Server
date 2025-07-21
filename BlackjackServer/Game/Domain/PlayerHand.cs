public class PlayerHand : Hand
{
    private readonly Guid _handId = Guid.NewGuid();
    public Guid HandId => _handId;

    private int _betAmount;
    public int BetAmount => _betAmount;

    private bool _isBettingDone = false;
    public bool IsBettingDone => _isBettingDone;
    private bool _isActionDone = false;
    public bool IsActionDone => _isActionDone;

    public PlayerHand() : base()
    {
    }

    public void PlaceBet(int amount)
    {
        _betAmount = amount;
        _isBettingDone = true;
    }

    public bool CanSplit()
    {
        if (_cards.Count != 2)
        {
            return false;
        }

        if (_cards[0].Rank != _cards[1].Rank)
        {
            return false;
        }

        return true;
    }

    public bool CanDoubleDown()
    {
        if (_cards.Count != 2)
        {
            return false;
        }

        return true;
    }

    public void SetActionDone()
    {
        _isActionDone = true;
    }

    public int GetCardCount()
    {
        return _cards.Count;
    }
}
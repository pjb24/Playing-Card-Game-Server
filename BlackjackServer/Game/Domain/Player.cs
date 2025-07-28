public class Player
{
    private readonly Guid _guid = Guid.NewGuid();
    public Guid Guid => _guid;

    private string _id;
    public string Id => _id;
    private string? _displayName;
    public string DisplayName => _displayName ?? "Unknown Player";

    private int _chips;
    public int Chips => _chips;

    private List<PlayerHand> _hands = new();
    public IReadOnlyList<PlayerHand> Hands => _hands.AsReadOnly();

    private bool _isAllHandBettingDone = false;
    public bool IsAllHandBettingDone => _isAllHandBettingDone;
    private bool _isAllHandActionDone = false;
    public bool IsAllHandActionDone => _isAllHandActionDone;

    public Player(string id, string displayName, int initialChips)
    {
        _id = id;
        _displayName = displayName;
        _chips = initialChips;
    }

    public void AddHand(PlayerHand hand)
    {
        if (!_hands.Contains(hand))
        {
            _hands.Add(hand);
        }
    }

    public PlayerHand? GetHandById(Guid handId)
    {
        return _hands.FirstOrDefault(hand => hand.HandId == handId);
    }

    public void ClearHand()
    {
        _hands.Clear();
    }

    public PlayerHand? GetNextBettingHand()
    {
        return _hands.FirstOrDefault(hand => !hand.IsBettingDone);
    }

    public PlayerHand? GetNextActionHand()
    {
        return _hands.FirstOrDefault(hand => !hand.IsActionDone);
    }

    public void SetAllHandBettingDone()
    {
        _isAllHandBettingDone = true;
    }

    public void SetAllHandActionDone()
    {
        _isAllHandActionDone = true;
    }

    public void SetAllHandDoneReset()
    {
        _isAllHandBettingDone = false;
        _isAllHandActionDone = false;
    }

    public bool PlaceBet(int amount, PlayerHand hand)
    {
        if (Chips < amount)
        {
            return false; // Not enough chips to place bet
        }

        hand.PlaceBet(amount);
        _chips -= amount;

        return true;
    }

    public bool DoubleDown(PlayerHand hand)
    {
        if (Chips < hand.BetAmount)
        {
            return false; // Not enough chips to place bet
        }

        _chips -= hand.BetAmount;
        hand.PlaceBet(hand.BetAmount * 2);

        return true;
    }

    public void Win(PlayerHand hand)
    {
        int payout = hand.BetAmount * 2;
        _chips += payout;
    }

    public void Lose(PlayerHand hand)
    {

    }

    public void Push(PlayerHand hand)
    {
        int payout = hand.BetAmount;
        _chips += payout;
    }

    public void Blackjack(PlayerHand hand)
    {
        int payout = (int)(hand.BetAmount * 2.5f);
        _chips += payout;
    }
}
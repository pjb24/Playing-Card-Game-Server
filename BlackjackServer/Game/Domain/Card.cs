public enum E_CardSuit
{
    Spades,
    Hearts,
    Diamonds,
    Clovers,
}

public enum E_CardRank
{
    Ace = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
}

public class Card
{
    private E_CardSuit _suit;
    private E_CardRank _rank;
    public E_CardRank Rank => _rank;

    public Card(E_CardSuit suit, E_CardRank rank)
    {
        _suit = suit;
        _rank = rank;
    }

    public int GetValue()
    {
        int result = (int)_rank;

        if (_rank >= E_CardRank.Ten)
        {
            result = 10; // J, Q, K는 10으로 처리
        }

        // Ace는 1 또는 11로 계산될 수 있지만, 여기서는 11로 고정합니다.
        if (_rank == E_CardRank.Ace)
        {
            return 11;
        }

        return result;
    }

    public override string ToString()
    {
        return $"{_rank} of {_suit}";
    }
}
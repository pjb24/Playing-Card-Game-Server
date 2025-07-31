public class Deck
{
    private List<Card> _shoe;
    private int _numberOfDecks;
    private System.Random rng = new System.Random();

    // 남은 카드가 18% 이하일 때 셔플링을 수행합니다.
    private float _shuffleThreshold = 0.18f;

    // shoe의 남은 카드 개수 확인
    public int RemainingCards => _shoe.Count;
    public float RemainingRatio => (float)RemainingCards / (_numberOfDecks * 52);
    public bool IsShuffleNeeded => RemainingRatio <= _shuffleThreshold;

    public Deck(int numberOfDecks = 6)
    {
        _numberOfDecks = numberOfDecks;
        _shoe = new List<Card>();
        InitializeDeck();
    }

    private void InitializeDeck()
    {
        _shoe.Clear();
        for (int i = 0; i < _numberOfDecks; i++)
        {
            _shoe.AddRange(GenerateStandardDeck());
        }
        Shuffle();
    }

    private List<Card> GenerateStandardDeck()
    {
        List<Card> deck = new List<Card>();
        foreach (E_CardSuit suit in Enum.GetValues(typeof(E_CardSuit)))
        {
            foreach (E_CardRank rank in Enum.GetValues(typeof(E_CardRank)))
            {
                deck.Add(new Card(suit, rank));
            }
        }

        return deck;
    }

    // Fisher-Yates Shuffle 알고리즘을 사용하여 카드 셔플링
    private void Shuffle()
    {
        int n = _shoe.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            Card temp = _shoe[i];
            _shoe[i] = _shoe[j];
            _shoe[j] = temp;
        }
    }

    public Card DrawCard()
    {
        if (_shoe.Count == 0)
        {
            // 카드가 없으면 덱을 초기화하고 셔플링합니다.
            // 보통은 _shuffleThreshold 이하로 남으면 셔플
            InitializeDeck();
        }

        Card card = _shoe[0];
        _shoe.RemoveAt(0);

        return card;
    }

    public void PrepareNextRound()
    {
        if (IsShuffleNeeded)
        {
            InitializeDeck();
        }
    }
}
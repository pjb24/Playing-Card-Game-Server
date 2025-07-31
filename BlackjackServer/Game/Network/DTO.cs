#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

// Server To Client DTO

public class WelcomeDTO
{
    public string message { get; set; }
}

public class UserConnectedDTO
{
    public string message { get; set; }
}

public class UserDisconnectedDTO
{
    public string message { get; set; }
}

public class OnErrorDTO
{
    public string message { get; set; }
}

public class OnJoinSuccessDTO
{
    public string userName { get; set; }
    public string playerGuid { get; set; }
}

public class OnUserJoinedDTO
{
    public string userName { get; set; }
}

public class OnPlayerRemainChipsDTO
{
    public string chips { get; set; }
}

public class OnGameStateChangedDTO
{
    public string state { get; set; }
}

public class OnBetPlacedDTO
{
    public string playerName { get; set; }
    public int betAmount { get; set; }
    public string handId { get; set; }
}

public class UserLeftDTO
{
    public string connectionId { get; set; }
}

public class OnTimeToBettingDTO
{
    public string handId { get; set; }
}

public class OnPayoutDTO
{
    public string handId { get; set; }
    public string evaluationResult { get; set; }
}

public class OnCardDealtDTO
{
    public string playerGuid { get; set; }
    public string playerName { get; set; }
    public string cardString { get; set; }
    public string handId { get; set; }
}

public class OnPlayerBustedDTO
{
    public string playerGuid { get; set; }
    public string playerName { get; set; }
    public string handId { get; set; }
}

public class OnActionDoneDTO
{
    public string playerGuid { get; set; }
    public string playerName { get; set; }
    public string handId { get; set; }
}

public class OnHandSplitDTO
{
    public string playerName { get; set; }
    public string handId { get; set; }
    public string newHandId { get; set; }
}

public class OnDealerHoleCardRevealedDTO
{
    public string cardString { get; set; }
}

public class OnDealerCardDealtDTO
{
    public string cardString { get; set; }
}

public class OnDealerHiddenCardDealtDTO
{
}

public class OnTimeToActionDTO
{
    public string handId { get; set; }
    public string playerGuid { get; set; }
    public string playerName { get; set; }
}

public class OnHandEvaluationDTO
{
    public string playerGuid { get; set; }
    public string playerName { get; set; }
    public string handId { get; set; }
    public string evaluationResult { get; set; }
}

// Server To Client DTO


// Client To Server DTO

public class JoinGameDTO
{
    public string userName { get; set; }
}

public class StartGameDTO
{
}

public class PlaceBetDTO
{
    public int amount { get; set; }
    public string handId { get; set; }
}

public class HitDTO
{
    public string handId { get; set; }
}

public class StandDTO
{
    public string handId { get; set; }
}

public class SplitDTO
{
    public string handId { get; set; }
}

public class DoubleDownDTO
{
    public string handId { get; set; }
}

public class LeaveGameDTO
{
    public string gameId { get; set; }
}

// Client To Server DTO

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

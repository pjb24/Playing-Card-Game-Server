public class GameStateMachine
{
    private IGameState? _currentState;
    public IGameState? CurrentState => _currentState;

    public void ChangeState(IGameState newState)
    {
        if (_currentState != null)
        {
            Console.WriteLine($"Exiting state: {_currentState.GetType().Name}");
            _currentState.Exit();
        }

        _currentState = newState;

        if (_currentState != null)
        {
            Console.WriteLine($"Entering state: {_currentState.GetType().Name}");
            _currentState.Enter();
        }
    }

    public void Update()
    {
        _currentState?.Update();
    }
}
using System;

namespace GltronMonoGame
{
    public enum GameState
    {
        SplashScreen,
        MainMenu,
        Playing,
        GameOver,
        Paused
    }

    public class GameStateManager
    {
        private GameState _currentState;
        private GameState _previousState;
        private float _stateTimer;

        public GameState CurrentState => _currentState;
        public GameState PreviousState => _previousState;
        public float StateTimer => _stateTimer;

        public GameStateManager()
        {
            _currentState = GameState.SplashScreen;
            _previousState = GameState.SplashScreen;
            _stateTimer = 0f;
        }

        public void ChangeState(GameState newState)
        {
            if (_currentState != newState)
            {
                _previousState = _currentState;
                _currentState = newState;
                _stateTimer = 0f;
                
                try { Android.Util.Log.Info("GLTRON", $"State changed: {_previousState} -> {_currentState}"); } catch { }
            }
        }

        public void Update(float deltaTime)
        {
            _stateTimer += deltaTime;
        }

        public bool IsState(GameState state)
        {
            return _currentState == state;
        }

        public bool HasBeenInStateFor(float seconds)
        {
            return _stateTimer >= seconds;
        }
    }
}

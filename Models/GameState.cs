using System;

namespace monogame_test.Models
{
    // Enum for game states
    public enum GameStateType
    {
        MainMenu,
        Options,
        Playing,
        GameOver
    }

    // Singleton for game state management
    public class GameStateManager
    {
        private static GameStateManager _instance;
        public GameStateType CurrentState { get; private set; } = GameStateType.MainMenu;

        private GameStateManager() {}

        public static GameStateManager Instance => _instance ??= new GameStateManager();

        public void ChangeState(GameStateType newState)
        {
            CurrentState = newState;
        }
    }
}

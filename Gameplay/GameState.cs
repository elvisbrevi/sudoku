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

        public bool gameInitialized = false;
        public bool puzzleSolved = false;

        private int _selectedRow = -1;
        private int _selectedCol = -1;

        private GameStateManager() {}

        public static GameStateManager Instance => _instance ??= new GameStateManager();

        public void ChangeState(GameStateType newState)
        {
            CurrentState = newState;
        }

        public void InitializeNewGame()
        {
            // Reset and initialize the SudokuGrid singleton with the configured size
            SudokuGrid.Reset();
            SudokuGrid.Initialize(GameConfig.Instance.Size);
            
            // Create the puzzle with the configured difficulty
            SudokuGrid.Instance.CreatePuzzle(GameConfig.Instance.Difficulty);
            
            _selectedRow = -1;
            _selectedCol = -1;
            puzzleSolved = false;
            gameInitialized = true;
            
            // Actualizar el panel de selección de valores con la configuracion actual
            UI_Manager.Instance.UpdateValueSelectionPanel();
        }
    }
}

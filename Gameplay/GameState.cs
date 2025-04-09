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
    public sealed class GameStateManager
    {
        public GameStateType CurrentState { get; private set; } = GameStateType.MainMenu;

        public bool gameInitialized = false;
        public bool puzzleSolved = false;

        private int _selectedRow = -1;
        private int _selectedCol = -1;
        private UI_Manager _uiManager = UI_Manager.GetInstance();
        private SudokuGrid _sudoGrid = SudokuGrid.GetInstance();
        private GameConfig _gameConfig = GameConfig.GetInstance();
        private static GameStateManager _instance;
        private GameStateManager() { }

        public static GameStateManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new GameStateManager();
            }
            return _instance;
        }

        public void ChangeState(GameStateType newState)
        {
            CurrentState = newState;
        }

        public void InitializeNewGame()
        {
            // Reset and initialize the SudokuGrid singleton with the configured size
            SudokuGrid.Reset();
            SudokuGrid.Initialize(_gameConfig.Size);

            // Create the puzzle with the configured difficulty
            _sudoGrid.CreatePuzzle(_gameConfig.Difficulty);

            _selectedRow = -1;
            _selectedCol = -1;
            puzzleSolved = false;
            gameInitialized = true;

            // Actualizar el panel de selección de valores con la configuracion actual
            _uiManager.UpdateValueSelectionPanel();
        }
    }
}

using System.Collections.Generic;
using monogame_test.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

public class UI_Manager {
    private GameConfig _gameConfig;
    private List<UIComponent> _mainMenuComponents = new List<UIComponent>();
    public List<UIComponent> optionsComponents = new List<UIComponent>();
    public List<UIComponent> gameplayComponents = new List<UIComponent>();
    private ValueSelectionPanel _valueSelectionPanel;
    public List<UIComponent> gameOverComponents = new List<UIComponent>();
    private static UI_Manager _instance;
    public static UI_Manager Instance => _instance ??= new UI_Manager();

    private GameStateManager _gameStateManager = GameStateManager.Instance;

    public UI_Manager() {
        _gameConfig = GameConfig.Instance;
    }

    public void Load(
        Game game,
        SpriteFont font
    ) {
        CreateMainMenuUI(game, font);
        CreateOptionsUI();
        CreateGameplayUI();
        CreateGameOverUI();
    }
    public void CreateGameplayUI()
    {
        gameplayComponents.Clear();
        
        // New Game Button
        var newGameButton = new Button(new Rectangle(650, 100, 120, 40), "New Game");
        newGameButton.OnClick += () => 
        {
            _gameStateManager.InitializeNewGame();
        };
        gameplayComponents.Add(newGameButton);

        // Options Button
        var optionsButton = new Button(new Rectangle(650, 160, 120, 40), "Options");
        optionsButton.OnClick += () => _gameStateManager.ChangeState(GameStateType.Options);
        gameplayComponents.Add(optionsButton);

        // Back to Menu Button
        var menuButton = new Button(new Rectangle(650, 220, 120, 40), "Main Menu");
        menuButton.OnClick += () => _gameStateManager.ChangeState(GameStateType.MainMenu);
        gameplayComponents.Add(menuButton);

        // Calcular tamaño y posición del panel de selección de valores
        int panelWidth = 180;
        int panelHeight = 180;
        int panelX = 650;  // Alineado con los botones
        int panelY = 280;  // Debajo de los botones
        
        // Crear panel de selección de valores
        _valueSelectionPanel = new ValueSelectionPanel(
            new Rectangle(panelX, panelY, panelWidth, panelHeight),
            GameConfig.Instance.GridSize,
            GameConfig.Instance.Representation);
        
        // Manejar el evento de selección de valor
        _valueSelectionPanel.OnValueSelected += (value) => {
            if (SudokuGrid.Instance.selectedRow >= 0 && SudokuGrid.Instance.selectedCol >= 0 && !SudokuGrid.Instance.Revealed[SudokuGrid.Instance.selectedRow, SudokuGrid.Instance.selectedCol])
            {
                SudokuGrid.Instance.PlaceNumber(SudokuGrid.Instance.selectedRow, SudokuGrid.Instance.selectedCol, value);
            }
        };
        
        gameplayComponents.Add(_valueSelectionPanel);
    }

    public void CreateGameOverUI()
    {
        gameOverComponents.Clear();
        
        // New Game Button
        var newGameButton = new Button(new Rectangle(300, 300, 200, 50), "New Game");
        newGameButton.OnClick += () => 
        {
            _gameStateManager.InitializeNewGame();
            _gameStateManager.ChangeState(GameStateType.Playing);
        };
        gameOverComponents.Add(newGameButton);

        // Back to Menu Button
        var menuButton = new Button(new Rectangle(300, 375, 200, 50), "Main Menu");
        menuButton.OnClick += () => _gameStateManager.ChangeState(GameStateType.MainMenu);
        gameOverComponents.Add(menuButton);
    }

    public void CreateOptionsUI()
    {
        optionsComponents.Clear();
        
        // Grid Size Dropdown
        var sizeLabel = new Button(new Rectangle(200, 150, 200, 30), "Grid Size:");
        sizeLabel.BackgroundColor = Color.Transparent;
        sizeLabel.TextColor = Color.Black;
        optionsComponents.Add(sizeLabel);
        
        var sizeOptions = new string[] { "4x4 (2x2)", "9x9 (3x3)" };
        var sizeDropdown = new Dropdown(new Rectangle(400, 150, 200, 30), sizeOptions, _gameConfig.Size - 2);
        sizeDropdown.OnSelectionChanged += (index) => {
            _gameConfig.Size = index + 2;
            // Actualizar el panel si ya existe
            UpdateValueSelectionPanel();
        };
        optionsComponents.Add(sizeDropdown);

        // Difficulty Dropdown
        var difficultyLabel = new Button(new Rectangle(200, 200, 200, 30), "Difficulty:");
        difficultyLabel.BackgroundColor = Color.Transparent;
        difficultyLabel.TextColor = Color.Black;
        optionsComponents.Add(difficultyLabel);
        
        var difficultyOptions = new string[] { "Easy", "Medium", "Hard", "Expert" };
        var difficultyDropdown = new Dropdown(new Rectangle(400, 200, 200, 30), difficultyOptions, _gameConfig.Difficulty - 1);
        difficultyDropdown.OnSelectionChanged += (index) => _gameConfig.Difficulty = index + 1;
        optionsComponents.Add(difficultyDropdown);

        // Style Dropdown
        var styleLabel = new Button(new Rectangle(200, 250, 200, 30), "Style:");
        styleLabel.BackgroundColor = Color.Transparent;
        styleLabel.TextColor = Color.Black;
        optionsComponents.Add(styleLabel);
        
        var styleOptions = new string[] { "Numbers", "Emojis", "Letters", "Colors" };
        var styleDropdown = new Dropdown(new Rectangle(400, 250, 200, 30), styleOptions, (int)_gameConfig.RepresentationType);
        styleDropdown.OnSelectionChanged += (index) => {
            _gameConfig.RepresentationType = (RepresentationFactory.RepresentationType)index;
            // Actualizar el panel si ya existe
            UpdateValueSelectionPanel();
        };
        optionsComponents.Add(styleDropdown);

        // Apply Button
        var applyButton = new Button(new Rectangle(250, 350, 150, 50), "Apply");
        applyButton.OnClick += () => 
        {
            if (_gameStateManager.gameInitialized)
            {
                _gameStateManager.InitializeNewGame();
                _gameStateManager.ChangeState(GameStateType.Playing);
            }
            else
            {
                _gameStateManager.ChangeState(GameStateType.MainMenu);
            }
        };
        optionsComponents.Add(applyButton);

        // Back Button
        var backButton = new Button(new Rectangle(425, 350, 150, 50), "Back");
        backButton.OnClick += () => _gameStateManager.ChangeState(GameStateType.MainMenu);
        optionsComponents.Add(backButton);
    }

    public void CreateMainMenuUI(Game game, SpriteFont font)
    {
        _mainMenuComponents.Clear();
        
        // Title
        var titleSize = font.MeasureString("SUDOKU");

        // Start Game Button
        var startButton = new Button(new Rectangle(300, 200, 200, 50), "Start Game");
        startButton.OnClick += () => 
        {
            _gameStateManager.InitializeNewGame();
            _gameStateManager.ChangeState(GameStateType.Playing);
        };
        _mainMenuComponents.Add(startButton);

        // Options Button
        var optionsButton = new Button(new Rectangle(300, 275, 200, 50), "Options");
        optionsButton.OnClick += () => _gameStateManager.ChangeState(GameStateType.Options);
        _mainMenuComponents.Add(optionsButton);

        // Exit Button
        var exitButton = new Button(new Rectangle(300, 350, 200, 50), "Exit");
        exitButton.OnClick += game.Exit;
        _mainMenuComponents.Add(exitButton);
    }

    public void UpdateValueSelectionPanel()
    {
        // Solo actualizar si estamos en modo de juego y el panel ya existe
        if (_valueSelectionPanel != null && _gameStateManager.CurrentState == GameStateType.Playing)
        {
            // Mantener la misma posición y tamaño, pero actualizar con los nuevos valores
            Rectangle bounds = _valueSelectionPanel.Bounds;
            
            // Crear un nuevo panel con la configuración actual
            _valueSelectionPanel = new ValueSelectionPanel(
                bounds,
                _gameConfig.GridSize,
                _gameConfig.Representation);
            
            // Restaurar el manejador de eventos
            _valueSelectionPanel.OnValueSelected += (value) => {
                if (SudokuGrid.Instance.selectedRow >= 0 && SudokuGrid.Instance.selectedCol >= 0 && !SudokuGrid.Instance.Revealed[SudokuGrid.Instance.selectedRow, SudokuGrid.Instance.selectedCol])
                {
                    SudokuGrid.Instance.PlaceNumber(SudokuGrid.Instance.selectedRow, SudokuGrid.Instance.selectedCol, value);
                }
            };
            
            // Reemplazar el panel antiguo en la lista de componentes
            for (int i = 0; i < gameplayComponents.Count; i++)
            {
                if (gameplayComponents[i] is ValueSelectionPanel)
                {
                    gameplayComponents[i] = _valueSelectionPanel;
                    break;
                }
            }
        }
    }
}
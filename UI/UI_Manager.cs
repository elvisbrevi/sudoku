using System.Collections.Generic;
using monogame_test.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

public sealed class UI_Manager
{
    private List<UIComponent> _mainMenuComponents = new List<UIComponent>();
    public List<UIComponent> optionsComponents = new List<UIComponent>();
    public List<UIComponent> gameplayComponents = new List<UIComponent>();
    private ValueSelectionPanel _valueSelectionPanel;
    public List<UIComponent> gameOverComponents = new List<UIComponent>();
    private SpriteFont _font;

    private Game _game;
    private GameStateManager _gameStateManager = GameStateManager.GetInstance();
    private SudokuGrid _sudoGrid = SudokuGrid.GetInstance();
    private GameConfig _gameConfig = GameConfig.GetInstance();
    private static UI_Manager _instance;
    private UI_Manager() { }

    public static UI_Manager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new UI_Manager();
        }
        return _instance;
    }

    public void Load(Game game, SpriteFont font)
    {
        _font = font;
        _game = game;
        CreateMainMenuUI();
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
            _gameConfig.GridSize,
            _gameConfig.Representation);

        // Manejar el evento de selección de valor
        _valueSelectionPanel.OnValueSelected += (value) =>
        {
            if (_sudoGrid.selectedRow >= 0 && _sudoGrid.selectedCol >= 0 && !_sudoGrid.Revealed[_sudoGrid.selectedRow, _sudoGrid.selectedCol])
            {
                _sudoGrid.PlaceNumber(_sudoGrid.selectedRow, _sudoGrid.selectedCol, value);
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
        sizeDropdown.OnSelectionChanged += (index) =>
        {
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
        styleDropdown.OnSelectionChanged += (index) =>
        {
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

    public void CreateMainMenuUI()
    {
        _mainMenuComponents.Clear();

        // Title
        var titleSize = _font.MeasureString("SUDOKU");

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
        exitButton.OnClick += _game.Exit;
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
            _valueSelectionPanel.OnValueSelected += (value) =>
            {
                if (_sudoGrid.selectedRow >= 0 && _sudoGrid.selectedCol >= 0 && !_sudoGrid.Revealed[_sudoGrid.selectedRow, _sudoGrid.selectedCol])
                {
                    _sudoGrid.PlaceNumber(_sudoGrid.selectedRow, _sudoGrid.selectedCol, value);
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

    public void DrawGameOver(SpriteBatch spriteBatch)
    {
        // Draw title
        string title = "PUZZLE SOLVED!";
        Vector2 titleSize = _font.MeasureString(title);
        spriteBatch.DrawString(_font, title, new Vector2(400 - titleSize.X / 2, 150), Color.Green, 0f, Vector2.Zero, 1.8f, SpriteEffects.None, 0f);

        // Draw time and score info (if implemented)
        string message = "Congratulations! You've completed the puzzle.";
        Vector2 messageSize = _font.MeasureString(message);
        spriteBatch.DrawString(_font, message, new Vector2(400 - messageSize.X / 2, 220), Color.Black);

        // Draw UI components
        foreach (var component in gameOverComponents)
        {
            component.Draw(spriteBatch, _font);
        }
    }

    public void DrawMainMenu(SpriteBatch spriteBatch)
    {
        // Draw title
        string title = "SUDOKU";
        Vector2 titleSize = _font.MeasureString(title);
        spriteBatch.DrawString(_font, title, new Vector2(400 - titleSize.X / 2, 100), Color.DarkBlue, 0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0f);

        // Draw UI components
        foreach (var component in _mainMenuComponents)
        {
            component.Draw(spriteBatch, _font);
        }
    }

    public void DrawOptions(SpriteBatch spriteBatch)
    {
        // Draw title
        string title = "OPTIONS";
        Vector2 titleSize = _font.MeasureString(title);
        spriteBatch.DrawString(_font, title, new Vector2(400 - titleSize.X / 2, 80), Color.DarkBlue, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);

        // Dibujar primero los fondos y los bordes de todos los componentes
        foreach (var component in optionsComponents)
        {
            if (component is Dropdown dropdown)
            {
                // Solo dibujamos el fondo y el borde del dropdown, no su contenido desplegado
                dropdown.DrawBackground(spriteBatch, _font);
            }
            else
            {
                // Componentes normales se dibujan completamente
                component.Draw(spriteBatch, _font);
            }
        }

        // Luego dibujamos el contenido de los dropdowns por encima de todo
        foreach (var component in optionsComponents)
        {
            if (component is Dropdown dropdown)
            {
                dropdown.DrawContent(spriteBatch, _font);
            }
        }
    }
}
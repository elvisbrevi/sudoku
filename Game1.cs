using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using monogame_test.Models;

namespace monogame_test
{
    public class Game1 : Game
    {
        // Graphics and Assets
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        // Game State
        private GameStateManager _gameStateManager;
        private GameConfig _gameConfig;
        private SudokuGrid _sudokuGrid;
        private int _selectedRow = -1;
        private int _selectedCol = -1;
        private MouseState _prevMouseState;
        private KeyboardState _prevKeyboardState;
        private bool _gameInitialized = false;
        private bool _puzzleSolved = false;

        // UI Elements
        private List<UIComponent> _mainMenuComponents = new List<UIComponent>();
        private List<UIComponent> _optionsComponents = new List<UIComponent>();
        private List<UIComponent> _gameplayComponents = new List<UIComponent>();
        private List<UIComponent> _gameOverComponents = new List<UIComponent>();
        
        // Constants for drawing
        private const int CellSize = 40;
        private const int GridMargin = 50;
        private readonly Color GridLineColor = Color.Black;
        private readonly Color SelectedCellColor = new Color(173, 216, 230, 200); // Light blue with more opacity
        private readonly Color SelectedCellBorderColor = Color.Blue; // Bright blue for selected cell border
        private readonly Color OriginalCellColor = new Color(211, 211, 211, 150);  // Light gray
        private readonly Color CorrectCellColor = new Color(144, 238, 144, 150);   // Light green
        private readonly Color IncorrectCellColor = new Color(255, 182, 193, 150); // Light red

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        // Initialize singletons
        _gameStateManager = GameStateManager.Instance;
        _gameConfig = GameConfig.Instance;
    }

    protected override void Initialize()
    {
        // Set window size
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 600;
        _graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("DefaultFont");
        
        // Create UI components
        CreateMainMenuUI();
        CreateOptionsUI();
        CreateGameplayUI();
        CreateGameOverUI();
    }

    private void CreateMainMenuUI()
    {
        _mainMenuComponents.Clear();
        
        // Title
        var titleSize = _font.MeasureString("SUDOKU");

        // Start Game Button
        var startButton = new Button(new Rectangle(300, 200, 200, 50), "Start Game");
        startButton.OnClick += () => 
        {
            InitializeNewGame();
            _gameStateManager.ChangeState(GameStateType.Playing);
        };
        _mainMenuComponents.Add(startButton);

        // Options Button
        var optionsButton = new Button(new Rectangle(300, 275, 200, 50), "Options");
        optionsButton.OnClick += () => _gameStateManager.ChangeState(GameStateType.Options);
        _mainMenuComponents.Add(optionsButton);

        // Exit Button
        var exitButton = new Button(new Rectangle(300, 350, 200, 50), "Exit");
        exitButton.OnClick += Exit;
        _mainMenuComponents.Add(exitButton);
    }

    private void CreateOptionsUI()
    {
        _optionsComponents.Clear();
        
        // Grid Size Dropdown
        var sizeLabel = new Button(new Rectangle(200, 150, 200, 30), "Grid Size:");
        sizeLabel.BackgroundColor = Color.Transparent;
        sizeLabel.TextColor = Color.Black;
        _optionsComponents.Add(sizeLabel);
        
        var sizeOptions = new string[] { "4x4 (2x2)", "9x9 (3x3)" };
        var sizeDropdown = new Dropdown(new Rectangle(400, 150, 200, 30), sizeOptions, _gameConfig.Size - 2);
        sizeDropdown.OnSelectionChanged += (index) => _gameConfig.Size = index + 2;
        _optionsComponents.Add(sizeDropdown);

        // Difficulty Dropdown
        var difficultyLabel = new Button(new Rectangle(200, 200, 200, 30), "Difficulty:");
        difficultyLabel.BackgroundColor = Color.Transparent;
        difficultyLabel.TextColor = Color.Black;
        _optionsComponents.Add(difficultyLabel);
        
        var difficultyOptions = new string[] { "Easy", "Medium", "Hard", "Expert" };
        var difficultyDropdown = new Dropdown(new Rectangle(400, 200, 200, 30), difficultyOptions, _gameConfig.Difficulty - 1);
        difficultyDropdown.OnSelectionChanged += (index) => _gameConfig.Difficulty = index + 1;
        _optionsComponents.Add(difficultyDropdown);

        // Style Dropdown
        var styleLabel = new Button(new Rectangle(200, 250, 200, 30), "Style:");
        styleLabel.BackgroundColor = Color.Transparent;
        styleLabel.TextColor = Color.Black;
        _optionsComponents.Add(styleLabel);
        
        var styleOptions = new string[] { "Numbers", "Emojis", "Letters", "Colors" };
        var styleDropdown = new Dropdown(new Rectangle(400, 250, 200, 30), styleOptions, (int)_gameConfig.RepresentationType);
        styleDropdown.OnSelectionChanged += (index) => _gameConfig.RepresentationType = (RepresentationFactory.RepresentationType)index;
        _optionsComponents.Add(styleDropdown);

        // Apply Button
        var applyButton = new Button(new Rectangle(250, 350, 150, 50), "Apply");
        applyButton.OnClick += () => 
        {
            if (_gameInitialized)
            {
                InitializeNewGame();
                _gameStateManager.ChangeState(GameStateType.Playing);
            }
            else
            {
                _gameStateManager.ChangeState(GameStateType.MainMenu);
            }
        };
        _optionsComponents.Add(applyButton);

        // Back Button
        var backButton = new Button(new Rectangle(425, 350, 150, 50), "Back");
        backButton.OnClick += () => _gameStateManager.ChangeState(GameStateType.MainMenu);
        _optionsComponents.Add(backButton);
    }

    private void CreateGameplayUI()
    {
        _gameplayComponents.Clear();
        
        // New Game Button
        var newGameButton = new Button(new Rectangle(650, 100, 120, 40), "New Game");
        newGameButton.OnClick += () => 
        {
            InitializeNewGame();
        };
        _gameplayComponents.Add(newGameButton);

        // Options Button
        var optionsButton = new Button(new Rectangle(650, 160, 120, 40), "Options");
        optionsButton.OnClick += () => _gameStateManager.ChangeState(GameStateType.Options);
        _gameplayComponents.Add(optionsButton);

        // Back to Menu Button
        var menuButton = new Button(new Rectangle(650, 220, 120, 40), "Main Menu");
        menuButton.OnClick += () => _gameStateManager.ChangeState(GameStateType.MainMenu);
        _gameplayComponents.Add(menuButton);
    }

    private void CreateGameOverUI()
    {
        _gameOverComponents.Clear();
        
        // New Game Button
        var newGameButton = new Button(new Rectangle(300, 300, 200, 50), "New Game");
        newGameButton.OnClick += () => 
        {
            InitializeNewGame();
            _gameStateManager.ChangeState(GameStateType.Playing);
        };
        _gameOverComponents.Add(newGameButton);

        // Back to Menu Button
        var menuButton = new Button(new Rectangle(300, 375, 200, 50), "Main Menu");
        menuButton.OnClick += () => _gameStateManager.ChangeState(GameStateType.MainMenu);
        _gameOverComponents.Add(menuButton);
    }

    private void InitializeNewGame()
    {
        _sudokuGrid = new SudokuGrid(_gameConfig.Size);
        _sudokuGrid.CreatePuzzle(_gameConfig.Difficulty);
        _selectedRow = -1;
        _selectedCol = -1;
        _puzzleSolved = false;
        _gameInitialized = true;
    }

    protected override void Update(GameTime gameTime)
    {
        // Exit condition
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            if (_gameStateManager.CurrentState == GameStateType.Playing)
            {
                _gameStateManager.ChangeState(GameStateType.MainMenu);
            }
            else if (_gameStateManager.CurrentState == GameStateType.Options)
            {
                _gameStateManager.ChangeState(GameStateType.MainMenu);
            }
            else
            {
                Exit();
            }
        }

        // Get current input states
        var mouseState = Mouse.GetState();
        var keyboardState = Keyboard.GetState();

        // Update UI based on game state
        switch (_gameStateManager.CurrentState)
        {
            case GameStateType.MainMenu:
                UpdateMainMenu(mouseState);
                break;
            case GameStateType.Options:
                UpdateOptions(mouseState);
                break;
            case GameStateType.Playing:
                UpdateGameplay(mouseState, keyboardState);
                break;
            case GameStateType.GameOver:
                UpdateGameOver(mouseState);
                break;
        }

        // Store previous input states
        _prevMouseState = mouseState;
        _prevKeyboardState = keyboardState;

        base.Update(gameTime);
    }

    private void UpdateMainMenu(MouseState mouseState)
    {
        foreach (var component in _mainMenuComponents)
        {
            component.Update(mouseState, _prevMouseState);
        }
    }

    private void UpdateOptions(MouseState mouseState)
    {
        foreach (var component in _optionsComponents)
        {
            component.Update(mouseState, _prevMouseState);
        }
    }

    private void UpdateGameplay(MouseState mouseState, KeyboardState keyboardState)
    {
        // Update UI components
        foreach (var component in _gameplayComponents)
        {
            component.Update(mouseState, _prevMouseState);
        }

        if (_gameInitialized && !_puzzleSolved)
        {
            // Check if puzzle is solved
            if (_sudokuGrid.IsSolved())
            {
                _puzzleSolved = true;
                _gameStateManager.ChangeState(GameStateType.GameOver);
                return;
            }

            // Handle mouse selection of cells
            HandleCellSelection(mouseState);

            // Handle keyboard input for numbers
            HandleNumberInput(keyboardState);
        }
    }

    private void UpdateGameOver(MouseState mouseState)
    {
        foreach (var component in _gameOverComponents)
        {
            component.Update(mouseState, _prevMouseState);
        }
    }

    private void HandleCellSelection(MouseState mouseState)
    {
        if (mouseState.LeftButton == ButtonState.Released && _prevMouseState.LeftButton == ButtonState.Pressed)
        {
            // Calculate grid boundaries
            int gridSizePixels = _sudokuGrid.GridSize * CellSize;
            Rectangle gridBounds = new Rectangle(GridMargin, GridMargin, gridSizePixels, gridSizePixels);

            if (gridBounds.Contains(mouseState.Position))
            {
                // Calculate selected cell
                int col = (mouseState.X - GridMargin) / CellSize;
                int row = (mouseState.Y - GridMargin) / CellSize;

                // Ensure within bounds
                if (row >= 0 && row < _sudokuGrid.GridSize && col >= 0 && col < _sudokuGrid.GridSize)
                {
                    // Can't select revealed cells
                    if (!_sudokuGrid.Revealed[row, col])
                    {
                        _selectedRow = row;
                        _selectedCol = col;
                    }
                    else
                    {
                        // When clicking on revealed cell, deselect
                        _selectedRow = -1;
                        _selectedCol = -1;
                    }
                }
            }
            else
            {
                // When clicking outside grid, deselect
                _selectedRow = -1;
                _selectedCol = -1;
            }
        }
    }

    private void HandleArrowKeyNavigation(KeyboardState keyboardState)
    {
        // Check if arrow keys are pressed to navigate between cells
        if (keyboardState.IsKeyDown(Keys.Left) && !_prevKeyboardState.IsKeyDown(Keys.Left))
        {
            // Move selection left, wrap around to the end of the previous row if needed
            if (_selectedCol > 0)
            {
                _selectedCol--;
            }
            else if (_selectedRow > 0)
            {
                _selectedRow--;
                _selectedCol = _sudokuGrid.GridSize - 1;
            }
            
            // Skip revealed cells (original clues)
            while (_selectedRow >= 0 && _selectedCol >= 0 && _sudokuGrid.Revealed[_selectedRow, _selectedCol])
            {
                // Continue moving left or to previous row if needed
                if (_selectedCol > 0)
                {
                    _selectedCol--;
                }
                else if (_selectedRow > 0)
                {
                    _selectedRow--;
                    _selectedCol = _sudokuGrid.GridSize - 1;
                }
                else
                {
                    break; // Can't move anymore
                }
            }
        }
        else if (keyboardState.IsKeyDown(Keys.Right) && !_prevKeyboardState.IsKeyDown(Keys.Right))
        {
            // Move selection right, wrap around to the start of the next row if needed
            if (_selectedCol < _sudokuGrid.GridSize - 1)
            {
                _selectedCol++;
            }
            else if (_selectedRow < _sudokuGrid.GridSize - 1)
            {
                _selectedRow++;
                _selectedCol = 0;
            }
            
            // Skip revealed cells (original clues)
            while (_selectedRow >= 0 && _selectedCol >= 0 && 
                   _selectedRow < _sudokuGrid.GridSize && _selectedCol < _sudokuGrid.GridSize && 
                   _sudokuGrid.Revealed[_selectedRow, _selectedCol])
            {
                // Continue moving right or to next row if needed
                if (_selectedCol < _sudokuGrid.GridSize - 1)
                {
                    _selectedCol++;
                }
                else if (_selectedRow < _sudokuGrid.GridSize - 1)
                {
                    _selectedRow++;
                    _selectedCol = 0;
                }
                else
                {
                    break; // Can't move anymore
                }
            }
        }
        else if (keyboardState.IsKeyDown(Keys.Up) && !_prevKeyboardState.IsKeyDown(Keys.Up))
        {
            // Move selection up, stay in the same column
            if (_selectedRow > 0)
            {
                _selectedRow--;
            }
            
            // Skip revealed cells (original clues)
            while (_selectedRow >= 0 && _sudokuGrid.Revealed[_selectedRow, _selectedCol])
            {
                if (_selectedRow > 0)
                {
                    _selectedRow--;
                }
                else
                {
                    break; // Can't move anymore
                }
            }
        }
        else if (keyboardState.IsKeyDown(Keys.Down) && !_prevKeyboardState.IsKeyDown(Keys.Down))
        {
            // Move selection down, stay in the same column
            if (_selectedRow < _sudokuGrid.GridSize - 1)
            {
                _selectedRow++;
            }
            
            // Skip revealed cells (original clues)
            while (_selectedRow < _sudokuGrid.GridSize && _sudokuGrid.Revealed[_selectedRow, _selectedCol])
            {
                if (_selectedRow < _sudokuGrid.GridSize - 1)
                {
                    _selectedRow++;
                }
                else
                {
                    break; // Can't move anymore
                }
            }
        }
        
        // If no cell is selected yet, select the first non-revealed cell
        if (_selectedRow < 0 || _selectedCol < 0)
        {
            for (int row = 0; row < _sudokuGrid.GridSize; row++)
            {
                for (int col = 0; col < _sudokuGrid.GridSize; col++)
                {
                    if (!_sudokuGrid.Revealed[row, col])
                    {
                        _selectedRow = row;
                        _selectedCol = col;
                        return;
                    }
                }
            }
        }
    }

    private void HandleNumberInput(KeyboardState keyboardState)
    {
        // Handle arrow key navigation for cell selection
        HandleArrowKeyNavigation(keyboardState);

        // Only process number input if a cell is selected
        if (_selectedRow >= 0 && _selectedCol >= 0)
        {
            // Check for number keys
            for (int i = 0; i <= _sudokuGrid.GridSize; i++)
            {
                Keys key = i == 0 ? Keys.D0 : (Keys)((int)Keys.D1 + i - 1);
                Keys numPadKey = i == 0 ? Keys.NumPad0 : (Keys)((int)Keys.NumPad1 + i - 1);

                if ((keyboardState.IsKeyDown(key) && !_prevKeyboardState.IsKeyDown(key)) ||
                    (keyboardState.IsKeyDown(numPadKey) && !_prevKeyboardState.IsKeyDown(numPadKey)))
                {
                    // Place the number in the grid if it's within valid range
                    if (i <= _sudokuGrid.GridSize)
                    {
                        _sudokuGrid.PlaceNumber(_selectedRow, _selectedCol, i);
                    }
                }
            }

            // Check for delete or backspace to clear a cell
            if ((keyboardState.IsKeyDown(Keys.Delete) && !_prevKeyboardState.IsKeyDown(Keys.Delete)) ||
                (keyboardState.IsKeyDown(Keys.Back) && !_prevKeyboardState.IsKeyDown(Keys.Back)))
            {
                _sudokuGrid.PlaceNumber(_selectedRow, _selectedCol, 0);
            }
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();

        // Draw based on current game state
        switch (_gameStateManager.CurrentState)
        {
            case GameStateType.MainMenu:
                DrawMainMenu();
                break;
            case GameStateType.Options:
                DrawOptions();
                break;
            case GameStateType.Playing:
                DrawGameplay();
                break;
            case GameStateType.GameOver:
                DrawGameOver();
                break;
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawMainMenu()
    {
        // Draw title
        string title = "SUDOKU";
        Vector2 titleSize = _font.MeasureString(title);
        _spriteBatch.DrawString(_font, title, new Vector2(400 - titleSize.X / 2, 100), Color.DarkBlue, 0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0f);

        // Draw UI components
        foreach (var component in _mainMenuComponents)
        {
            component.Draw(_spriteBatch, _font);
        }
    }

    private void DrawOptions()
    {
        // Draw title
        string title = "OPTIONS";
        Vector2 titleSize = _font.MeasureString(title);
        _spriteBatch.DrawString(_font, title, new Vector2(400 - titleSize.X / 2, 80), Color.DarkBlue, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);

        // Draw UI components
        foreach (var component in _optionsComponents)
        {
            component.Draw(_spriteBatch, _font);
        }
    }

    private void DrawGameplay()
    {
        if (!_gameInitialized) return;

        // Draw grid
        DrawSudokuGrid();
        
        // Draw UI components
        foreach (var component in _gameplayComponents)
        {
            component.Draw(_spriteBatch, _font);
        }
    }

    private void DrawGameOver()
    {
        // Draw title
        string title = "PUZZLE SOLVED!";
        Vector2 titleSize = _font.MeasureString(title);
        _spriteBatch.DrawString(_font, title, new Vector2(400 - titleSize.X / 2, 150), Color.Green, 0f, Vector2.Zero, 1.8f, SpriteEffects.None, 0f);

        // Draw time and score info (if implemented)
        string message = "Congratulations! You've completed the puzzle.";
        Vector2 messageSize = _font.MeasureString(message);
        _spriteBatch.DrawString(_font, message, new Vector2(400 - messageSize.X / 2, 220), Color.Black);

        // Draw UI components
        foreach (var component in _gameOverComponents)
        {
            component.Draw(_spriteBatch, _font);
        }
    }

    private void DrawSudokuGrid()
    {
        // Grid dimensions
        int gridSize = _sudokuGrid.GridSize;
        int gridSizePixels = gridSize * CellSize;
        
        // Draw background
        var gridBackground = new Rectangle(GridMargin - 1, GridMargin - 1, gridSizePixels + 2, gridSizePixels + 2);
        _spriteBatch.Draw(new Texture2D(GraphicsDevice, 1, 1), gridBackground, Color.DarkGray);

        // Draw cells
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                // Cell rectangle
                Rectangle cellRect = new Rectangle(
                    GridMargin + col * CellSize, 
                    GridMargin + row * CellSize, 
                    CellSize, 
                    CellSize);
                
                // Cell background
                Color cellBackground = Color.White;
                
                // Apply cell coloring
                if (row == _selectedRow && col == _selectedCol)
                {
                    cellBackground = SelectedCellColor;
                }
                else if (_sudokuGrid.Revealed[row, col])
                {
                    cellBackground = OriginalCellColor;
                }
                else if (_sudokuGrid.Grid[row, col] != 0)
                {
                    // Check if value is correct
                    if (_sudokuGrid.IsValueCorrect(row, col, _sudokuGrid.Grid[row, col]))
                    {
                        cellBackground = CorrectCellColor;
                    }
                    else
                    {
                        cellBackground = IncorrectCellColor;
                    }
                }
                
                // Draw cell background
                _spriteBatch.Draw(new Texture2D(GraphicsDevice, 1, 1), cellRect, cellBackground);
                
                // Draw cell border
                if (row == _selectedRow && col == _selectedCol)
                {
                    // Draw a thicker, colored border for the selected cell
                    DrawRectangle(cellRect, SelectedCellBorderColor, 3);
                }
                else
                {
                    // Normal border for non-selected cells
                    DrawRectangle(cellRect, GridLineColor, 1);
                }
                
                // Only draw cell values that should be visible to the player:
                // 1. Revealed cells (initial clues) 
                // 2. Cells modified by the player
                // 3. All cells if the puzzle is solved
                if (_sudokuGrid.Grid[row, col] != 0 && 
                    (_sudokuGrid.Revealed[row, col] || _sudokuGrid.PlayerModified[row, col] || _puzzleSolved))
                {
                    string cellText = _gameConfig.Representation.GetRepresentation(_sudokuGrid.Grid[row, col]);
                    Vector2 textSize = _font.MeasureString(cellText);
                    Vector2 textPos = new Vector2(
                        GridMargin + col * CellSize + (CellSize - textSize.X) / 2,
                        GridMargin + row * CellSize + (CellSize - textSize.Y) / 2
                    );
                    
                    _spriteBatch.DrawString(_font, cellText, textPos, Color.Black);
                }
            }
        }
        
        // Draw thicker lines for box boundaries
        for (int i = 0; i <= gridSize; i++)
        {
            int thickness = (i % _sudokuGrid.BoxSize == 0) ? 2 : 1;
            Color lineColor = (i % _sudokuGrid.BoxSize == 0) ? Color.Black : GridLineColor;
            
            // Horizontal lines
            DrawLine(
                new Vector2(GridMargin, GridMargin + i * CellSize),
                new Vector2(GridMargin + gridSizePixels, GridMargin + i * CellSize),
                lineColor,
                thickness
            );
            
            // Vertical lines
            DrawLine(
                new Vector2(GridMargin + i * CellSize, GridMargin),
                new Vector2(GridMargin + i * CellSize, GridMargin + gridSizePixels),
                lineColor,
                thickness
            );
        }
    }

    private void DrawRectangle(Rectangle rectangle, Color color, int thickness)
    {
        // Top line
        DrawLine(
            new Vector2(rectangle.Left, rectangle.Top),
            new Vector2(rectangle.Right, rectangle.Top),
            color,
            thickness
        );
        
        // Right line
        DrawLine(
            new Vector2(rectangle.Right, rectangle.Top),
            new Vector2(rectangle.Right, rectangle.Bottom),
            color,
            thickness
        );
        
        // Bottom line
        DrawLine(
            new Vector2(rectangle.Left, rectangle.Bottom),
            new Vector2(rectangle.Right, rectangle.Bottom),
            color,
            thickness
        );
        
        // Left line
        DrawLine(
            new Vector2(rectangle.Left, rectangle.Top),
            new Vector2(rectangle.Left, rectangle.Bottom),
            color,
            thickness
        );
    }

    private void DrawLine(Vector2 start, Vector2 end, Color color, int thickness)
    {
        var texture = new Texture2D(GraphicsDevice, 1, 1);
        texture.SetData(new[] { Color.White });
        
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        float length = edge.Length();
        
        _spriteBatch.Draw(
            texture,
            start,
            null,
            color,
            angle,
            Vector2.Zero,
            new Vector2(length, thickness),
            SpriteEffects.None,
            0
        );
    }
}
}

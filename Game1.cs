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
        private List<UIComponent> _gameplayComponents = new List<UIComponent>();

        // Game Assets
        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        // Game State
        private GameStateManager _gameStateManager;
        private SudokuGrid _sudokuGrid;
        private MouseState _prevMouseState;
        private KeyboardState _prevKeyboardState;
        private UI_Manager _uiManager;
        private GameConfig _gameConfig;

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
            _gameStateManager = GameStateManager.GetInstance();
            _uiManager = UI_Manager.GetInstance();
            _gameConfig = GameConfig.GetInstance();
            _sudokuGrid = SudokuGrid.GetInstance();
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
            _uiManager.Load(this, _font);
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
                    _uiManager.UpdateMainMenu(mouseState, _prevMouseState);
                    break;
                case GameStateType.Options:
                    _uiManager.UpdateOptions(mouseState, _prevMouseState);
                    break;
                case GameStateType.Playing:
                    UpdateGameplay(mouseState, keyboardState);
                    break;
                case GameStateType.GameOver:
                    _uiManager.UpdateGameOver(mouseState, _prevMouseState);
                    break;
            }

            // Store previous input states
            _prevMouseState = mouseState;
            _prevKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        private void UpdateGameplay(MouseState mouseState, KeyboardState keyboardState)
        {
            // Crear una copia de la lista para evitar errores si se modifica durante la iteración
            var componentsCopy = new List<UIComponent>(_gameplayComponents);

            // Update UI components from copy
            foreach (var component in componentsCopy)
            {
                component.Update(mouseState, _prevMouseState);
            }

            if (_gameStateManager.gameInitialized && !_gameStateManager.puzzleSolved)
            {
                // Check if puzzle is solved
                if (_sudokuGrid.IsSolved())
                {
                    _gameStateManager.puzzleSolved = true;
                    _gameStateManager.ChangeState(GameStateType.GameOver);
                    return;
                }

                // Handle mouse selection of cells
                HandleCellSelection(mouseState);

                // Handle keyboard navigation with arrow keys
                HandleArrowKeyNavigation(keyboardState);

                // Handle keyboard input for numbers
                HandleNumberInput(keyboardState);
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
                            _sudokuGrid.selectedRow = row;
                            _sudokuGrid.selectedCol = col;
                        }
                        else
                        {
                            // When clicking on revealed cell, deselect
                            _sudokuGrid.selectedRow = -1;
                            _sudokuGrid.selectedCol = -1;
                        }
                    }
                }
                else
                {
                    // When clicking outside grid, deselect
                    _sudokuGrid.selectedRow = -1;
                    _sudokuGrid.selectedCol = -1;
                }
            }
        }

        private void HandleArrowKeyNavigation(KeyboardState keyboardState)
        {
            int gridSize = _sudokuGrid.GridSize;
            int moveDirection = -1; // 0=left, 1=right, 2=up, 3=down, -1=none

            // Check which arrow key was pressed - very optimized for speed
            if (keyboardState.IsKeyDown(Keys.Left) && !_prevKeyboardState.IsKeyDown(Keys.Left))
                moveDirection = 0;
            else if (keyboardState.IsKeyDown(Keys.Right) && !_prevKeyboardState.IsKeyDown(Keys.Right))
                moveDirection = 1;
            else if (keyboardState.IsKeyDown(Keys.Up) && !_prevKeyboardState.IsKeyDown(Keys.Up))
                moveDirection = 2;
            else if (keyboardState.IsKeyDown(Keys.Down) && !_prevKeyboardState.IsKeyDown(Keys.Down))
                moveDirection = 3;

            // If no movement, exit early
            if (moveDirection == -1) return;

            // Initialize selection if needed
            if (_sudokuGrid.selectedRow < 0 || _sudokuGrid.selectedCol < 0)
            {
                InitializeSelection();
                return;
            }

            // Perform direct movement based on direction
            switch (moveDirection)
            {
                case 0: // Left
                    _sudokuGrid.selectedCol = (_sudokuGrid.selectedCol > 0) ? _sudokuGrid.selectedCol - 1 : gridSize - 1;
                    break;
                case 1: // Right
                    _sudokuGrid.selectedCol = (_sudokuGrid.selectedCol < gridSize - 1) ? _sudokuGrid.selectedCol + 1 : 0;
                    break;
                case 2: // Up
                    _sudokuGrid.selectedRow = (_sudokuGrid.selectedRow > 0) ? _sudokuGrid.selectedRow - 1 : gridSize - 1;
                    break;
                case 3: // Down
                    _sudokuGrid.selectedRow = (_sudokuGrid.selectedRow < gridSize - 1) ? _sudokuGrid.selectedRow + 1 : 0;
                    break;
            }

            // If we land on a revealed cell, find next non-revealed cell
            if (_sudokuGrid.Revealed[_sudokuGrid.selectedRow, _sudokuGrid.selectedCol])
            {
                // Skip revealed cells in the same direction
                FindNextNonRevealedCell(moveDirection, gridSize);
            }
        }

        // Extremadamente optimizado para velocidad de navegaciu00f3n
        private void FindNextNonRevealedCell(int moveDirection, int gridSize)
        {
            int startRow = _sudokuGrid.selectedRow;
            int startCol = _sudokuGrid.selectedCol;

            // Continuar moviendo en la misma direcciu00f3n hasta encontrar una celda no revelada o dar la vuelta completa
            do
            {
                // Mover en la direcciu00f3n indicada con envoltura (wrapping)
                switch (moveDirection)
                {
                    case 0: // Izquierda
                        _sudokuGrid.selectedCol = (_sudokuGrid.selectedCol > 0) ? _sudokuGrid.selectedCol - 1 : gridSize - 1;
                        break;
                    case 1: // Derecha
                        _sudokuGrid.selectedCol = (_sudokuGrid.selectedCol < gridSize - 1) ? _sudokuGrid.selectedCol + 1 : 0;
                        break;
                    case 2: // Arriba
                        _sudokuGrid.selectedRow = (_sudokuGrid.selectedRow > 0) ? _sudokuGrid.selectedRow - 1 : gridSize - 1;
                        break;
                    case 3: // Abajo
                        _sudokuGrid.selectedRow = (_sudokuGrid.selectedRow < gridSize - 1) ? _sudokuGrid.selectedRow + 1 : 0;
                        break;
                }

                // Verificar si encontramos una celda no revelada
                if (!_sudokuGrid.Revealed[_sudokuGrid.selectedRow, _sudokuGrid.selectedCol])
                {
                    return; // Encontramos una celda vu00e1lida, salir
                }

                // Verificar si hemos dado una vuelta completa
            } while (_sudokuGrid.selectedRow != startRow || _sudokuGrid.selectedCol != startCol);

            // Si no encontramos ninguna celda no revelada en la direcciu00f3n actual, buscar en todo el tablero
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    if (!_sudokuGrid.Revealed[row, col])
                    {
                        _sudokuGrid.selectedRow = row;
                        _sudokuGrid.selectedCol = col;
                        return;
                    }
                }
            }
        }

        // If no cell is selected yet, select the first non-revealed cell
        private void InitializeSelection()
        {
            if (_sudokuGrid.selectedRow < 0 || _sudokuGrid.selectedCol < 0)
            {
                for (int row = 0; row < _sudokuGrid.GridSize; row++)
                {
                    for (int col = 0; col < _sudokuGrid.GridSize; col++)
                    {
                        if (!_sudokuGrid.Revealed[row, col])
                        {
                            _sudokuGrid.selectedRow = row;
                            _sudokuGrid.selectedCol = col;
                            return;
                        }
                    }
                }
            }
        }

        private void HandleNumberInput(KeyboardState keyboardState)
        {
            // Only process number input if a valid cell is selected (not revealed)
            if (_sudokuGrid.selectedRow >= 0 && _sudokuGrid.selectedCol >= 0 && !_sudokuGrid.Revealed[_sudokuGrid.selectedRow, _sudokuGrid.selectedCol])
            {
                // Check for number keys (1-9 for 9x9 grid, 1-4 for 4x4 grid)
                for (int i = 1; i <= _sudokuGrid.GridSize; i++) // Start from 1, not 0
                {
                    // Check both regular number keys and numpad keys
                    Keys key = (Keys)((int)Keys.D1 + i - 1); // D1 is 1, D2 is 2, etc.
                    Keys numPadKey = (Keys)((int)Keys.NumPad1 + i - 1);

                    if ((keyboardState.IsKeyDown(key) && !_prevKeyboardState.IsKeyDown(key)) ||
                        (keyboardState.IsKeyDown(numPadKey) && !_prevKeyboardState.IsKeyDown(numPadKey)))
                    {
                        // Place the number in the grid
                        _sudokuGrid.PlaceNumber(_sudokuGrid.selectedRow, _sudokuGrid.selectedCol, i);
                        // Add sound or visual feedback here if desired
                        break; // Exit after processing a number key
                    }
                }

                // Check for delete, backspace, or the number 0 to clear a cell
                if ((keyboardState.IsKeyDown(Keys.Delete) && !_prevKeyboardState.IsKeyDown(Keys.Delete)) ||
                    (keyboardState.IsKeyDown(Keys.Back) && !_prevKeyboardState.IsKeyDown(Keys.Back)) ||
                    (keyboardState.IsKeyDown(Keys.D0) && !_prevKeyboardState.IsKeyDown(Keys.D0)) ||
                    (keyboardState.IsKeyDown(Keys.NumPad0) && !_prevKeyboardState.IsKeyDown(Keys.NumPad0)))
                {
                    _sudokuGrid.PlaceNumber(_sudokuGrid.selectedRow, _sudokuGrid.selectedCol, 0);
                    // Add sound or visual feedback for clearing a cell
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
                    _uiManager.DrawMainMenu(_spriteBatch);
                    break;
                case GameStateType.Options:
                    _uiManager.DrawOptions(_spriteBatch);
                    break;
                case GameStateType.Playing:
                    DrawGameplay();
                    break;
                case GameStateType.GameOver:
                    _uiManager.DrawGameOver(_spriteBatch);
                    break;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawGameplay()
        {
            if (!_gameStateManager.gameInitialized) return;

            // Draw grid
            DrawSudokuGrid();

            // Dibujamos todos los componentes
            foreach (var component in _uiManager.gameplayComponents)
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
                    if (row == _sudokuGrid.selectedRow && col == _sudokuGrid.selectedCol)
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
                    if (row == _sudokuGrid.selectedRow && col == _sudokuGrid.selectedCol)
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
                        (_sudokuGrid.Revealed[row, col] || _sudokuGrid.PlayerModified[row, col] || _gameStateManager.puzzleSolved))
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

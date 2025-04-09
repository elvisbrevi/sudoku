using System;
using System.Collections.Generic;

namespace monogame_test.Models
{
    public sealed class SudokuGrid
    {
        private int[,] _grid;
        private bool[,] _revealed;
        private bool[,] _playerModified; // Track cells modified by the player
        private int _gridSize;
        private int _boxSize;
        private Random _random = new Random();

        private static SudokuGrid _instance;
        private static readonly object _lock = new object();
        private SudokuGrid() { }

        public static SudokuGrid GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SudokuGrid();
                    }
                }
            }
            return _instance;
        }

        // Method to initialize the singleton with parameters
        public static void Initialize(int boxSize)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new SudokuGrid(boxSize);
                }
                else
                {
                    throw new InvalidOperationException("SudokuGrid is already initialized");
                }
            }
        }

        // Method to reset the singleton (useful for testing or creating new games)
        public static void Reset()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }

        public int[,] Grid => _grid;
        public bool[,] Revealed => _revealed;
        public bool[,] PlayerModified => _playerModified; // Public accessor for player-modified cells
        public int GridSize => _gridSize;
        public int BoxSize => _boxSize;

        public int selectedRow = -1;
        public int selectedCol = -1;

        // Changed to private constructor for singleton pattern
        private SudokuGrid(int boxSize)
        {
            _boxSize = boxSize;
            _gridSize = boxSize * boxSize;
            _grid = new int[_gridSize, _gridSize];
            _revealed = new bool[_gridSize, _gridSize];
            _playerModified = new bool[_gridSize, _gridSize]; // Initialize player modifications array
            GenerateFullGrid();
        }

        // Generate a valid full Sudoku grid
        private void GenerateFullGrid()
        {
            ClearGrid();
            SolveGrid();
        }

        // Clear the entire grid
        private void ClearGrid()
        {
            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    _grid[row, col] = 0;
                }
            }
        }

        // Recursive backtracking algorithm to solve the grid
        private bool SolveGrid()
        {
            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    if (_grid[row, col] == 0)
                    {
                        // Try placing each number
                        List<int> numbers = GetShuffledNumbers();
                        foreach (int num in numbers)
                        {
                            if (IsValidPlacement(row, col, num))
                            {
                                _grid[row, col] = num;

                                if (SolveGrid())
                                {
                                    return true;
                                }

                                // If we couldn't solve with this number, backtrack
                                _grid[row, col] = 0;
                            }
                        }
                        // If we tried all numbers and none worked, this grid is unsolvable
                        return false;
                    }
                }
            }
            // If we got here, the grid is solved
            return true;
        }

        // Create a playable puzzle by removing some numbers
        public void CreatePuzzle(int difficulty)
        {
            // First, mark all cells as hidden and not modified by player
            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    _revealed[row, col] = false;
                    _playerModified[row, col] = false;
                }
            }

            // Calculate how many cells to reveal based on difficulty
            // Lower difficulty means more cells revealed
            double totalCells = _gridSize * _gridSize;
            double revealPercentage = 0;

            switch (difficulty)
            {
                case 1: // Easy
                    revealPercentage = 0.6; // 60% revealed
                    break;
                case 2: // Medium
                    revealPercentage = 0.45; // 45% revealed
                    break;
                case 3: // Hard
                    revealPercentage = 0.35; // 35% revealed
                    break;
                case 4: // Expert
                    revealPercentage = 0.25; // 25% revealed
                    break;
                default:
                    revealPercentage = 0.45; // Default to medium
                    break;
            }

            int cellsToReveal = (int)(totalCells * revealPercentage);

            // Randomly reveal cells
            int revealed = 0;
            while (revealed < cellsToReveal)
            {
                int row = _random.Next(_gridSize);
                int col = _random.Next(_gridSize);

                if (!_revealed[row, col])
                {
                    _revealed[row, col] = true;
                    revealed++;
                }
            }
        }

        // Checks if placing a number at a specific position is valid
        public bool IsValidPlacement(int row, int col, int num)
        {
            // Temporarily clear the current cell to not interfere with validation
            int originalValue = _grid[row, col];
            _grid[row, col] = 0;

            // Check row
            for (int c = 0; c < _gridSize; c++)
            {
                if (_grid[row, c] == num)
                {
                    _grid[row, col] = originalValue; // Restore original value
                    return false;
                }
            }

            // Check column
            for (int r = 0; r < _gridSize; r++)
            {
                if (_grid[r, col] == num)
                {
                    _grid[row, col] = originalValue; // Restore original value
                    return false;
                }
            }

            // Check box
            int boxRow = row - row % _boxSize;
            int boxCol = col - col % _boxSize;

            for (int r = 0; r < _boxSize; r++)
            {
                for (int c = 0; c < _boxSize; c++)
                {
                    if (_grid[boxRow + r, boxCol + c] == num)
                    {
                        _grid[row, col] = originalValue; // Restore original value
                        return false;
                    }
                }
            }

            // Restore original value
            _grid[row, col] = originalValue;
            return true;
        }

        // Get a list of shuffled numbers from 1 to gridSize
        private List<int> GetShuffledNumbers()
        {
            List<int> numbers = new List<int>();
            for (int i = 1; i <= _gridSize; i++)
            {
                numbers.Add(i);
            }

            // Fisher-Yates shuffle
            for (int i = numbers.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                int temp = numbers[i];
                numbers[i] = numbers[j];
                numbers[j] = temp;
            }

            return numbers;
        }

        // Place a number in the grid at the specified position
        public bool PlaceNumber(int row, int col, int num)
        {
            // Validaciones básicas
            if (row < 0 || row >= _gridSize || col < 0 || col >= _gridSize)
            {
                return false; // Fuera de límites
            }

            if (_revealed[row, col])
            {
                return false; // No se pueden modificar celdas reveladas
            }

            if (num < 0 || num > _gridSize)
            {
                return false; // Número inválido
            }

            // Borrar un número (siempre permitido)
            if (num == 0)
            {
                _grid[row, col] = 0;
                _playerModified[row, col] = true; // Marcar como modificado por el jugador
                return true;
            }

            // Comprobar si el número es válido según las reglas del Sudoku
            if (IsValidPlacement(row, col, num))
            {
                _grid[row, col] = num;
                _playerModified[row, col] = true; // Marcar como modificado por el jugador
                return true;
            }
            else
            {
                // Permitir cualquier número aunque no cumpla las reglas del Sudoku
                // para una experiencia de juego más fluida
                _grid[row, col] = num;
                _playerModified[row, col] = true;
                return true;
            }
        }

        // Check if the puzzle is solved
        public bool IsSolved()
        {
            // Count how many cells the player has actually modified
            int playerModifiedCount = 0;
            int nonRevealedCount = 0;

            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    // Skip revealed cells (the original clues)
                    if (_revealed[row, col])
                    {
                        continue;
                    }

                    // Count non-revealed cells
                    nonRevealedCount++;

                    // Check if player modified this cell
                    if (_playerModified[row, col])
                    {
                        playerModifiedCount++;

                        // If the cell is empty or has an invalid value, puzzle isn't solved
                        if (_grid[row, col] == 0 || !IsValidCell(row, col))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // Non-modified non-revealed cell means puzzle isn't complete
                        return false;
                    }
                }
            }

            // The puzzle is solved if:
            // 1. There are non-revealed cells to solve
            // 2. The player has modified all non-revealed cells
            // 3. All player modifications are valid
            return nonRevealedCount > 0 && playerModifiedCount == nonRevealedCount;
        }

        // Check if a cell's value is valid in the current grid
        private bool IsValidCell(int row, int col)
        {
            int num = _grid[row, col];

            // Temporarily clear this cell to check if it's valid
            _grid[row, col] = 0;
            bool isValid = IsValidPlacement(row, col, num);
            _grid[row, col] = num;

            return isValid;
        }

        // Public method to check if a value would be correct for a given cell
        public bool IsValueCorrect(int row, int col, int value)
        {
            int originalValue = _grid[row, col];
            _grid[row, col] = 0;
            bool isValid = IsValidPlacement(row, col, value);
            _grid[row, col] = originalValue;
            return isValid;
        }
    }
}

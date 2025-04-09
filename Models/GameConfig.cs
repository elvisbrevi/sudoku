using System;
using monogame_test.Models;

namespace monogame_test.Models
{
    // Singleton for game configuration
    public sealed class GameConfig
    {
        // Game settings
        public int BoxSize { get; set; } = 3; // 2 = 4x4, 3 = 9x9
        public int Size { get => BoxSize; set => BoxSize = value; } // Alias for BoxSize
        public int Difficulty { get; set; } = 2; // 1=Easy, 2=Medium, 3=Hard, 4=Expert
        public RepresentationFactory.RepresentationType RepresentationType { get; set; } = RepresentationFactory.RepresentationType.Numbers;
        public INumberRepresentation Representation => RepresentationFactory.CreateRepresentation(RepresentationType);

        // Computed properties
        public int GridSize => BoxSize * BoxSize;

        private static GameConfig _instance;
        private GameConfig() { }

        public static GameConfig GetInstance()
        {
            if (_instance == null)
            {
                _instance = new GameConfig();
            }
            return _instance;
        }

        // Reset configuration to defaults
        public void ResetToDefaults()
        {
            BoxSize = 3;
            Difficulty = 2;
            RepresentationType = RepresentationFactory.RepresentationType.Numbers;
        }
    }
}

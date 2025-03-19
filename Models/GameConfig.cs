using System;
using monogame_test.Models;

namespace monogame_test.Models
{
    // Singleton for game configuration
    public class GameConfig
    {
        private static GameConfig _instance;
        
        // Game settings
        public int BoxSize { get; set; } = 3; // 2 = 4x4, 3 = 9x9
        public int Size { get => BoxSize; set => BoxSize = value; } // Alias for BoxSize
        public int Difficulty { get; set; } = 2; // 1=Easy, 2=Medium, 3=Hard, 4=Expert
        public RepresentationFactory.RepresentationType RepresentationType { get; set; } = RepresentationFactory.RepresentationType.Numbers;
        public INumberRepresentation Representation => RepresentationFactory.CreateRepresentation(RepresentationType);

        // Computed properties
        public int GridSize => BoxSize * BoxSize;
        
        private GameConfig() {}

        public static GameConfig Instance => _instance ??= new GameConfig();

        // Reset configuration to defaults
        public void ResetToDefaults()
        {
            BoxSize = 3;
            Difficulty = 2;
            RepresentationType = RepresentationFactory.RepresentationType.Numbers;
        }
    }
}

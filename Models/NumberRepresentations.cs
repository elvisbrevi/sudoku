using System;
using Microsoft.Xna.Framework;

namespace monogame_test.Models
{
    // Strategy interface for number representation
    public interface INumberRepresentation
    {
        string GetRepresentation(int number);
        Color GetColor(int number);
    }

    // Concrete strategies
    public class NumberRepresentation : INumberRepresentation
    {
        public string GetRepresentation(int number) => number > 0 ? number.ToString() : string.Empty;
        public Color GetColor(int number) => Color.Black;
    }

    public class EmojiRepresentation : INumberRepresentation
    {
        private static readonly string[] Emojis = { "", "😀", "😎", "🤩", "🥳", "😍", "🤓", "🥸", "🤠", "😇" };
        
        public string GetRepresentation(int number) => number > 0 && number < Emojis.Length ? Emojis[number] : string.Empty;
        public Color GetColor(int number) => Color.Black;
    }

    public class LetterRepresentation : INumberRepresentation
    {
        public string GetRepresentation(int number) => number > 0 ? ((char)('A' + number - 1)).ToString() : string.Empty;
        public Color GetColor(int number) => Color.Black;
    }

    public class ColorRepresentation : INumberRepresentation
    {
        private static readonly Color[] Colors = {
            Color.White,          // Empty (0)
            Color.Red,            // 1
            Color.Blue,           // 2
            Color.Green,          // 3
            Color.Yellow,         // 4
            Color.Purple,         // 5
            Color.Orange,         // 6
            Color.Cyan,           // 7
            Color.DarkRed,        // 8
            Color.DarkBlue        // 9
        };

        public string GetRepresentation(int number) => "■";
        
        public Color GetColor(int number) => 
            number >= 0 && number < Colors.Length ? Colors[number] : Color.White;
    }

    // Factory for creating representation strategies
    public static class RepresentationFactory
    {
        public enum RepresentationType
        {
            Numbers,
            Emojis,
            Letters,
            Colors
        }

        public static INumberRepresentation CreateRepresentation(RepresentationType type)
        {
            return type switch
            {
                RepresentationType.Numbers => new NumberRepresentation(),
                RepresentationType.Emojis => new EmojiRepresentation(),
                RepresentationType.Letters => new LetterRepresentation(),
                RepresentationType.Colors => new ColorRepresentation(),
                _ => new NumberRepresentation()
            };
        }
    }
}

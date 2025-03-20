using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogame_test.Models
{
    // Basic UI Component
    public abstract class UIComponent
    {
        public Rectangle Bounds { get; set; }
        public bool IsHovered { get; protected set; }
        public bool IsVisible { get; set; } = true;

        public virtual void Update(MouseState mouseState, MouseState prevMouseState) {}
        public virtual void Draw(SpriteBatch spriteBatch, SpriteFont font) {}
    }

    // Button component
    public class Button : UIComponent
    {
        public string Text { get; set; }
        public Color TextColor { get; set; } = Color.Black;
        public Color BackgroundColor { get; set; } = Color.White;
        public Color HoverColor { get; set; } = Color.LightGray;
        public event Action OnClick;

        public Button(Rectangle bounds, string text)
        {
            Bounds = bounds;
            Text = text;
        }

        public override void Update(MouseState mouseState, MouseState prevMouseState)
        {
            if (!IsVisible) return;
            
            IsHovered = Bounds.Contains(mouseState.Position);
            
            if (IsHovered && mouseState.LeftButton == ButtonState.Released && 
                prevMouseState.LeftButton == ButtonState.Pressed)
            {
                OnClick?.Invoke();
            }
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (!IsVisible) return;
            
            // Draw button background
            spriteBatch.Draw(UIExtensions.GetPixelTexture(spriteBatch.GraphicsDevice), 
                Bounds, IsHovered ? HoverColor : BackgroundColor);
            
            // Draw button border
            DrawBorder(spriteBatch, Bounds, Color.Black, 2);
            
            // Draw button text
            Vector2 textSize = font.MeasureString(Text);
            Vector2 textPosition = new Vector2(
                Bounds.X + (Bounds.Width - textSize.X) / 2,
                Bounds.Y + (Bounds.Height - textSize.Y) / 2
            );
            
            spriteBatch.DrawString(font, Text, textPosition, TextColor);
        }
        
        // Helper method to draw borders
        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int thickness)
        {
            Texture2D pixel = UIExtensions.GetPixelTexture(spriteBatch.GraphicsDevice);
            
            // Draw top line
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
            // Draw left line
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
            // Draw right line
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y, thickness, rectangle.Height), color);
            // Draw bottom line
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color);
        }
    }

    // Dropdown component
    public class Dropdown : UIComponent
    {
        private string[] _options;
        private int _selectedIndex;
        private bool _isOpen;
        private Rectangle[] _optionRects;
        public event Action<int> OnSelectionChanged;
        
        // Variable estática para asegurar que solo un dropdown esté abierto a la vez
        private static Dropdown _currentlyOpenDropdown;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value && value >= 0 && value < _options.Length)
                {
                    _selectedIndex = value;
                    OnSelectionChanged?.Invoke(_selectedIndex);
                }
            }
        }

        public string SelectedOption => _options[_selectedIndex];

        public Dropdown(Rectangle bounds, string[] options, int defaultIndex = 0)
        {
            Bounds = bounds;
            _options = options;
            _selectedIndex = defaultIndex;
            _isOpen = false;
            
            // Create rectangles for each option when dropdown is open
            _optionRects = new Rectangle[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                _optionRects[i] = new Rectangle(
                    bounds.X, bounds.Y + bounds.Height * (i + 1),
                    bounds.Width, bounds.Height
                );
            }
        }

        public override void Update(MouseState mouseState, MouseState prevMouseState)
        {
            if (!IsVisible) return;
            
            IsHovered = Bounds.Contains(mouseState.Position);
            
            // Toggle dropdown when clicked
            if (IsHovered && mouseState.LeftButton == ButtonState.Released && 
                prevMouseState.LeftButton == ButtonState.Pressed)
            {
                // Si este dropdown ya está abierto, cerrarlo
                if (_isOpen)
                {
                    _isOpen = false;
                    _currentlyOpenDropdown = null;
                }
                else
                {
                    // Cerrar cualquier otro dropdown que esté abierto
                    if (_currentlyOpenDropdown != null && _currentlyOpenDropdown != this)
                    {
                        _currentlyOpenDropdown._isOpen = false;
                    }
                    
                    // Abrir este dropdown
                    _isOpen = true;
                    _currentlyOpenDropdown = this;
                }
            }
            
            // Check if an option was clicked
            if (_isOpen)
            {
                for (int i = 0; i < _options.Length; i++)
                {
                    if (_optionRects[i].Contains(mouseState.Position) &&
                        mouseState.LeftButton == ButtonState.Released &&
                        prevMouseState.LeftButton == ButtonState.Pressed)
                    {
                        SelectedIndex = i;
                        _isOpen = false;
                        _currentlyOpenDropdown = null;
                        break;
                    }
                }
            }
            
            // Close dropdown if clicked outside
            if (_isOpen && mouseState.LeftButton == ButtonState.Released && 
                prevMouseState.LeftButton == ButtonState.Pressed &&
                !IsHovered && !IsOptionHovered(mouseState.Position))
            {
                _isOpen = false;
                _currentlyOpenDropdown = null;
            }
        }

        private bool IsOptionHovered(Point position)
        {
            for (int i = 0; i < _optionRects.Length; i++)
            {
                if (_optionRects[i].Contains(position))
                {
                    return true;
                }
            }
            return false;
        }

        // Método de dibujo principal (ahora llama a los métodos separados)
        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (!IsVisible) return;
            
            // Dibujar el fondo y los botones del dropdown
            DrawBackground(spriteBatch, font);
            
            // Dibujar el contenido y las opciones desplegadas
            DrawContent(spriteBatch, font);
        }
        
        // Método para dibujar solamente el fondo y el borde del dropdown
        public void DrawBackground(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (!IsVisible) return;
            
            Texture2D pixel = UIExtensions.GetPixelTexture(spriteBatch.GraphicsDevice);
            
            // Draw main dropdown box
            spriteBatch.Draw(pixel, Bounds, IsHovered ? Color.LightGray : Color.White);
            DrawBorder(spriteBatch, Bounds, Color.Black, 2);
        }
        
        // Método para dibujar el texto y las opciones desplegadas del dropdown
        public void DrawContent(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (!IsVisible) return;
            
            Texture2D pixel = UIExtensions.GetPixelTexture(spriteBatch.GraphicsDevice);
            
            // Draw selected option text
            Vector2 textSize = font.MeasureString(SelectedOption);
            Vector2 textPosition = new Vector2(
                Bounds.X + 10,
                Bounds.Y + (Bounds.Height - textSize.Y) / 2
            );
            
            spriteBatch.DrawString(font, SelectedOption, textPosition, Color.Black);
            
            // Draw dropdown arrow (usando caracteres ASCII simples)
            string arrow = _isOpen ? "^" : "v";
            Vector2 arrowSize = font.MeasureString(arrow);
            Vector2 arrowPosition = new Vector2(
                Bounds.X + Bounds.Width - arrowSize.X - 10,
                Bounds.Y + (Bounds.Height - arrowSize.Y) / 2
            );
            
            spriteBatch.DrawString(font, arrow, arrowPosition, Color.Black);
            
            // Draw options if open
            if (_isOpen)
            {
                for (int i = 0; i < _options.Length; i++)
                {
                    bool isOptionHovered = _optionRects[i].Contains(Mouse.GetState().Position);
                    
                    spriteBatch.Draw(pixel, _optionRects[i], isOptionHovered ? Color.LightGray : Color.White);
                    DrawBorder(spriteBatch, _optionRects[i], Color.Black, 1);
                    
                    Vector2 optionTextSize = font.MeasureString(_options[i]);
                    Vector2 optionTextPosition = new Vector2(
                        _optionRects[i].X + 10,
                        _optionRects[i].Y + (_optionRects[i].Height - optionTextSize.Y) / 2
                    );
                    
                    spriteBatch.DrawString(font, _options[i], optionTextPosition, Color.Black);
                }
            }
        }
        
        // Helper method to draw borders
        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int thickness)
        {
            Texture2D pixel = UIExtensions.GetPixelTexture(spriteBatch.GraphicsDevice);
            
            // Draw top line
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
            // Draw left line
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
            // Draw right line
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y, thickness, rectangle.Height), color);
            // Draw bottom line
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color);
        }
    }

    // Extension methods for UI components
    public static class UIExtensions
    {
        private static Texture2D _pixelTexture;

        public static Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
        {
            if (_pixelTexture == null)
            {
                _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
            }
            return _pixelTexture;
        }
    }
}

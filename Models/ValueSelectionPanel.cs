using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogame_test.Models
{
    // Componente para seleccionar valores en el tablero de Sudoku
    public class ValueSelectionPanel : UIComponent
    {
        private readonly int _gridSize;
        private readonly INumberRepresentation _representation;
        private Rectangle[] _valueRects;
        private bool[] _valueHovered;
        
        // Evento que se dispara cuando se selecciona un valor
        public event Action<int> OnValueSelected;
        
        public ValueSelectionPanel(Rectangle bounds, int gridSize, INumberRepresentation representation)
        {
            Bounds = bounds;
            _gridSize = gridSize;
            _representation = representation;
            
            InitializeValueRectangles();
        }
        
        private void InitializeValueRectangles()
        {
            // Crear un rectángulo para cada valor posible (1 a gridSize) + 1 para borrar (0)
            _valueRects = new Rectangle[_gridSize + 1];
            _valueHovered = new bool[_gridSize + 1];
            
            // Calcular dimensiones
            int itemsPerRow = (int)Math.Ceiling(Math.Sqrt(_gridSize + 1));
            int itemWidth = Bounds.Width / itemsPerRow;
            int itemHeight = Bounds.Height / ((int)Math.Ceiling((_gridSize + 1) / (float)itemsPerRow));
            
            // Crear rectángulos para cada valor
            for (int i = 0; i <= _gridSize; i++)
            {
                int row = i / itemsPerRow;
                int col = i % itemsPerRow;
                
                _valueRects[i] = new Rectangle(
                    Bounds.X + col * itemWidth,
                    Bounds.Y + row * itemHeight,
                    itemWidth,
                    itemHeight
                );
            }
        }
        
        public override void Update(MouseState mouseState, MouseState prevMouseState)
        {
            if (!IsVisible) return;
            
            IsHovered = Bounds.Contains(mouseState.Position);
            
            // Actualizar estado de hover para cada valor
            for (int i = 0; i <= _gridSize; i++)
            {
                _valueHovered[i] = _valueRects[i].Contains(mouseState.Position);
                
                // Comprobar si se ha hecho clic en un valor
                if (_valueHovered[i] && mouseState.LeftButton == ButtonState.Released && 
                    prevMouseState.LeftButton == ButtonState.Pressed)
                {
                    // Disparar evento con el valor seleccionado (0 para borrar, 1-9 para valores)
                    OnValueSelected?.Invoke(i);
                }
            }
        }
        
        public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (!IsVisible) return;
            
            Texture2D pixel = UIExtensions.GetPixelTexture(spriteBatch.GraphicsDevice);
            
            // Dibujar fondo del panel
            spriteBatch.Draw(pixel, Bounds, new Color(245, 245, 245, 230)); // Fondo semi-transparente
            DrawBorder(spriteBatch, Bounds, Color.DarkGray, 2);
            
            // Dibujar cada botón de valor
            for (int i = 0; i <= _gridSize; i++)
            {
                // Color de fondo según estado
                Color bgColor = _valueHovered[i] ? new Color(220, 220, 255) : new Color(250, 250, 250);
                spriteBatch.Draw(pixel, _valueRects[i], bgColor);
                DrawBorder(spriteBatch, _valueRects[i], Color.Gray, 1);
                
                // Valor especial para el 0 (borrar)
                if (i == 0)
                {
                    // Dibujar símbolo de borrar (X)
                    string clearSymbol = "X";
                    Vector2 textSize = font.MeasureString(clearSymbol);
                    Vector2 textPosition = new Vector2(
                        _valueRects[i].X + (_valueRects[i].Width - textSize.X) / 2,
                        _valueRects[i].Y + (_valueRects[i].Height - textSize.Y) / 2
                    );
                    
                    spriteBatch.DrawString(font, clearSymbol, textPosition, Color.Red);
                }
                else
                {
                    // Obtener representación del número según el tipo configurado
                    string representation = _representation.GetRepresentation(i);
                    Color textColor = _representation.GetColor(i);
                    
                    // Calcular posición centrada del texto
                    Vector2 textSize = font.MeasureString(representation);
                    Vector2 textPosition = new Vector2(
                        _valueRects[i].X + (_valueRects[i].Width - textSize.X) / 2,
                        _valueRects[i].Y + (_valueRects[i].Height - textSize.Y) / 2
                    );
                    
                    // Dibujar representación del valor
                    spriteBatch.DrawString(font, representation, textPosition, textColor);
                }
            }
        }
        
        // Método auxiliar para dibujar bordes
        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int thickness)
        {
            Texture2D pixel = UIExtensions.GetPixelTexture(spriteBatch.GraphicsDevice);
            
            // Dibujar línea superior
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
            // Dibujar línea izquierda
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
            // Dibujar línea derecha
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y, thickness, rectangle.Height), color);
            // Dibujar línea inferior
            spriteBatch.Draw(pixel, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color);
        }
    }
}

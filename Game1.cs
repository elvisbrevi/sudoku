using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogame_test;

public class Game1 : Game
{
    //Texture2D ballTexture;
    // The Sprite Font reference to draw with
    SpriteFont font1;

    // The position to draw the text
    Vector2 fontPos;
    int[,] grid;
    int difficulty = 3;
    int gridSize;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        font1 = Content.Load<SpriteFont>("DefaultFont");
        Viewport viewport = _graphics.GraphicsDevice.Viewport;

        // TODO: Load your game content here            
        fontPos = new Vector2(viewport.Width / 2, viewport.Height / 2);

        gridSize = difficulty * difficulty;
        grid = new int[gridSize, gridSize];

        assignBoardValues();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Create a new SpriteBatch, which can be used to draw textures.
    _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    public void assignBoardValues()
    {
        for (int row = 0; row < gridSize; row++)
        {
            List<int> verticalTempValues = new List<int>();
            for (int i = 0; i < gridSize; i++) {
                verticalTempValues.Add(i);
                Console.WriteLine("Vertical temp values: " + verticalTempValues[i]);
            }
            for (int column = 0; column < gridSize; column++)
            {  
                assignCellValues(row, column, verticalTempValues);
            }
        }
    }

    void assignCellValues(int row, int column, List<int> verticalTempValues)
    {
        Console.WriteLine("Assigning cell values for row: " + row + " and column: " + column);
        Func<int> getRandomKey = delegate() {
            Random random = new Random();
            return random.Next(0, verticalTempValues.Count);
        };
        
        int randomIndex = getRandomKey();
        Console.WriteLine("Random index: " + randomIndex);
        grid[row, column] = verticalTempValues[randomIndex];
        verticalTempValues.RemoveAt(randomIndex);
    }

    protected override void Draw(GameTime gameTime)
    {
        _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        _spriteBatch.Begin();

        // Draw the string
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                _spriteBatch.DrawString(font1, grid[i, j].ToString(), new Vector2(i * 20, j * 20), Color.Black);
            }
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

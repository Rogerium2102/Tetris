using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Tetris.Objects;

namespace Tetris;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D squarebase;
    private Texture2D cellbase;
    private Tetromino baseTet;
    private List<Tetromino> tetrominos;
    private Rectangle board = new Rectangle(100,0,600,1250);
    private Random _random;
    private Vector2 defaultPos = new Vector2(200,0);
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 750;
        _graphics.PreferredBackBufferHeight = 1400;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        tetrominos = new List<Tetromino>();
        _random = new Random(DateTime.Now.Second);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        squarebase = Content.Load<Texture2D>("square");
        baseTet = new Tetromino(squarebase, _random.Next(0,7), board, defaultPos);
        baseTet.setPosition(defaultPos);
        tetrominos.Add(baseTet);
        cellbase = Content.Load<Texture2D>("Cell");
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        int collisionResult = CheckCollision();
        if (gameTime.TotalGameTime.Ticks % 5 == 0)
        {
            KeyboardState input = Keyboard.GetState();
            if (input.IsKeyDown(Keys.D) || input.IsKeyDown(Keys.Right))
            {
                if (collisionResult != 1)
                {
                    baseTet.move(0);
                }
            }
            if (input.IsKeyDown(Keys.A) || input.IsKeyDown(Keys.Left))
            {
                if (collisionResult != 2)
                {
                    baseTet.move(1);
                }
            }
            if (input.IsKeyDown(Keys.S) || input.IsKeyDown(Keys.Down)) 
            {
                if (collisionResult == -1)
                {
                    baseTet.move(2);
                }
            }
            if (input.IsKeyDown(Keys.R))
            {
                baseTet.rotate();
            }
        }
        if (gameTime.TotalGameTime.Ticks % 70 == 0)
        {
            if (collisionResult == -1)
            {
                baseTet.move(2);
            }
            else
            {
                baseTet.stopBlock();
            }
        }
        if (baseTet.hasStopped())
        {
            baseTet = new Tetromino(squarebase, _random.Next(0, 7), board, defaultPos);
            baseTet.setPosition(defaultPos);
            tetrominos.Add(baseTet);
        }
            // TODO: Add your update logic here

        base.Update(gameTime);
    }

    private int CheckCollision()
    {
        Rectangle[] currentBound = baseTet.getBoundingRectangles();
        foreach (Tetromino t in tetrominos)
        {
            if (t == baseTet) { continue; }
            foreach (Rectangle c in t.getBoundingRectangles())
            {
                if ((c.Location.ToVector2() - baseTet.getPivot()).Length() > 3 * Square.length)
                {
                    continue;
                }
                foreach (Rectangle bound in baseTet.getBoundingRectangles())
                {
                    if (baseTet.isSquareAboveOthers(bound)) { continue; }
                    if (c.Contains(new Point(bound.X, bound.Y +Square.length))) {
                        return 0;
                    }
                    if (c.Contains(new Point(bound.X + Square.length, bound.Y)))
                    {
                        return 1;
                    }
                    if (c.Contains(new Point(bound.X - Square.length, bound.Y)))
                    {
                        return 2;
                    }
                }
            }
        }
        return -1;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        Color squarecol = new Color(new Vector3(0, 150, 150));
        _spriteBatch.Begin();
        for (int i = 0; i < board.Width / Square.length; i++)
        {
            for (int j = 0; j < board.Height / Square.length; j++)
            {
                _spriteBatch.Draw(cellbase, new Rectangle(board.X + i * Square.length,board.Y + j * Square.length, Square.length, Square.length), Color.DarkGray);
            }
        }
        Vector2 debug = baseTet.getDebugVector();
        Rectangle[] bounds = baseTet.getBoundingRectangles();
        foreach (Rectangle c in bounds)
        {
            _spriteBatch.Draw(cellbase, c, Color.AliceBlue);
        }
        foreach (Tetromino c in  tetrominos)
        {
            c.draw(_spriteBatch);
        }
        // TODO: Add your drawing code here
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}

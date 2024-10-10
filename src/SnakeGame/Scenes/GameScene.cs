using System.Buffers;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SnakeGame.Models;
using SnakeGame.Services;

namespace SnakeGame.Scenes;

public enum GameSceneState { Created, Playing, Paused, GameOver }

public sealed class GameScene(
    GraphicsDevice graphicsDevice,
    ContentManager contentManager,
    KeyboardManager keyboardManager)
    : IScene, IDisposable
{
    private static readonly TimeSpan s_updateInterval = TimeSpan.FromSeconds(0.125);
    private static readonly int s_rows = 24;
    private static readonly int s_cols = 40;
    private static readonly Vector2 s_tileSize = new(20);

    // TODO: Provide this in a TextureManager?
    private readonly Texture2D _texture = CreateTexture1px(graphicsDevice);
    private readonly Snake _snake = new(new Point(s_cols / 2, s_rows / 2));
    private readonly SpriteFont _font = contentManager.Load<SpriteFont>("fonts/Arcade");

    private Point _food = new(-1);
    private Direction _direction = Direction.Right;

    private GameSceneState _gameSceneState = GameSceneState.Created;

    private TimeSpan _updateCooldown = s_updateInterval;

    private static Texture2D CreateTexture1px(GraphicsDevice graphicsDevice)
    {
        var texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData(new Color[] { Color.White });
        return texture;
    }

    public void Dispose() => _texture.Dispose();

    public void Update(GameTime gameTime)
    {
        keyboardManager.Update();
        _updateCooldown -= gameTime.ElapsedGameTime;
        if (_updateCooldown <= TimeSpan.Zero)
        {
            switch (_gameSceneState)
            {
                case GameSceneState.Created:
                    {
                        if (keyboardManager.IsAnyKeyDown(Keys.Right, Keys.D, Keys.Down, Keys.S, Keys.Left, Keys.A, Keys.Up, Keys.W))
                        {
                            if (keyboardManager.IsDirectionPressed(Direction.Right))
                                _direction = Direction.Right;
                            else if (keyboardManager.IsDirectionPressed(Direction.Down))
                                _direction = Direction.Down;
                            else if (keyboardManager.IsDirectionPressed(Direction.Left))
                                _direction = Direction.Left;
                            else if (keyboardManager.IsDirectionPressed(Direction.Up))
                                _direction = Direction.Up;
                            else
                                throw new UnreachableException($"Unexpected starting {nameof(Direction)}");
                            _food = NextFoodPoint(_snake.Body, s_rows, s_cols);
                            _gameSceneState = GameSceneState.Playing;
                        }
                    }
                    break;
                case GameSceneState.Playing when keyboardManager.IsAnyKeyDown(Keys.Space, Keys.P):
                    {
                        _gameSceneState = GameSceneState.Paused;
                    }
                    break;
                case GameSceneState.Playing:
                    {
                        _direction = GetNextDirection();
                        var head = Wrap(_snake.Head.Move(_direction));
                        if (head == _food)
                        {
                            _snake.Eat(head);
                            _food = NextFoodPoint(_snake.Body, s_rows, s_cols);
                        }
                        else if (_snake.Body.Contains(head))
                        {
                            _gameSceneState = GameSceneState.GameOver;
                        }
                        else
                        {
                            _snake.Move(head);
                        }
                    }
                    break;
                case GameSceneState.Paused:
                    {
                        if (keyboardManager.IsAnyKeyDown(Keys.Space, Keys.P))
                            _gameSceneState = GameSceneState.Playing;
                    }
                    break;
                case GameSceneState.GameOver:
                    break;
                default:
                    throw new UnreachableException($"Unexpected {nameof(GameSceneState)} '{_gameSceneState}'");
            }
            _updateCooldown = s_updateInterval;
        }

        static Point Wrap(Point p) => new((p.X % s_cols + s_cols) % s_cols, (p.Y % s_rows + s_rows) % s_rows);
    }

    private static Point NextFoodPoint(IReadOnlyList<Point> snakeBody, int rows, int cols)
    {
        var size = rows * cols - snakeBody.Count;
        var buffer = ArrayPool<Point>.Shared.Rent(size);
        var choices = buffer.AsSpan(0, size);
        var i = 0;
        for (var y = 0; y < rows; ++y)
        {
            for (var x = 0; x < cols; ++x)
            {
                var p = new Point(x, y);
                if (!snakeBody.Contains(p))
                {
                    choices[i++] = p;
                }
            }
        }

        Span<Point> choiceSpan = stackalloc Point[1];

        Random.Shared.GetItems(choices, choiceSpan);

        var choice = choiceSpan[0];

        ArrayPool<Point>.Shared.Return(buffer);

        return choice;
    }

    private Direction GetNextDirection()
    {
        switch (_direction)
        {
            case Direction.Right or Direction.Left:
                if (keyboardManager.IsDirectionPressed(Direction.Down))
                    return Direction.Down;
                if (keyboardManager.IsDirectionPressed(Direction.Up))
                    return Direction.Up;
                return _direction;

            case Direction.Down or Direction.Up:
                if (keyboardManager.IsDirectionPressed(Direction.Right))
                    return Direction.Right;
                if (keyboardManager.IsDirectionPressed(Direction.Left))
                    return Direction.Left;
                return _direction;

            default:
                return _direction;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        DrawGrid(spriteBatch);
        DrawFood(spriteBatch);
        DrawSnake(spriteBatch);
        DrawText(spriteBatch);
    }

    private void DrawGrid(SpriteBatch spriteBatch)
    {
        for (var y = 0; y < s_rows; ++y)
        {
            for (var x = 0; x < s_cols; ++x)
            {
                spriteBatch.Draw(
                    _texture,
                    new Rectangle(
                        location: (new Vector2(x, y) * s_tileSize).ToPoint(),
                        size: s_tileSize.ToPoint()),
                    ((x + y) % 2 == 0) ? Color.White : Color.Wheat);
            }
        }
    }

    private void DrawSnake(SpriteBatch spriteBatch)
    {
        foreach (var part in _snake.Body)
        {
            spriteBatch.Draw(
                _texture,
                new Rectangle(
                    location: (part.ToVector2() * s_tileSize).ToPoint(),
                    size: s_tileSize.ToPoint()),
                Color.Black);
        }
    }

    private void DrawFood(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _texture,
            new Rectangle(
                location: (_food.ToVector2() * s_tileSize).ToPoint(),
                size: s_tileSize.ToPoint()),
            Color.Green);
    }

    private void DrawText(SpriteBatch spriteBatch)
    {
        var text = _gameSceneState switch
        {
            GameSceneState.Paused => "Pause",
            GameSceneState.GameOver => "Game Over",
            _ => null
        };

        var hBoundsWidth = graphicsDevice.PresentationParameters.Bounds.Width / 2;
        var hBoundsHeight = graphicsDevice.PresentationParameters.Bounds.Height / 2;

        if (text is not null)
        {
            var measure = _font.MeasureString(text) / 2;
            var pausePosition = new Vector2(hBoundsWidth - measure.X, hBoundsHeight - measure.Y);
            spriteBatch.DrawString(_font, text, pausePosition, Color.DarkBlue);
        }
    }
}

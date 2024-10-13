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
    private static readonly int s_rows = 20;
    private static readonly int s_cols = 36;
    private static readonly Vector2 s_tileSize = new(20);
    private static readonly Vector2 s_drawOffset = new(2);
    private static readonly Point s_outOfBounds = ((s_drawOffset + new Vector2(1)) * -1).ToPoint();
    private static readonly int s_bugCountdown = 5;
    private static readonly TimeSpan s_initialUpdateInternal = TimeSpan.FromSeconds(0.125);
    private static readonly int s_speedCountdown = 9;
    private static readonly TimeSpan s_bugShowDuration = TimeSpan.FromSeconds(7);

    private int _score = 0;

    private readonly Snake _snake = new(new Point(s_cols / 2, s_rows / 2), new Point(s_cols / 2 - 1, s_rows / 2));

    private readonly Texture2D _texture1px = CreateTexture1px(graphicsDevice);
    private readonly Texture2D _snakeSprite = contentManager.Load<Texture2D>("sprites/snake");
    private readonly SpriteFont _font = contentManager.Load<SpriteFont>("fonts/Arcade");

    private GameSceneState _gameSceneState = GameSceneState.Created;
    private Direction _direction = Direction.Right;

    private Point _food = s_outOfBounds;
    private Point _bug = s_outOfBounds;
    private int _bugIndex = 0;
    private Duration _bugShowDuration = new Duration(s_bugShowDuration);
    private int _nextBugCountdown = s_bugCountdown;

    private Duration _updateCooldown = new Duration(s_initialUpdateInternal);
    private int _nextSpeedUpCountdown = s_speedCountdown;

    private static Texture2D CreateTexture1px(GraphicsDevice graphicsDevice)
    {
        var texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData(new Color[] { Color.White });
        return texture;
    }

    public void Dispose() => _texture1px.Dispose();

    public void Update(GameTime gameTime)
    {
        keyboardManager.Update();
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
                        _food = NextEmptyPoint(_snake.Body, _bug, s_rows, s_cols);
                        _gameSceneState = GameSceneState.Playing;
                    }
                }
                break;
            case GameSceneState.Playing when keyboardManager.IsAnyKeyPressed(Keys.Space, Keys.P):
                {
                    _gameSceneState = GameSceneState.Paused;
                }
                break;
            case GameSceneState.Playing:
                {
                    _updateCooldown.Update(gameTime);
                    if (_updateCooldown.IsExpired)
                    {
                        _direction = GetNextDirection();
                        var head = Wrap(_snake.Head.Point.GetNextPosition(_direction));
                        if (head == _food)
                        {
                            _snake.Eat(head);
                            _score += 9;
                            _food = NextEmptyPoint(_snake.Body, _bug, s_rows, s_cols);
                            if (_nextBugCountdown > 0)
                            {
                                --_nextBugCountdown;
                                if (_nextBugCountdown == 0)
                                {
                                    _bug = NextEmptyPoint(_snake.Body, _food, s_rows, s_cols);
                                    _bugIndex = Random.Shared.Next(5);
                                    _bugShowDuration.Reset();
                                }
                            }
                        }
                        else if (head == _bug)
                        {
                            _snake.Eat(head);
                            _bug = s_outOfBounds;
                            _score += (int)Math.Round(_bugShowDuration.RemainingTime.TotalSeconds, MidpointRounding.AwayFromZero) * 10;
                            _nextBugCountdown = s_bugCountdown;
                        }
                        else if (head != _snake.Tail.Point && _snake.Body.Any(bp => head == bp.Point))
                        {
                            _gameSceneState = GameSceneState.GameOver;
                        }
                        else
                        {
                            _snake.Move(head);
                        }
                        _updateCooldown.Reset();
                    }

                    _bugShowDuration.Update(gameTime);
                    if (_bugShowDuration.IsExpired && _nextBugCountdown == 0)
                    {
                        _bug = s_outOfBounds;
                        _nextBugCountdown = s_bugCountdown;
                    }
                }
                break;
            case GameSceneState.Paused:
                {
                    if (keyboardManager.IsAnyKeyPressed(Keys.Space, Keys.P))
                        _gameSceneState = GameSceneState.Playing;
                }
                break;
            case GameSceneState.GameOver:
                break;
            default:
                throw new UnreachableException($"Unexpected {nameof(GameSceneState)} '{_gameSceneState}'");
        }

        static Point Wrap(Point p) => new((p.X % s_cols + s_cols) % s_cols, (p.Y % s_rows + s_rows) % s_rows);
    }

    private static Point NextEmptyPoint(IReadOnlyList<BodyPart> snakeBody, Point foodOrBug, int rows, int cols)
    {
        var size = rows * cols - snakeBody.Count - (foodOrBug.X > 0 ? 1 : 0);
        var buffer = ArrayPool<Point>.Shared.Rent(size);
        var choices = buffer.AsSpan(0, size);
        var i = 0;
        for (var y = 0; y < rows; ++y)
        {
            for (var x = 0; x < cols; ++x)
            {
                var p = new Point(x, y);
                if (p != foodOrBug && snakeBody.All(bp => p != bp.Point))
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
        DrawBorder(spriteBatch);
        //DrawGrid(spriteBatch);
        DrawFood(spriteBatch);
        DrawBug(spriteBatch);
        DrawBugTime(spriteBatch);
        DrawSnake(spriteBatch);
        DrawScore(spriteBatch);
        DrawText(spriteBatch);
    }

    private void DrawBorder(SpriteBatch spriteBatch)
    {
        // Vertical
        for (var y = -1; y <= s_rows; ++y)
        {
            spriteBatch.Draw(
                _texture1px,
                new Rectangle(
                    location: ((new Vector2(s_drawOffset.X - 1, y + s_drawOffset.Y)) * s_tileSize).ToPoint(),
                    size: s_tileSize.ToPoint()),
                Color.Black);

            spriteBatch.Draw(
                _texture1px,
                new Rectangle(
                    location: ((new Vector2(s_cols, y) + s_drawOffset) * s_tileSize).ToPoint(),
                    size: s_tileSize.ToPoint()),
                Color.Black);
        }

        // Horizontal
        for (var x = 0; x < s_cols; ++x)
        {
            spriteBatch.Draw(
                _texture1px,
                new Rectangle(
                    location: ((new Vector2(x + s_drawOffset.X, s_drawOffset.Y - 1)) * s_tileSize).ToPoint(),
                    size: s_tileSize.ToPoint()),
                Color.Black);

            spriteBatch.Draw(
                _texture1px,
                new Rectangle(
                    location: ((new Vector2(x, s_rows) + s_drawOffset) * s_tileSize).ToPoint(),
                    size: s_tileSize.ToPoint()),
                Color.Black);
        }
    }

    private void DrawGrid(SpriteBatch spriteBatch)
    {
        for (var y = 0; y < s_rows; ++y)
        {
            for (var x = 0; x < s_cols; ++x)
            {
                spriteBatch.Draw(
                    _texture1px,
                    new Rectangle(
                        location: ((new Vector2(x, y) + s_drawOffset) * s_tileSize).ToPoint(),
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
                _snakeSprite,
                new Rectangle(
                    location: ((part.Vector2 + s_drawOffset) * s_tileSize).ToPoint(),
                    size: s_tileSize.ToPoint()),
                part.SrcRect,
                Color.Black);
        }
    }

    private void DrawFood(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _snakeSprite,
            new Rectangle(
                location: ((_food.ToVector2() + s_drawOffset) * s_tileSize).ToPoint(),
                size: s_tileSize.ToPoint()),
            new Rectangle(
                location: new Point(100, 0),
                size: new Point(20)),
            Color.DarkGreen);
    }

    private void DrawBug(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _snakeSprite,
            new Rectangle(
                location: ((_bug.ToVector2() + s_drawOffset) * s_tileSize).ToPoint(),
                size: s_tileSize.ToPoint()),
            new Rectangle(
                location: new Point(80, _bugIndex * 20),
                size: new Point(20)),
            Color.DarkRed);
    }

    private void DrawScore(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(
            _font,
            $"Score: {_score}",
            new Vector2(s_drawOffset.X - 1, s_drawOffset.Y - 2) * s_tileSize, Color.Black);
    }

    private void DrawBugTime(SpriteBatch spriteBatch)
    {
        if (_nextBugCountdown == 0)
        {
            var time = $"{_bugShowDuration.RemainingTime.TotalSeconds:F1}s";
            var size = _font.MeasureString(time);
            spriteBatch.DrawString(
                _font,
                time,
                new Vector2(s_cols + s_drawOffset.X, s_drawOffset.Y - 2) * s_tileSize - new Vector2(size.X, 0),
                Color.Black);

            spriteBatch.Draw(
            _snakeSprite,
            new Rectangle(
                location: (new Vector2(s_cols + s_drawOffset.X, s_drawOffset.Y - 2) * s_tileSize).ToPoint(),
                size: s_tileSize.ToPoint()),
            new Rectangle(
                location: new Point(80, _bugIndex * 20),
                size: new Point(20)),
            Color.Black);
        }
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

public record struct Duration(TimeSpan DurationTime)
{
    public TimeSpan RemainingTime { get; private set; } = DurationTime;

    public readonly bool IsExpired => RemainingTime <= TimeSpan.Zero;

    public void Update(GameTime gameTime) => RemainingTime -= gameTime.ElapsedGameTime;

    public void Reset() => RemainingTime = DurationTime;
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SnakeGame.Models;
using SnakeGame.Services;

namespace SnakeGame.Scenes;
public sealed class MenuScene : IScene
{
    private readonly GameOptions _gameOptions;
    private readonly SceneManager _sceneManager;
    private readonly ContentManager _contentManager;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly InputManager _inputManager;
    private readonly Texture2D _title;
    private readonly SpriteFont _font;
    private readonly MenuItem[] _menuItems;
    private readonly int _ySpacing;
    private int _selectedIndex = 0;
    private int _difficulty = 9;

    public MenuScene(GameOptions gameOptions, SceneManager sceneManager, ContentManager contentManager, GraphicsDevice graphicsDevice, InputManager inputManager)
    {
        _gameOptions = gameOptions;
        _sceneManager = sceneManager;
        _contentManager = contentManager;
        _graphicsDevice = graphicsDevice;
        _inputManager = inputManager;

        _title = _contentManager.Load<Texture2D>("sprites/title");
        _font = _contentManager.Load<SpriteFont>("fonts/Arcade");
        _menuItems = [
            new MenuItem(_font, "Play")
            {
                OnEnter = mi =>
                {
                    _gameOptions.Difficulty = _difficulty;
                    _sceneManager.Push<GameScene>();
                }
            },
            new MenuItem(_font, $"Difficulty {_difficulty}")
            {
                OnLeft = mi =>
                {
                    _difficulty = int.Max(1, _difficulty - 1);
                    mi.Content = $"Difficulty {_difficulty}";
                },
                OnRight = mi =>
                {
                    _difficulty = int.Min(9, _difficulty + 1);
                    mi.Content = $"Difficulty {_difficulty}";
                }
            },
            new MenuItem(_font, "Exit")
            {
                OnEnter = mi => _sceneManager.Pop(),
            }
        ];
        _ySpacing = (int)_menuItems.Max(i => i.Measure.Y) + 5;
    }


    public void Update(GameTime gameTime)
    {
        if (_inputManager.Keyboard.WasDirectionPressed(Direction.Down))
            ++_selectedIndex;
        if (_inputManager.Keyboard.WasDirectionPressed(Direction.Up))
            --_selectedIndex;
        _selectedIndex = int.Clamp(_selectedIndex, 0, _menuItems.Length - 1);

        var menuItem = _menuItems[_selectedIndex];

        if (_inputManager.Keyboard.WasDirectionPressed(Direction.Right))
            menuItem.OnRight?.Invoke(menuItem);
        if (_inputManager.Keyboard.WasDirectionPressed(Direction.Left))
            menuItem.OnLeft?.Invoke(menuItem);
        if (_inputManager.Keyboard.WasKeyPressed(Keys.Enter))
            menuItem.OnEnter?.Invoke(menuItem);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        DrawHighScore(spriteBatch);
        DrawSnakeIcon(spriteBatch);
        DrawMenuItems(spriteBatch);
    }

    private void DrawHighScore(SpriteBatch spriteBatch)
    {
        var display = $"Highscore: {_gameOptions.HighScore}";
        var measure = _font.MeasureString(display);

        var position = new Vector2(
            _graphicsDevice.PresentationParameters.Bounds.Width - measure.X - 15,
            _graphicsDevice.PresentationParameters.Bounds.Height - measure.Y - 5);

        spriteBatch.DrawString(_font, display, position, Color.Black);
    }

    private void DrawSnakeIcon(SpriteBatch spriteBatch)
    {
        var x = _graphicsDevice.PresentationParameters.Bounds.Width / 2 - _title.Width / 2;
        var y = _graphicsDevice.PresentationParameters.Bounds.Height / 2 - _title.Height;
        spriteBatch.Draw(_title, new Vector2(x, y), Color.Black);
    }

    private void DrawMenuItems(SpriteBatch spriteBatch)
    {
        var x = _graphicsDevice.PresentationParameters.Bounds.Width / 2;
        var y = _graphicsDevice.PresentationParameters.Bounds.Height / 2;
        for (var i = 0; i < _menuItems.Length; ++i)
        {
            var color = i == _selectedIndex ? Color.Green : Color.Black;
            _menuItems[i].Draw(spriteBatch, new Vector2(x, y + i * _ySpacing), color);
        }
    }
}

public sealed class MenuItem
{
    private readonly SpriteFont _font;

    private string _content;

    public MenuItem(SpriteFont font, string content)
    {
        _font = font;
        _content = content;
        Measure = _font.MeasureString(content);
    }

    public string Content
    {
        get => _content;
        set
        {
            if (value == _content) return;
            _content = value;
            _font.MeasureString(_content);
        }
    }

    public Vector2 Measure { get; private set; }

    public Action<MenuItem>? OnRight { get; init; }
    public Action<MenuItem>? OnLeft { get; init; }
    public Action<MenuItem>? OnEnter { get; init; }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
    {
        position.X -= Measure.X / 2;
        spriteBatch.DrawString(_font, Content, position, color);
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SnakeGame.Scenes;
using SnakeGame.Services;

namespace SnakeGame;
public class GameRoot : Game
{
    private readonly IServiceProvider _services;
    private KeyboardManager? _keyboardManager;
    private SceneManager? _sceneManager;
    private SpriteBatch? _spriteBatch;

    private KeyboardManager KeyboardManager => _keyboardManager ??= _services.GetRequiredService<KeyboardManager>();
    private SceneManager SceneManager => _sceneManager ??= _services.GetRequiredService<SceneManager>();
    private SpriteBatch SpriteBatch => _spriteBatch ??= _services.GetRequiredService<SpriteBatch>();


    public GameRoot()
    {
        var graphicsDeviceManager = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _services = new ServiceCollection()
            .AddSingleton<IGraphicsDeviceService>(graphicsDeviceManager)
            .AddSingleton(provider => provider.GetRequiredService<IGraphicsDeviceService>().GraphicsDevice)
            .AddSingleton(provider => new SpriteBatch(provider.GetRequiredService<GraphicsDevice>()))
            .AddSingleton(Content)
            .AddSingleton<KeyboardManager>()
            .AddSingleton<SceneManager>()
            .AddTransient<GameScene>()
            .BuildServiceProvider();
    }

    protected override void Initialize()
    {
        SceneManager.Push<GameScene>();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        if (SceneManager.Count == 1 && KeyboardManager.IsKeyPressed(Keys.Escape))
            Exit();

        SceneManager.CurrentScene.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Begin();
        SceneManager.CurrentScene.Draw(SpriteBatch);
        SpriteBatch.End();

        base.Draw(gameTime);
    }
}

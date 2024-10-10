using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnakeGame.Scenes;
using SnakeGame.Services;

namespace SnakeGame;
public class GameRoot : Game
{
    private readonly IServiceProvider _services;

    public GameRoot()
    {
        var graphicsDeviceManager = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "Snake";

        _services = new ServiceCollection()
            .AddSingleton(this)
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
        Components.Add(_services.GetRequiredService<SceneManager>());

        base.Initialize();
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SnakeGame.Scenes;

namespace SnakeGame.Services;

public sealed class SceneManager(
    IServiceProvider services,
    GraphicsDevice graphicsDevice,
    SpriteBatch spriteBatch,
    KeyboardManager keyboardManager)
    : IGameComponent, IUpdateable, IDrawable
{
    private readonly Stack<IScene> _scenes = [];

#pragma warning disable CS0414
#pragma warning disable CS0067
    public event EventHandler<EventArgs>? EnabledChanged;
    public event EventHandler<EventArgs>? UpdateOrderChanged;
    public event EventHandler<EventArgs>? VisibleChanged;
    public event EventHandler<EventArgs>? DrawOrderChanged;
#pragma warning restore CS0414
#pragma warning restore CS0067

    public IScene CurrentScene => _scenes.Peek();

    bool IUpdateable.Enabled => true;
    int IUpdateable.UpdateOrder => 0;

    bool IDrawable.Visible => true;
    int IDrawable.DrawOrder => 0;

    public void Push<TScene>() where TScene : IScene =>
        _scenes.Push(services.GetRequiredService<TScene>());

    public IScene Pop() => _scenes.Pop();

    public void Initialize() => Push<GameScene>();

    public void Update(GameTime gameTime)
    {
        if (keyboardManager.IsKeyPressed(Keys.Escape))
        {
            if (_scenes.Count > 1)
                _scenes.Pop();
            else
                services.GetRequiredService<GameRoot>().Exit();
        }

        CurrentScene.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        graphicsDevice.Clear(Color.CornflowerBlue);

        spriteBatch.Begin();
        CurrentScene.Draw(spriteBatch);
        spriteBatch.End();
    }
}

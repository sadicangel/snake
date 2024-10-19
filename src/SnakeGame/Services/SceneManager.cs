using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SnakeGame.Scenes;

namespace SnakeGame.Services;

public sealed class SceneManager(
    GameRoot game,
    GraphicsDevice graphicsDevice,
    SpriteBatch spriteBatch,
    InputManager inputManager)
    : DrawableGameComponent(game)
{
    private readonly Stack<IScene> _scenes = [];

    public IScene ActiveScene => _scenes.Peek();

    public void Push<TScene>() where TScene : IScene =>
        _scenes.Push(game.GetRequiredService<TScene>());

    public IScene Pop() => _scenes.Pop();

    public override void Initialize() => Push<MenuScene>();

    public override void Update(GameTime gameTime)
    {
        if (inputManager.Keyboard.WasKeyPressed(Keys.Escape))
        {
            if (_scenes.Count > 1)
                _scenes.Pop();
            else
                game.Exit();
        }

        ActiveScene.Update(gameTime);
        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        graphicsDevice.Clear(Color.DarkSeaGreen);

        spriteBatch.Begin();
        ActiveScene.Draw(spriteBatch);
        spriteBatch.End();
        base.Draw(gameTime);
    }
}

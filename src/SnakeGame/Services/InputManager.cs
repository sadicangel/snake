using Microsoft.Xna.Framework;
using MonoGame.Extended.Input;

namespace SnakeGame.Services;
public sealed class InputManager(GameRoot game) : GameComponent(game)
{
    public KeyboardStateExtended Keyboard { get; private set; }

    public override void Update(GameTime gameTime)
    {
        KeyboardExtended.Update();
        Keyboard = KeyboardExtended.GetState();
        base.Update(gameTime);
    }
}

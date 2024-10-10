using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SnakeGame.Scenes;
public interface IScene
{
    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
}

using Microsoft.Xna.Framework;

namespace SnakeGame.Models;

public record struct Duration(TimeSpan DurationTime)
{
    public TimeSpan RemainingTime { get; private set; } = DurationTime;

    public readonly bool IsExpired => RemainingTime <= TimeSpan.Zero;

    public void Update(GameTime gameTime) => RemainingTime -= gameTime.ElapsedGameTime;

    public void Reset() => RemainingTime = DurationTime;
}

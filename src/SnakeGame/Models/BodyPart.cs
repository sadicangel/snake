using Microsoft.Xna.Framework;

namespace SnakeGame.Models;

public readonly record struct BodyPart(Point Point, PartType Type, Direction Direction)
{
    public Vector2 Vector2 => Point.ToVector2();

    public Rectangle SrcRect => new(new Point((int)Direction * 20, (int)Type * 20), new Point(20));
}

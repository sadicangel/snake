using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace SnakeGame.Models;

public enum Direction
{
    Right, Down, Left, Up
}

public static class DirectionExtensions
{
    public static bool IsPositive(this Direction direction) => direction is Direction.Right or Direction.Down;

    public static Point ToPoint(this Direction direction) => direction switch
    {
        Direction.Right => new Point(1, 0),
        Direction.Down => new Point(0, 1),
        Direction.Left => new Point(-1, 0),
        Direction.Up => new Point(0, -1),
        _ => throw new UnreachableException($"Unexpected {nameof(Direction)} '{direction}'"),
    };

    public static Point GetNextPosition(this Point point, Direction direction) => point + direction.ToPoint();

    public static bool IsDirectionDown(this KeyboardStateExtended keyboardManager, Direction direction) => direction switch
    {
        Direction.Right => keyboardManager.IsKeyDown(Keys.Right) || keyboardManager.IsKeyDown(Keys.D),
        Direction.Down => keyboardManager.IsKeyDown(Keys.Down) || keyboardManager.IsKeyDown(Keys.S),
        Direction.Left => keyboardManager.IsKeyDown(Keys.Left) || keyboardManager.IsKeyDown(Keys.A),
        Direction.Up => keyboardManager.IsKeyDown(Keys.Up) || keyboardManager.IsKeyDown(Keys.W),
        _ => throw new UnreachableException($"Unexpected {nameof(Direction)} '{direction}'"),
    };

    public static bool WasDirectionPressed(this KeyboardStateExtended keyboardManager, Direction direction) => direction switch
    {
        Direction.Right => keyboardManager.WasKeyPressed(Keys.Right) || keyboardManager.WasKeyPressed(Keys.D),
        Direction.Down => keyboardManager.WasKeyPressed(Keys.Down) || keyboardManager.WasKeyPressed(Keys.S),
        Direction.Left => keyboardManager.WasKeyPressed(Keys.Left) || keyboardManager.WasKeyPressed(Keys.A),
        Direction.Up => keyboardManager.WasKeyPressed(Keys.Up) || keyboardManager.WasKeyPressed(Keys.W),
        _ => throw new UnreachableException($"Unexpected {nameof(Direction)} '{direction}'"),
    };
}

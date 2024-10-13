using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SnakeGame.Services;

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

    public static bool IsDirectionPressed(this KeyboardManager keyboardManager, Direction direction) => direction switch
    {
        Direction.Right => keyboardManager.IsAnyKeyDown(Keys.Right, Keys.D),
        Direction.Down => keyboardManager.IsAnyKeyDown(Keys.Down, Keys.S),
        Direction.Left => keyboardManager.IsAnyKeyDown(Keys.Left, Keys.A),
        Direction.Up => keyboardManager.IsAnyKeyDown(Keys.Up, Keys.W),
        _ => throw new UnreachableException($"Unexpected {nameof(Direction)} '{direction}'"),
    };
}

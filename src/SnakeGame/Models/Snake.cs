using Microsoft.Xna.Framework;
using Nito.Collections;

namespace SnakeGame.Models;

public sealed class Snake(Point head, Point tail)
{
    private readonly Deque<BodyPart> _parts = new([
        new BodyPart(head, PartType.Head, Direction.Right),
        new BodyPart(tail, PartType.Tail, Direction.Right),
    ]);

    private bool _wasLastMoveAnEat = false;

    public BodyPart Head => _parts.First();

    public BodyPart Tail => _parts.Last();

    public IReadOnlyList<BodyPart> Body => _parts;

    public string ToString(int rows, int cols)
    {
        var grid = Enumerable
            .Range(0, rows)
            .Select(_ =>
            {
                var col = new char[cols];
                Array.Fill(col, '.');
                return col;
            })
            .ToArray();

        foreach (var point in _parts.Select(p => p.Point))
            grid[point.Y][point.X] = 'X';

        return string.Join('\n', grid.Select(row => new string(row)));
    }

    private static Direction GetNextDirection(Point prev, Point next)
    {
        if (prev.Y == next.Y)
        {
            return next.X == prev.X + 1 || next.X < prev.X - 1
                ? Direction.Right
                : Direction.Left;
        }
        else
        {
            return next.Y == prev.Y + 1 || next.Y < prev.Y - 1
                ? Direction.Down
                : Direction.Up;
        }
    }

    private static PartType GetNextPartType(Direction prev, Direction next)
    {
        if (next == prev)
        {
            return PartType.BellyEmpty;
        }

        return prev.IsPositive() ? PartType.CornerPositive : PartType.CornerNegative;
    }

    public void Move(Point point)
    {
        var direction = GetNextDirection(Head.Point, point);
        _parts[0] = _parts[0] with { Type = _wasLastMoveAnEat ? PartType.BellyFull : GetNextPartType(_parts[0].Direction, direction), Direction = direction };
        _parts.AddToFront(new BodyPart(point, PartType.Head, direction));
        _parts.RemoveFromBack();
        _parts[^1] = _parts[^1] with { Type = PartType.Tail };
        _wasLastMoveAnEat = false;
    }

    public void Eat(Point point)
    {
        var direction = GetNextDirection(Head.Point, point);
        _parts[0] = _parts[0] with { Type = _wasLastMoveAnEat ? PartType.BellyFull : GetNextPartType(_parts[0].Direction, direction) };
        _parts.AddToFront(new BodyPart(point, PartType.Head, direction));
        _wasLastMoveAnEat = true;
    }
}

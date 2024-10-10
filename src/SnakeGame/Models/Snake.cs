using Microsoft.Xna.Framework;
using Nito.Collections;

namespace SnakeGame.Models;

public sealed class Snake(Point point)
{
    private readonly Deque<Point> _points = new([point]);

    public Point Head => _points.First();

    public Point Tail => _points.Last();

    public IReadOnlyList<Point> Body => _points;

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

        foreach (var point in _points)
            grid[point.Y][point.X] = 'X';

        return string.Join('\n', grid.Select(row => new string(row)));
    }

    public void Move(Point point)
    {
        _points.AddToFront(point);
        _points.RemoveFromBack();
    }

    public void Eat(Point point)
    {
        _points.AddToFront(point);
    }
}

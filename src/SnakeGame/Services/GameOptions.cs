using System.Diagnostics;
using SnakeGame.Models;

namespace SnakeGame.Services;

public sealed class GameOptions
{
    public int Difficulty { get; set; }
    public Duration UpdateDuration
    {
        get
        {

            Debug.WriteLine(0.5 - double.Lerp(0.1, 0.4, Difficulty / 9d));
            return new(TimeSpan.FromSeconds(0.5 - double.Lerp(0.0, 0.4, Difficulty / 9d)));
        }
    }
    public Duration BugShowDuration { get => new(TimeSpan.FromSeconds(15 - Difficulty)); }

    public int HighScore { get; set; }
}

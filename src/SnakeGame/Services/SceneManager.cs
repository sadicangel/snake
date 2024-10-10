using Microsoft.Extensions.DependencyInjection;
using SnakeGame.Scenes;

namespace SnakeGame.Services;

public sealed class SceneManager(IServiceProvider services)
{
    private readonly Stack<IScene> _scenes = [];

    public IScene CurrentScene => _scenes.Peek();

    public int Count => _scenes.Count;

    public void Push<TScene>() where TScene : IScene =>
        _scenes.Push(services.GetRequiredService<TScene>());

    public IScene Pop() => _scenes.Pop();
}

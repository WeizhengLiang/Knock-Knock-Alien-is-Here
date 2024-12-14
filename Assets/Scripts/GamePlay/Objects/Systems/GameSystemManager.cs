using System.Collections.Generic;

public class GameSystemManager
{
    private readonly List<IGameSystem> systems = new List<IGameSystem>();

    public T GetSystem<T>() where T : class, IGameSystem
    {
        return systems.Find(s => s is T) as T;
    }

    public void AddSystem(IGameSystem system)
    {
        if (system != null && !systems.Contains(system))
        {
            systems.Add(system);
            system.Initialize();
        }
    }

    public void Update()
    {
        foreach (var system in systems)
        {
            system.Update();
        }
    }

    public void FixedUpdate()
    {
        foreach (var system in systems)
        {
            system.FixedUpdate();
        }
    }

    public void Cleanup()
    {
        foreach (var system in systems)
        {
            system.Cleanup();
        }
        systems.Clear();
    }
} 
public interface IGameSystem
{
    void Initialize();
    void Cleanup() { }
    void Update();
    void FixedUpdate();
} 
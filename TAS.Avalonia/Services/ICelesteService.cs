namespace TAS.Avalonia.Services;

public interface ICelesteService : IService
{
    void WriteWait();
    void SendPath(string path);
    void Play();

    void ToggleHitboxes();
}

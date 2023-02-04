namespace TAS.Avalonia.Services;

public interface ICelesteService : IService
{
    void WriteWait();
    void SendPath(string path);
    void Play();

    void ToggleHitboxes();
    void ShowTriggerHitboxes();
    void ShowUnloadedRoomsHitboxes();
    void ShowCameraHitboxes();
    void SimplifiedHitboxes();
    void ShowActualCollideHitboxes();
    
    void SimplifiedGraphics();
    void ShowGameplay();
    
    void CenterCamera();
    
    void InfoHud();
    void InfoTasInput();
    void InfoGame();
    void InfoWatchEntity();
    void InfoCustom();
    void InfoSubpixelIndicator();
    
    void SpeedUnit();
    // ("Copy Custom Info Template to Clipboard", null, sender);
    // ("Set Custom Info Template From Clipboard", null, sender);
    // ("Clear Custom Info Template", null, sender);
    // ("Clear Watch Entity Info", null, sender);
}

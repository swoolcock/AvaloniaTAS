namespace TAS.Avalonia.Services;

public interface ICelesteService : IService {
    void WriteWait();
    void SendPath(string path);
    void Play();

    void ToggleHitboxes();
    void ToggleTriggerHitboxes();
    void ToggleUnloadedRoomsHitboxes();
    void ToggleCameraHitboxes();
    void ToggleSimplifiedHitboxes();
    void ToggleActualCollideHitboxes();

    void ToggleSimplifiedGraphics();
    void ToggleGameplay();

    void ToggleCenterCamera();

    void ToggleInfoHud();
    void ToggleInfoTasInput();
    void ToggleInfoGame();
    void ToggleInfoWatchEntity();
    void ToggleInfoCustom();
    void ToggleInfoSubpixelIndicator();

    void ToggleSpeedUnit();
    // ("Copy Custom Info Template to Clipboard", null, sender);
    // ("Set Custom Info Template From Clipboard", null, sender);
    // ("Clear Custom Info Template", null, sender);
    // ("Clear Watch Entity Info", null, sender);
}

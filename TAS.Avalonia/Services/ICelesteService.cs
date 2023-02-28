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

    int GetPositionDecimals();
    void SetPositionDecimals(int value);
    
    int GetSpeedDecimals();
    void SetSpeedDecimals(int value);

    int GetVelocityDecimals();
    void SetVelocityDecimals(int value);

    int GetCustomInfoDecimals();
    void SetCustomInfoDecimals(int value);

    int GetSubpixelIndicatorDecimals();
    void SetSubpixelIndicatorDecimals(int value);

    int GetFastForwardSpeed();
    void SetFastForwardSpeed(int value);

    float GetSlowForwardSpeed();
    void SetSlowForwardSpeed(float value);
}

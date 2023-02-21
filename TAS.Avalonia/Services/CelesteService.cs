using JetBrains.Annotations;
using StudioCommunication;
using TAS.Avalonia.Communication;

namespace TAS.Avalonia.Services;

[UsedImplicitly]
public class CelesteService : BaseService, ICelesteService {
    private Dictionary<HotkeyID, List<Keys>> _bindings;
    private StudioInfo _state;

    public StudioCommunicationServer Server { get; }

    public CelesteService() {
        Server = new StudioCommunicationServer();
        Server.BindingsUpdated += bindings => _bindings = bindings;
        Server.StateUpdated += state => _state = state;
        Server.Run();
    }

    public void WriteWait() => Server.WriteWait();
    public void SendPath(string path) => Server.SendPath(path);

    public void Play() {
    }

    public void ToggleHitboxes() => Server.ToggleGameSetting("ShowHitboxes", null);
    public void ToggleTriggerHitboxes() => Server.ToggleGameSetting("ShowTriggerHitboxes", null);
    public void ToggleUnloadedRoomsHitboxes() => Server.ToggleGameSetting("ShowUnloadedRoomsHitboxes", null);
    public void ToggleCameraHitboxes() => Server.ToggleGameSetting("ShowCameraHitboxes", null);
    public void ToggleSimplifiedHitboxes() => Server.ToggleGameSetting("SimplifiedHitboxes", null);
    public void ToggleActualCollideHitboxes() => Server.ToggleGameSetting("ShowActualCollideHitboxes", null);
    public void ToggleSimplifiedGraphics() => Server.ToggleGameSetting("SimplifiedGraphics", null);
    public void ToggleGameplay() => Server.ToggleGameSetting("ShowGameplay", null);
    public void ToggleCenterCamera() => Server.ToggleGameSetting("CenterCamera", null);
    public void ToggleInfoHud() => Server.ToggleGameSetting("InfoHud", null);
    public void ToggleInfoTasInput() => Server.ToggleGameSetting("InfoTasInput", null);
    public void ToggleInfoGame() => Server.ToggleGameSetting("InfoGame", null);
    public void ToggleInfoWatchEntity() => Server.ToggleGameSetting("InfoWatchEntity", null);
    public void ToggleInfoCustom() => Server.ToggleGameSetting("InfoCustom", null);
    public void ToggleInfoSubpixelIndicator() => Server.ToggleGameSetting("InfoSubpixelIndicator", null);
    public void ToggleSpeedUnit() => Server.ToggleGameSetting("SpeedUnit", null);
}

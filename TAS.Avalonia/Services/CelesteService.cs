using System.Collections.Generic;
using JetBrains.Annotations;
using StudioCommunication;
using TAS.Avalonia.Communication;

namespace TAS.Avalonia.Services;

[UsedImplicitly]
public class CelesteService : BaseService, ICelesteService
{
    private Dictionary<HotkeyID, List<Keys>> _bindings;
    private StudioInfo _state;

    public StudioCommunicationServer Server { get; }

    public CelesteService()
    {
        Server = new StudioCommunicationServer();
        Server.BindingsUpdated += bindings => _bindings = bindings;
        Server.StateUpdated += state => _state = state;
        Server.Run();
    }

    public void WriteWait() => Server.WriteWait();
    public void SendPath(string path) => Server.SendPath(path);

    public void Play()
    {
    }

    public void ToggleHitboxes()
    {
        Server.ToggleGameSetting("ShowHitboxes", null);
    }
    
    public void ShowTriggerHitboxes() {
        Server.ToggleGameSetting("ShowTriggerHitboxes", null);
    }
    
    public void ShowUnloadedRoomsHitboxes() {
        Server.ToggleGameSetting("ShowUnloadedRoomsHitboxes", null);
    }
    
    public void ShowCameraHitboxes() {
        Server.ToggleGameSetting("ShowCameraHitboxes", null);
    }
    
    public void SimplifiedHitboxes() {
        Server.ToggleGameSetting("SimplifiedHitboxes", null);
    }
    
    public void ShowActualCollideHitboxes() {
        Server.ToggleGameSetting("ShowActualCollideHitboxes", null);
    }
    
    public void SimplifiedGraphics() {
        Server.ToggleGameSetting("SimplifiedGraphics", null);
    }
    
    public void ShowGameplay() {
        Server.ToggleGameSetting("ShowGameplay", null);
    }
    
    public void CenterCamera() {
        Server.ToggleGameSetting("CenterCamera", null);
    }
    
    public void InfoHud() {
        Server.ToggleGameSetting("InfoHud", null);
    }
    
    public void InfoTasInput() {
        Server.ToggleGameSetting("InfoTasInput", null);
    }
    
    public void InfoGame() {
        Server.ToggleGameSetting("InfoGame", null);
    }
    
    public void InfoWatchEntity() {
        Server.ToggleGameSetting("InfoWatchEntity", null);
    }
    
    public void InfoCustom() {
        Server.ToggleGameSetting("InfoCustom", null);
    }
    
    public void InfoSubpixelIndicator() {
        Server.ToggleGameSetting("InfoSubpixelIndicator", null);
    }
    
    public void SpeedUnit() {
        Server.ToggleGameSetting("SpeedUnit", null);
    }
}

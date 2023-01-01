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
}

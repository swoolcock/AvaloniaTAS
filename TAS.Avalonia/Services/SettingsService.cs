using ReactiveUI;
using System.ComponentModel;
using YamlDotNet.Serialization;

namespace TAS.Avalonia.Services;

public class SettingsService : ReactiveObject {
    private const string AppDataDirectory = "AvaloniaTAS";
    private const string DefaultSettingsFileName = "settings.yaml";

    private Settings _settings = new Settings();
    private readonly string _settingsFilePath;

    public string LastOpenFilePath {
        get => _settings.LastOpenFilePath;
        set => this.RaiseAndSetIfChanged(ref _settings.LastOpenFilePath, value);
    }
    public string LastOpenFileName => LastOpenFilePath == null ? null : Path.GetFileName(LastOpenFilePath);

    public bool SendInputs {
        get => _settings.SendInputs;
        set => this.RaiseAndSetIfChanged(ref _settings.SendInputs, value);
    }
    public bool GameInfoVisible {
        get => _settings.GameInfoVisible;
        set => this.RaiseAndSetIfChanged(ref _settings.GameInfoVisible, value);
    }

    public SettingsService() {
        var dataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _settingsFilePath = Path.Combine(dataDir, AppDataDirectory, DefaultSettingsFileName);
        LoadYaml(_settingsFilePath);
        PropertyChanged += OnPropertyChanged;
    }

    private void LoadYaml(string filePath) {
        if (!Path.Exists(filePath)) {
            SaveYaml(filePath);
            return;
        }

        string input = "";
        using (var fs = new FileStream(filePath, FileMode.Open))
        using (var sr = new StreamReader(fs)) {
            input = sr.ReadToEnd();
        }

        var deserializer = new Deserializer();
        _settings = deserializer.Deserialize<Settings>(input);
    }

    private void SaveYaml(string filePath) {
        _settings ??= new Settings();
        var serializer = new Serializer();
        var output = serializer.Serialize(_settings);

        if (!Path.Exists(filePath)) {
            if (Path.GetDirectoryName(filePath) is not { } directory) return;
            Directory.CreateDirectory(directory);
        }

        using var fs = new FileStream(filePath, FileMode.Create);
        using var sw = new StreamWriter(fs);
        sw.Write(output);
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        SaveYaml(_settingsFilePath);
    }

    [Serializable]
    public class Settings {
        public string LastOpenFilePath = "";

        public bool SendInputs = true;
        public bool GameInfoVisible = true;
    }
}

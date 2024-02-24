using Avalonia;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using ReactiveUI;

namespace TAS.Avalonia.Models;

public class TASDocument : ReactiveObject {
    private const string EmptyDocument = "RecordCount: 1\n\n#Start\n";

    public TextDocument Document { get; }

    private string _filePath;
    public string FilePath {
        get => _filePath;
        set {
            this.RaiseAndSetIfChanged(ref _filePath, value);
            this.RaisePropertyChanged(nameof(FileName));
        }
    }
    public string FileName => FilePath == null ? null : Path.GetFileName(FilePath);

    private bool _dirty;
    public bool Dirty {
        get => _dirty;
        private set {
            this.RaiseAndSetIfChanged(ref _dirty, value);
            if (value) Save();
        }
    }

    private TASDocument(string contents) {
        Document = new TextDocument(contents);
        Document.TextChanged += Document_TextChanged;

        (Application.Current as App)!.CelesteService.Server.LinesUpdated += OnLinesUpdated;
    }

    ~TASDocument() {
        (Application.Current as App)!.CelesteService.Server.LinesUpdated -= OnLinesUpdated;
    }

    public static TASDocument CreateBlank() => new TASDocument(EmptyDocument);

    public static TASDocument Load(string path) {
        try {
            string text = File.ReadAllText(path);
            return new TASDocument(text) {
                FilePath = path,
            };
        } catch (Exception e) {
            Console.WriteLine(e);
        }

        return null;
    }

    public void Save() {
        if (FilePath != null) {
            try {
                File.WriteAllText(FilePath, Document.Text);
                Dirty = false;
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }

    private void Document_TextChanged(object sender, EventArgs eventArgs) {
        Dirty = true;
    }

    private void OnLinesUpdated(Dictionary<int, string> lines) {
        Dispatcher.UIThread.Post(() => {
            foreach ((int lineNum, string newText) in lines) {
                var line = Document.GetLineByNumber(lineNum + 1); // 0-indexed
                Document.Replace(line, newText);
            }
        });
    }
}

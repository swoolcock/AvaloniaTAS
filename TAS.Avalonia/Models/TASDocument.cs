using AvaloniaEdit.Document;
using ReactiveUI;

namespace TAS.Avalonia.Models;

public class TASDocument : ReactiveObject {
    private const string EmptyDocument = "RecordCount: 1\n\n#Start\n";

    public TextDocument Document { get; }

    public string _filePath;
    public string FilePath {
        get => _filePath;
        set => this.RaiseAndSetIfChanged(ref _filePath, value);
    }
    public string FileName => FilePath == null ? null : Path.GetFileName(FilePath);

    private bool _dirty;
    public bool Dirty {
        get => _dirty;
        private set => this.RaiseAndSetIfChanged(ref _dirty, value);
    }

    private TASDocument(string contents) {
        Document = new TextDocument(contents);
        Document.TextChanged += Document_TextChanged;
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
}

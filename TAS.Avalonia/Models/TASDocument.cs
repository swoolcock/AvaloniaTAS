using AvaloniaEdit.Document;

namespace TAS.Avalonia.Models;

public class TASDocument {
    private const string EmptyDocument = "RecordCount: 1\n\n#Start\n";

    public TextDocument Document { get; }
    public string Filename { get; set; }
    public bool Dirty { get; private set; }

    private TASDocument(string contents) {
        Document = new TextDocument(contents);
        Document.TextChanged += Document_TextChanged;
    }

    public static TASDocument CreateBlank() => new TASDocument(EmptyDocument);

    public static TASDocument Load(string path) {
        try {
            string text = File.ReadAllText(path);
            return new TASDocument(text) {
                Filename = path,
            };
        } catch (Exception e) {
            Console.WriteLine(e);
        }

        return null;
    }

    public void Save(string path = null) {
        path ??= Filename;

        if (path != null) {
            try {
                File.WriteAllText(path, Document.Text);
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

namespace Lljt;

/// <summary>
/// The metadata block of a LibLouis translation table, parsed into a multi-map of key to values.
/// </summary>
/// <remarks>
/// Two rules from the LibLouis manual ("Table Metadata") shape this parser:
/// <list type="bullet">
/// <item>Metadata lives in the <em>table header</em> — the run of comment and empty lines at the top of
/// the file, before the first translation rule. Anything after that is not metadata, even if it looks
/// like it.</item>
/// <item>"The same key may appear multiple times in a table", so values are collected into a list
/// rather than overwriting.</item>
/// </list>
/// A field is a whole line of the form <c>#+key: value</c> (queryable metadata) or <c>#-key: value</c>
/// (informative metadata), where the key is a run of <c>a-z A-Z 0-9 . - _</c> and the colon may carry
/// spaces or tabs on either side. Matching the key is exact, not by substring: two tables
/// (<c>ancient-languages-us.utb</c> and <c>ancient-languages-borger.utb</c>) carry the word "language"
/// inside their display name, which a substring match would mistake for the language field.
/// </remarks>
internal sealed class TableHeader {
    private readonly Dictionary<string, List<string>> fields = new(StringComparer.OrdinalIgnoreCase);

    private TableHeader() { }

    /// <summary>Reads the header of <paramref name="path"/> and returns its metadata fields.</summary>
    public static TableHeader Read(string path) {
        var header = new TableHeader();

        foreach (string line in File.ReadLines(path)) {
            string trimmed = line.Trim();

            // The header ends at the first line that is neither empty nor a comment.
            if (trimmed.Length > 0 && trimmed[0] != '#') {
                break;
            }

            if (TryParseField(trimmed, out string key, out string value)) {
                header.Add(key, value);
            }
        }

        return header;
    }

    /// <summary>All values declared for <paramref name="key"/>, in declaration order, without
    /// duplicates. Empty when the key is absent.</summary>
    public IReadOnlyList<string> All(string key) =>
        this.fields.TryGetValue(key, out List<string>? values) ? values : [];

    /// <summary>The first value declared for <paramref name="key"/>, or <see langword="null"/> when the
    /// key is absent. Used for the keys that carry at most one value in practice.</summary>
    public string? First(string key) => this.All(key).FirstOrDefault();

    /// <summary>The first value of <paramref name="key"/> parsed as an integer, or 0 when the key is
    /// absent or not numeric — the "no dots metadata declared" case.</summary>
    public int FirstAsInt(string key) =>
        int.TryParse(this.First(key), out int parsed) ? parsed : 0;

    private void Add(string key, string value) {
        if (!this.fields.TryGetValue(key, out List<string>? values)) {
            values = [];
            this.fields[key] = values;
        }

        // A key may legitimately repeat with different values, but an exact repeat carries nothing
        // (en_US-comp8-ext.tbl declares "direction: both" twice), so keep the set distinct.
        if (!values.Contains(value, StringComparer.Ordinal)) {
            values.Add(value);
        }
    }

    private static bool TryParseField(string line, out string key, out string value) {
        key = string.Empty;
        value = string.Empty;

        // "#+" introduces queryable metadata, "#-" informative metadata. LibLouis uses "#-" for
        // display-name and index-name and "#+" for everything queryable; accept both prefixes and let
        // the key decide, so a future upstream move between them does not silently drop a field.
        if (line.Length < 3 || line[0] != '#' || (line[1] != '+' && line[1] != '-')) {
            return false;
        }

        int colon = line.IndexOf(':');
        if (colon < 0) {
            return false;
        }

        string rawKey = line[2..colon].Trim();
        if (rawKey.Length == 0 || !rawKey.All(IsKeyChar)) {
            return false;
        }

        key = rawKey;
        // Split on the first colon only: a value may contain further colons.
        value = line[(colon + 1)..].Trim();
        return value.Length > 0;
    }

    private static bool IsKeyChar(char c) =>
        char.IsAsciiLetterOrDigit(c) || c == '.' || c == '-' || c == '_';
}

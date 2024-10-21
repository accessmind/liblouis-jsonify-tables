namespace Lljt;

internal class Utils {
    public static string? ExtractFieldValueFromLine(string? line, string field) {
        if (line is null) {
            return null;
        }

        var parts = line?.Remove(0, 2).Split(":");

        if (parts[0] != field) {
            return null;
        }

        return parts[1]?.Trim();
    }
}

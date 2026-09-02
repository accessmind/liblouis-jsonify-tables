using System;
using System.IO;
using System.Text.Json;

namespace Lljt;

internal class Program {
    // Sub-tables meant to be pulled in by "include" rather than selected on their own. They carry no
    // display name, so they would be filtered out anyway; skipping them keeps the run quiet.
    private static readonly string[] ExcludedExtensions = [".dis", ".cti", ".uti"];

    static int Main(string[] args) {
        if (args.Length != 2) {
            Console.WriteLine("Usage: lljt <tables folder> <output JSON file name>");

            return ExitCode.Error;
        }

        string tablesFolder = args[0];
        string outputFile = args[1];

        if (!Directory.Exists(tablesFolder)) {
            Console.WriteLine("Tables folder does not exist. Please provide a valid folder.");

            return ExitCode.Error;
        }

        // Ordered by file name so a regenerated tables.json diffs cleanly against the previous one
        // instead of reshuffling with the directory enumeration order.
        var files = Directory.GetFiles(tablesFolder)
            .Where(file => !ExcludedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            .OrderBy(Path.GetFileName, StringComparer.Ordinal);
        var allTables = new List<TranslationTable>();

        foreach (var file in files) {
            allTables.Add(ReadTable(file));
        }

        // A table is only usable in a picker if it can be named and matched to a language, so those two
        // are the minimum bar for serialization.
        var tablesToSerialize = (from table in allTables
                                 where table.DisplayName is not null && table.Languages.Count > 0
                                 select table).ToList();
        using var jsonFile = File.Create(outputFile);
        var options = new JsonSerializerOptions { WriteIndented = true };
        JsonSerializer.Serialize(jsonFile, tablesToSerialize, options);
        Console.WriteLine("Done. Tables in total: {0}, serialized: {1}", allTables.Count, tablesToSerialize.Count);

        return ExitCode.Success;
    }

    private static TranslationTable ReadTable(string file) {
        TableHeader header = TableHeader.Read(file);

        return new TranslationTable(
            FileName: Path.GetFileName(file),
            DisplayName: header.First("display-name"),
            IndexName: header.First("index-name"),
            // Repeated by design: he-IL.utb declares he, ar and en; ancient-languages-us.utb declares 36.
            Languages: header.All("language"),
            Region: header.First("region"),
            // Also repeated by design: the Swedish and Elfdalian 8-dot tables are both computer and
            // literary braille.
            TableTypes: header.All("type"),
            ContractionType: header.First("contraction"),
            // Not an integer: alongside 0, 1, 2 and 3 the tables use 1.2, 1.3, 1.4 and 1.5.
            Grade: header.First("grade"),
            DotsMode: header.FirstAsInt("dots"),
            Direction: header.First("direction"),
            System: header.First("system"),
            Variant: header.First("variant"),
            Version: header.First("version"),
            Locale: header.First("locale"),
            UnicodeRange: header.First("unicode-range")
        );
    }
}

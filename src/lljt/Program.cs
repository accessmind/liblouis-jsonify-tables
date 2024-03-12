using System;
using System.IO;
using System.Text.Json;

namespace lljt;

internal class Program {
    static int Main(string[] args) {
        if (args.Length != 2) {
            Console.WriteLine("Usage: lljt <tables folder> <output JSON file name>");

            return 1;
        }

        string tablesFolder = args[0];
        string outputFile = args[1];

        if (!Directory.Exists(tablesFolder)) {
            Console.WriteLine("Tables folder does not exist. Please provide a valid folder.");

            return 1;
        }

        List<String> excludedExtensions = [".dis", ".cti", ".uti"];
        var files = from file in Directory.GetFiles(tablesFolder)
                    where !excludedExtensions.Contains(Path.GetExtension(file))
                    select file;
        var allTables = new List<TranslationTable>();

        foreach (var file in files) {
            var table = new TranslationTable();
            table.FileName = Path.GetFileName(file);

            // We read only lines starting with LibLouis metadata sign #+ or #- (the latter only for the display name)
            var text = File.ReadLines(file).Where(
                (string line) => line.TrimStart().StartsWith("#+") || line.TrimStart().StartsWith("#-")
            );

            #region Table fields
            var displayName = text.Where(line => line.Contains("display-name")).FirstOrDefault();
            displayName = Utils.ExtractFieldValueFromLine(displayName, "display-name");
            table.DisplayName = displayName;

            var language = text.Where(line => line.Contains("language")).FirstOrDefault();
            language = Utils.ExtractFieldValueFromLine(language, "language");
            table.Language = language;

            var tableType = text.Where(line => line.Contains("type")).FirstOrDefault();
            tableType = Utils.ExtractFieldValueFromLine(tableType, "type");
            table.TableType = tableType;

            var contractionType = text.Where(line => line.Contains("contraction")).FirstOrDefault();
            contractionType = Utils.ExtractFieldValueFromLine(contractionType, "contraction");
            table.ContractionType = contractionType;

            var direction = text.Where(line => line.Contains("direction")).FirstOrDefault();
            direction = Utils.ExtractFieldValueFromLine(direction, "direction");
            table.Direction = direction;

            var dotsMode = text.Where(line => line.Contains("dots")).FirstOrDefault();
            int dots;
            int.TryParse(Utils.ExtractFieldValueFromLine(dotsMode, "dots"), out dots);
            table.DotsMode = dots;
            #endregion

            allTables.Add(table);
        }

        var tablesToSerialize = from table in allTables
                                where table.DisplayName != null
                                && table.Language != null
                                select table;
        var jsonFile = File.Create(outputFile);
        var options = new JsonSerializerOptions { WriteIndented = true };
        JsonSerializer.Serialize(jsonFile, tablesToSerialize, options);
        Console.WriteLine("Done. Tables in total: {0}, serialized: {1}", allTables.Count.ToString(), tablesToSerialize.Count().ToString());
        return 0;
    }
}

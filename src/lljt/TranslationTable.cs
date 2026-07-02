namespace Lljt;

// Metadata fields other than FileName are optional in liblouis table headers, so they are nullable
// here. Tables missing DisplayName/Language are filtered out before serialization (see Program.cs).
public record struct TranslationTable(
    string FileName,
    string? DisplayName,
    string? Language,
    string? TableType,
    string? ContractionType,
    string? Direction,
    int DotsMode
);

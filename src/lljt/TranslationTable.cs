namespace Lljt;

public record struct TranslationTable(
    string FileName,
    string DisplayName,
    string Language,
    string TableType,
    string ContractionType,
    string Direction,
    int DotsMode
);

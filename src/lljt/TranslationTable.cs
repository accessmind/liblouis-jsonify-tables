namespace Lljt;

/// <summary>
/// The metadata LLJT extracts from one LibLouis translation table header.
/// </summary>
/// <remarks>
/// LibLouis declares queryable metadata as <c>#+key: value</c> lines and informative metadata as
/// <c>#-key: value</c> lines, both confined to the table header. Per the LibLouis manual "the same key
/// may appear multiple times in a table", which is why <see cref="Languages"/> and
/// <see cref="TableTypes"/> are lists: <c>he-IL.utb</c> declares three languages and
/// <c>ancient-languages-us.utb</c> declares thirty-six, while the Swedish and Elfdalian 8-dot tables
/// declare both a <c>computer</c> and a <c>literary</c> type. Every other key occurs at most once in
/// practice, so it is modelled as a single value.
///
/// All fields other than <see cref="FileName"/> are optional in a table header and so are nullable
/// here. Tables without a display name or without any language are dropped before serialization
/// (see Program.cs).
/// </remarks>
public record struct TranslationTable(
    string FileName,
    string? DisplayName,
    string? IndexName,
    IReadOnlyList<string> Languages,
    string? Region,
    IReadOnlyList<string> TableTypes,
    string? ContractionType,
    string? Grade,
    int DotsMode,
    string? Direction,
    string? System,
    string? Variant,
    string? Version,
    string? Locale,
    string? UnicodeRange
);

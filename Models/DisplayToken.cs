namespace KeyViz.Models;

internal sealed record DisplayToken(
    string Value,
    bool IsSpecial,
    string? SpecialLabel = null,
    int RepeatCount = 0);

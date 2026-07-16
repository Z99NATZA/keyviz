namespace KeyViz.Services;

internal static class UnicodeText
{
    internal static int CountCodePoints(string value)
    {
        var count = 0;

        for (var index = 0; index < value.Length; index += CodePointLengthAt(value, index))
        {
            count++;
        }

        return count;
    }

    internal static string RemoveFirstCodePoints(string value, int count)
    {
        var index = 0;

        while (index < value.Length && count > 0)
        {
            index += CodePointLengthAt(value, index);
            count--;
        }

        return value[index..];
    }

    internal static string RemoveLastCodePoint(string value)
    {
        if (value.Length == 0)
        {
            return value;
        }

        var codePointLength = value.Length >= 2
            && char.IsSurrogatePair(value[^2], value[^1])
                ? 2
                : 1;

        return value[..^codePointLength];
    }

    private static int CodePointLengthAt(string value, int index)
    {
        return index + 1 < value.Length && char.IsSurrogatePair(value[index], value[index + 1])
            ? 2
            : 1;
    }
}

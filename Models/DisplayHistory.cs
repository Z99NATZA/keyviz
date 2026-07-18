using System.Collections.ObjectModel;
using KeyViz.Services;

namespace KeyViz.Models;

internal sealed class DisplayHistory
{
    private int _maxLength;

    internal DisplayHistory(int maxLength)
    {
        _maxLength = Math.Max(1, maxLength);
    }

    internal ObservableCollection<DisplayToken> Tokens { get; } = [];

    internal bool CanStoreTokens => _maxLength > 0;

    internal int Length => Tokens.Sum(token => UnicodeText.CountCodePoints(token.Value));

    internal void SetMaxLength(int maxLength)
    {
        _maxLength = Math.Max(1, maxLength);
        Trim();
    }

    internal void AddSpecial(string label, bool replaceLastSpecial = false)
    {
        if (!CanStoreTokens)
        {
            return;
        }

        if (replaceLastSpecial && Tokens.Count > 0 && Tokens[^1].IsSpecial)
        {
            RemoveLastSpecialOccurrence();
        }

        if (Tokens.Count > 0
            && Tokens[^1].IsSpecial
            && Tokens[^1].SpecialLabel == label)
        {
            var lastToken = Tokens[^1];
            var repeatCount = lastToken.RepeatCount + 1;
            Tokens[^1] = lastToken with
            {
                Value = $"{label}*{repeatCount + 1}",
                RepeatCount = repeatCount
            };
        }
        else
        {
            Tokens.Add(new DisplayToken(label, IsSpecial: true, SpecialLabel: label));
        }

        Trim();
    }

    private void RemoveLastSpecialOccurrence()
    {
        var lastToken = Tokens[^1];
        if (lastToken.RepeatCount == 0)
        {
            Tokens.RemoveAt(Tokens.Count - 1);
            return;
        }

        var repeatCount = lastToken.RepeatCount - 1;
        Tokens[^1] = lastToken with
        {
            Value = repeatCount == 0
                ? lastToken.SpecialLabel!
                : $"{lastToken.SpecialLabel}*{repeatCount + 1}",
            RepeatCount = repeatCount
        };
    }

    internal void AppendText(string text)
    {
        if (!CanStoreTokens || text.Length == 0)
        {
            return;
        }

        if (Tokens.Count > 0 && !Tokens[^1].IsSpecial)
        {
            var lastToken = Tokens[^1];
            Tokens[^1] = lastToken with { Value = lastToken.Value + text };
        }
        else
        {
            Tokens.Add(new DisplayToken(text, IsSpecial: false));
        }

        Trim();
    }

    internal void RemoveLastTextCodePoint()
    {
        for (var index = Tokens.Count - 1; index >= 0; index--)
        {
            var token = Tokens[index];
            if (token.IsSpecial)
            {
                continue;
            }

            var value = UnicodeText.RemoveLastCodePoint(token.Value);
            if (value.Length == 0)
            {
                Tokens.RemoveAt(index);
            }
            else
            {
                Tokens[index] = token with { Value = value };
            }

            return;
        }
    }

    internal void Clear()
    {
        Tokens.Clear();
    }

    private void Trim()
    {
        var excess = Length - _maxLength;

        for (var index = 0; index < Tokens.Count && excess > 0;)
        {
            var token = Tokens[index];
            var tokenLength = UnicodeText.CountCodePoints(token.Value);

            if (token.IsSpecial || tokenLength <= excess)
            {
                excess -= tokenLength;
                Tokens.RemoveAt(index);
                continue;
            }

            Tokens[index] = token with
            {
                Value = UnicodeText.RemoveFirstCodePoints(token.Value, excess)
            };
            excess = 0;
        }

        MergeAdjacentTextTokens();
    }

    private void MergeAdjacentTextTokens()
    {
        for (var index = 0; index < Tokens.Count - 1;)
        {
            if (!Tokens[index].IsSpecial && !Tokens[index + 1].IsSpecial)
            {
                Tokens[index] = Tokens[index] with
                {
                    Value = Tokens[index].Value + Tokens[index + 1].Value
                };
                Tokens.RemoveAt(index + 1);
                continue;
            }

            index++;
        }
    }
}

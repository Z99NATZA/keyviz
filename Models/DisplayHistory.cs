using System.Collections.ObjectModel;
using KeyViz.Services;

namespace KeyViz.Models;

internal sealed class DisplayHistory
{
    private readonly int _maxLength;

    internal DisplayHistory(int maxLength)
    {
        _maxLength = maxLength;
    }

    internal ObservableCollection<DisplayToken> Tokens { get; } = [];

    internal bool CanStoreTokens => _maxLength > 0;

    internal int Length => Tokens.Sum(token => UnicodeText.CountCodePoints(token.Value));

    internal void AddSpecial(string label, bool replaceLastSpecial = false)
    {
        if (!CanStoreTokens)
        {
            return;
        }

        if (replaceLastSpecial && Tokens.Count > 0 && Tokens[^1].IsSpecial)
        {
            Tokens.RemoveAt(Tokens.Count - 1);
        }

        Tokens.Add(new DisplayToken(label, IsSpecial: true));
        Trim();
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

using System.Text;

namespace PlayBuilder.Services.Atlas;

/// <summary>
/// Splits a ROM filename into title text and metadata tags without interpreting them.
/// </summary>
public sealed class FilenameTokenizer
{
    public IReadOnlyList<FilenameToken> Tokenize(string filename)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);

        var value = Path.GetFileNameWithoutExtension(filename).Trim();
        var tokens = new List<FilenameToken>();
        var text = new StringBuilder();
        var textStart = 0;

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            var closingCharacter = character switch
            {
                '(' => ')',
                '[' => ']',
                _ => '\0'
            };

            if (closingCharacter == '\0')
            {
                if (text.Length == 0)
                {
                    textStart = index;
                }

                text.Append(character);
                continue;
            }

            var closingIndex = value.IndexOf(closingCharacter, index + 1);
            if (closingIndex < 0)
            {
                text.Append(character);
                continue;
            }

            FlushText(tokens, text, textStart);

            var tag = value[(index + 1)..closingIndex].Trim();
            if (tag.Length > 0)
            {
                tokens.Add(new FilenameToken(
                    character == '(' ? FilenameTokenType.ParentheticalTag : FilenameTokenType.BracketTag,
                    tag,
                    index));
            }

            index = closingIndex;
        }

        FlushText(tokens, text, textStart);
        return tokens;
    }

    private static void FlushText(
        ICollection<FilenameToken> tokens,
        StringBuilder text,
        int position)
    {
        var value = text.ToString().Trim();
        if (value.Length > 0)
        {
            tokens.Add(new FilenameToken(FilenameTokenType.Text, value, position));
        }

        text.Clear();
    }
}

using PlayBuilder.Services.Atlas;

namespace PlayBuilder.Tests.Atlas;

public sealed class FilenameTokenizerTests
{
    private readonly FilenameTokenizer _tokenizer = new();

    [Fact]
    public void Tokenize_SeparatesTitleAndTagTypes()
    {
        var tokens = _tokenizer.Tokenize("Example Game (USA) [!].zip");

        Assert.Collection(tokens,
            token =>
            {
                Assert.Equal(FilenameTokenType.Text, token.Type);
                Assert.Equal("Example Game", token.Value);
            },
            token =>
            {
                Assert.Equal(FilenameTokenType.ParentheticalTag, token.Type);
                Assert.Equal("USA", token.Value);
            },
            token =>
            {
                Assert.Equal(FilenameTokenType.BracketTag, token.Type);
                Assert.Equal("!", token.Value);
            });
    }

    [Fact]
    public void Tokenize_UnclosedTagRemainsTitleText()
    {
        var tokens = _tokenizer.Tokenize("Example Game (Prototype.zip");

        var token = Assert.Single(tokens);
        Assert.Equal(FilenameTokenType.Text, token.Type);
        Assert.Equal("Example Game (Prototype", token.Value);
    }
}

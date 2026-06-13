using DA_Common;

namespace DA_Business.Tests;

public class RichTextTests
{
    [Fact]
    public void ToQuillHtml_ConvertsRichTextBlockToParagraphs()
    {
        var rich = new RichText();
        rich += $"{RichText.BoldText("Hero")} is making a check";
        rich.NewLine();
        rich += "skill value: 5";
        rich.EndText();

        var quill = rich.ToQuillHtml();

        Assert.Contains("<p>", quill);
        Assert.Contains("<strong>Hero</strong>", quill);
        Assert.Contains("skill value: 5", quill);
        Assert.DoesNotContain("<blockquote>", quill);
        Assert.DoesNotContain("background-color", quill);
    }

    [Fact]
    public void ToThreadPostQuillHtml_WrapsContentInItalicParentheses()
    {
        var rich = new RichText();
        rich += "Hero rolls";
        rich.NewLine();
        rich += "Success!";
        rich.EndText();

        var thread = RichText.ToThreadPostQuillHtml(rich.AllText);

        Assert.StartsWith("<p><em>(", thread);
        Assert.EndsWith(")</em></p>", thread);
        Assert.Contains("Hero rolls Success!", thread);
        Assert.DoesNotContain("<br>", thread);
        Assert.DoesNotContain("<blockquote>", thread);
    }

    [Fact]
    public void ToFightQuillHtml_WrapsContentInItalic()
    {
        var rich = new RichText();
        rich += $"{RichText.BoldText("Hero")} attacks";
        rich.NewLine();
        rich += "Hit!";
        rich.EndText();

        var quill = RichText.ToFightQuillHtml(rich.AllText);

        Assert.Contains("<p><em>", quill);
        Assert.Contains("<strong>Hero</strong>", quill);
        Assert.DoesNotContain("<blockquote>", quill);
    }

    [Fact]
    public void ToThreadFightPostQuillHtml_WrapsContentInItalicParentheses()
    {
        var rich = new RichText();
        rich += $"{RichText.BoldText("Hero")} attacks";
        rich.EndText();

        var thread = RichText.ToThreadFightPostQuillHtml(rich.AllText);

        Assert.StartsWith("<p><em>(", thread);
        Assert.EndsWith(")</em></p>", thread);
        Assert.Contains("<strong>Hero</strong>", thread);
        Assert.DoesNotContain("<blockquote>", thread);
    }

    [Fact]
    public void ToQuillHtml_ConvertsMultipleBlocks()
    {
        var block1 = new RichText();
        block1 += "first roll";
        block1.EndText();

        var block2 = new RichText();
        block2 += "second roll";
        block2.EndText();

        var quill = RichText.ToQuillHtml(block1.AllText + block2.AllText);

        Assert.Contains("first roll", quill);
        Assert.Contains("second roll", quill);
    }
}

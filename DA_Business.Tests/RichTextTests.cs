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

        Assert.Contains("<blockquote", quill);
        Assert.Contains(RichText.QuoteBlockClass, quill);
        Assert.Contains("<strong>Hero</strong>", quill);
        Assert.Contains("skill value: 5", quill);
        Assert.DoesNotContain("background-color: #eaeaea", quill);
    }

    [Fact]
    public void ToThreadPostQuillHtml_PreservesQuoteBlock()
    {
        var rich = new RichText();
        rich += "Hero rolls";
        rich.NewLine();
        rich += "Success!";
        rich.EndText();

        var thread = RichText.ToThreadPostQuillHtml(rich.AllText);

        Assert.Contains(RichText.QuoteBlockClass, thread);
        Assert.Contains("<blockquote", thread);
        Assert.Contains("Hero rolls", thread);
        Assert.Contains("Success!", thread);
        Assert.DoesNotContain("<br>", thread);
    }

    [Fact]
    public void CollapseToSingleParagraph_MergesMultipleParagraphs()
    {
        var html = "<p><em>line one</em></p><p><em>line two</em></p>";

        var collapsed = RichText.CollapseToSingleParagraph(html);

        Assert.Equal("<p><em>line one line two</em></p>", collapsed);
        Assert.DoesNotContain("<br>", collapsed);
        Assert.DoesNotContain("</p><p>", collapsed);
    }

    [Fact]
    public void ToPlainEditorHtml_OmitsBlockquoteWrapper()
    {
        var rich = new RichText();
        rich += "Hero rolls";
        rich.EndText();

        var editor = RichText.ToPlainEditorHtml(rich.AllText);

        Assert.DoesNotContain("<blockquote", editor);
        Assert.Contains("Hero rolls", editor);
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
        Assert.Contains(RichText.QuoteBlockClass, RichText.ToQuillHtml(rich.AllText));
    }

    [Fact]
    public void ToFightEditorHtml_UsesParagraphsWithoutBlockquote()
    {
        var rich = new RichText();
        rich += $"{RichText.BoldText("Hero")} attacks";
        rich.NewLine();
        rich += "Hit!";
        rich.EndText();

        var editor = RichText.ToFightEditorHtml(rich.AllText);

        Assert.DoesNotContain("<blockquote", editor);
        Assert.Contains("<p><em>", editor);
        Assert.Contains("<strong>Hero</strong>", editor);
        Assert.Contains("Hit!", editor);
    }

    [Fact]
    public void ToThreadFightPostQuillHtml_WrapsContentInQuoteBlockWithItalic()
    {
        var rich = new RichText();
        rich += $"{RichText.BoldText("Hero")} attacks";
        rich.EndText();

        var thread = RichText.ToThreadFightPostQuillHtml(rich.AllText);

        Assert.Contains(RichText.QuoteBlockClass, thread);
        Assert.Contains("<blockquote", thread);
        Assert.Contains("<em>", thread);
        Assert.Contains("<strong>Hero</strong>", thread);
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

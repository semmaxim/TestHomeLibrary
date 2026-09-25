using System.Xml.Linq;
using TestHomeLibrary.Domain.Exceptions;
using TestHomeLibrary.Infrastructure.Html;

namespace TestHomeLibrary.UnitTests;

public sealed class HtmlTableOfContentsConverterTests
{
    private readonly HtmlTableOfContentsConverter _converter = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToXml_NullOrWhitespace_ReturnsNull(string? html)
    {
        Assert.Null(_converter.ToXml(html));
    }

    [Fact]
    public void ToXml_SimpleHtml_ProducesValidXmlDocument()
    {
        var xml = _converter.ToXml("<p>Chapter 1</p><h2>Part I</h2>");

        Assert.NotNull(xml);
        var document = XDocument.Parse(xml!);
        Assert.Equal("toc", document.Root!.Name.LocalName);
        Assert.Contains("Chapter 1", xml);
    }

    [Fact]
    public void ToXml_UnclosedTagsAndBr_ProducesWellFormedXml()
    {
        var xml = _converter.ToXml("<div><p>Line one<br><p>Line two</div>");

        Assert.NotNull(xml);
        var document = XDocument.Parse(xml!);
        Assert.Equal("toc", document.Root!.Name.LocalName);
    }

    [Fact]
    public void ToXml_HtmlNamedEntities_AreConvertedToValidXml()
    {
        var xml = _converter.ToXml("<p>A&nbsp;B &copy; C</p>");

        Assert.NotNull(xml);
        var document = XDocument.Parse(xml!);
        var text = string.Concat(document.Root!.DescendantNodes().OfType<XText>().Select(t => t.Value));
        Assert.Contains("\u00A0", text);
        Assert.DoesNotContain("&nbsp;", xml);
    }

    [Fact]
    public void ToXml_ComplexEditorHtml_ProducesSearchableXml()
    {
        var html = "<h2>Том I</h2><ol><li>Часть первая</li><li>Часть вторая</li></ol><table><tr><td>Стр. 5</td></tr></table>";

        var xml = _converter.ToXml(html);

        Assert.NotNull(xml);
        XDocument.Parse(xml!);
        Assert.Contains("Часть первая", xml);
    }

    [Fact]
    public void ToHtml_ValidTocXml_ReturnsInnerHtml()
    {
        const string xml = "<toc><h2>Part I</h2><ol><li>Chapter 1</li></ol></toc>";

        var html = _converter.ToHtml(xml);

        Assert.NotNull(html);
        Assert.Contains("<h2>Part I</h2>", html);
        Assert.Contains("Chapter 1", html);
        Assert.DoesNotContain("<toc>", html);
    }

    [Fact]
    public void ToHtml_NullOrWhitespace_ReturnsNull()
    {
        Assert.Null(_converter.ToHtml(null));
        Assert.Null(_converter.ToHtml("  "));
    }

    [Fact]
    public void ToHtml_InvalidXml_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => _converter.ToHtml("<toc><broken>"));
    }

    [Fact]
    public void RoundTrip_XmlToHtmlAndBack_KeepsContent()
    {
        var xml = _converter.ToXml("<h2>Глава 1</h2><p>Текст &amp; пример</p>");
        Assert.NotNull(xml);

        var html = _converter.ToHtml(xml);
        Assert.NotNull(html);

        var xmlAgain = _converter.ToXml(html);
        Assert.NotNull(xmlAgain);
        XDocument.Parse(xmlAgain!);
        Assert.Contains("Глава 1", xmlAgain);
        Assert.Contains("пример", xmlAgain);
    }
}

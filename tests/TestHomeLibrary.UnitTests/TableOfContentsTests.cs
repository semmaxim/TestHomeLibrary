using TestHomeLibrary.Domain.Exceptions;
using TestHomeLibrary.Domain.ValueObjects;

namespace TestHomeLibrary.UnitTests;

public sealed class TableOfContentsTests
{
    [Fact]
    public void FromXml_NullOrWhitespace_ReturnsNull()
    {
        Assert.Null(TableOfContents.FromXml(null));
        Assert.Null(TableOfContents.FromXml("   "));
    }

    [Fact]
    public void FromXml_ValidXml_ReturnsInstance()
    {
        const string xml = "<toc><ol><li>Chapter 1</li></ol></toc>";

        var result = TableOfContents.FromXml(xml);

        Assert.NotNull(result);
        Assert.Equal(xml, result!.Xml);
    }

    [Fact]
    public void FromXml_InvalidXml_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => TableOfContents.FromXml("<toc><li>unclosed</toc>"));
    }
}

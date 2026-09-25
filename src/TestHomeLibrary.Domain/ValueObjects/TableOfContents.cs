using System.Xml.Linq;
using TestHomeLibrary.Domain.Exceptions;

namespace TestHomeLibrary.Domain.ValueObjects;

public sealed class TableOfContents
{
    public string Xml { get; }

    private TableOfContents(string xml) => Xml = xml;

    public static TableOfContents? FromXml(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return null;
        }

        var trimmed = xml.Trim();
        Validate(trimmed);
        return new TableOfContents(trimmed);
    }

    private static void Validate(string xml)
    {
        try
        {
            XDocument.Parse(xml);
        }
        catch (Exception ex)
        {
            throw new DomainValidationException("Оглавление должно быть корректным XML-документом.", ex);
        }
    }
}

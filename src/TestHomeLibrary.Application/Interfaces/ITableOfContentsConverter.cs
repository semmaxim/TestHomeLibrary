namespace TestHomeLibrary.Application.Interfaces;

public interface ITableOfContentsConverter
{
    string? ToXml(string? html);

    string? ToHtml(string? xml);
}

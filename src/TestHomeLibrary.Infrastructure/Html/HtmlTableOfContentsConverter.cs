using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using HtmlAgilityPack;
using TestHomeLibrary.Application.Interfaces;
using TestHomeLibrary.Domain.Exceptions;

namespace TestHomeLibrary.Infrastructure.Html;

public sealed class HtmlTableOfContentsConverter : ITableOfContentsConverter
{
    private static readonly Regex EscapedNamedEntityRegex = new(
        @"&amp;(?<name>[a-zA-Z][a-zA-Z0-9]{1,31});",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex NamedEntityRegex = new(
        @"&(?!amp;|lt;|gt;|quot;|apos;|#)(?<name>[a-zA-Z][a-zA-Z0-9]{1,31});",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string? ToXml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var document = new HtmlDocument
        {
            OptionOutputAsXml = true,
            OptionWriteEmptyNodes = true
        };
        document.LoadHtml(html);

        var body = document.DocumentNode.SelectSingleNode("//body");
        var fragment = (body?.InnerHtml ?? document.DocumentNode.InnerHtml).Trim();
        if (fragment.Length == 0)
        {
            return null;
        }

        var fixedFragment = ReplaceNamedEntities(fragment);
        var wrapped = $"<toc>{fixedFragment}</toc>";

        try
        {
            XDocument.Parse(wrapped);
        }
        catch (Exception ex)
        {
            throw new DomainValidationException("Не удалось сохранить оглавление: HTML содержит некорректную разметку.", ex);
        }

        return wrapped;
    }

    public string? ToHtml(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return null;
        }

        try
        {
            var document = XDocument.Parse(xml);
            if (document.Root is null)
            {
                return null;
            }

            return string.Concat(document.Root.Nodes().Select(node => node.ToString(SaveOptions.DisableFormatting)));
        }
        catch (Exception ex) when (ex is not DomainValidationException)
        {
            throw new DomainValidationException("Оглавление содержит некорректный XML.", ex);
        }
    }

    private static string ReplaceNamedEntities(string html)
    {
        html = EscapedNamedEntityRegex.Replace(html, match =>
            DecodeEntity("&" + match.Groups["name"].Value + ";", match.Value));

        return NamedEntityRegex.Replace(html, match => DecodeEntity(match.Value, match.Value));
    }

    private static string DecodeEntity(string entity, string fallback)
    {
        var decoded = WebUtility.HtmlDecode(entity);
        if (string.IsNullOrEmpty(decoded) || decoded == entity)
        {
            return fallback;
        }

        return decoded
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}

using TestHomeLibrary.Application.DTOs;
using TestHomeLibrary.Application.Interfaces;
using TestHomeLibrary.Application.Mapping;
using TestHomeLibrary.Application.Models;
using TestHomeLibrary.Domain.Entities;
using TestHomeLibrary.Domain.Exceptions;
using TestHomeLibrary.Domain.Interfaces;

namespace TestHomeLibrary.Application.Services;

public sealed class BookService : IBookService
{
    private readonly IBookRepository _repository;
    private readonly ITableOfContentsConverter _tableOfContentsConverter;

    public BookService(IBookRepository repository, ITableOfContentsConverter tableOfContentsConverter)
    {
        _repository = repository;
        _tableOfContentsConverter = tableOfContentsConverter;
    }

    public async Task<BookDto> CreateAsync(BookEditModel model, CancellationToken cancellationToken = default)
    {
        var tableOfContentsXml = _tableOfContentsConverter.ToXml(model.TableOfContentsHtml);

        var book = Book.Create(
            model.Title,
            model.Author,
            model.PublicationYear,
            model.Publisher,
            model.Isbn,
            model.PageCount,
            model.Genre,
            model.Notes,
            tableOfContentsXml);

        var id = await _repository.AddAsync(book, cancellationToken);
        var created = await _repository.GetByIdAsync(id, cancellationToken)
                      ?? throw new BookNotFoundException(id);

        return ToDto(created);
    }

    public async Task<BookDto> UpdateAsync(BookEditModel model, CancellationToken cancellationToken = default)
    {
        var book = await _repository.GetByIdAsync(model.Id, cancellationToken)
                   ?? throw new BookNotFoundException(model.Id);

        var tableOfContentsXml = _tableOfContentsConverter.ToXml(model.TableOfContentsHtml);

        book.Update(
            model.Title,
            model.Author,
            model.PublicationYear,
            model.Publisher,
            model.Isbn,
            model.PageCount,
            model.Genre,
            model.Notes,
            tableOfContentsXml,
            DateTime.UtcNow);

        await _repository.UpdateAsync(book, cancellationToken);
        return ToDto(book);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new BookNotFoundException(id);
        }
    }

    public async Task<BookDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _repository.GetByIdAsync(id, cancellationToken);
        return book is null ? null : ToDto(book);
    }

    public async Task<PagedResult<BookListItemDto>> GetListAsync(
        int page,
        int pageSize,
        string sortField,
        string sortDirection,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetListAsync(page, pageSize, sortField, sortDirection, cancellationToken);
        return new PagedResult<BookListItemDto>(
            result.Items.Select(BookMapper.ToListItem).ToList(),
            result.TotalCount,
            NormalizePage(page),
            NormalizePageSize(pageSize));
    }

    public async Task<PagedResult<BookListItemDto>> SearchAsync(
        string searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var term = searchTerm?.Trim() ?? string.Empty;
        if (term.Length == 0)
        {
            return new PagedResult<BookListItemDto>([], 0, NormalizePage(page), NormalizePageSize(pageSize));
        }

        var result = await _repository.SearchAsync(term, NormalizePage(page), NormalizePageSize(pageSize), cancellationToken);
        return new PagedResult<BookListItemDto>(
            result.Items.Select(BookMapper.ToListItem).ToList(),
            result.TotalCount,
            NormalizePage(page),
            NormalizePageSize(pageSize));
    }

    private BookDto ToDto(Book book)
    {
        var html = _tableOfContentsConverter.ToHtml(book.TableOfContents?.Xml);
        return BookMapper.ToDto(book, html);
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => pageSize
    };
}

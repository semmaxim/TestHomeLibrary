using TestHomeLibrary.Application.DTOs;
using TestHomeLibrary.Application.Models;

namespace TestHomeLibrary.Application.Interfaces;

public interface IBookService
{
    Task<BookDto> CreateAsync(BookEditModel model, CancellationToken cancellationToken = default);

    Task<BookDto> UpdateAsync(BookEditModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<BookDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<BookListItemDto>> GetListAsync(
        int page,
        int pageSize,
        string sortField,
        string sortDirection,
        CancellationToken cancellationToken = default);

    Task<PagedResult<BookListItemDto>> SearchAsync(
        string searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

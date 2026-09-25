using TestHomeLibrary.Domain.Entities;

namespace TestHomeLibrary.Domain.Interfaces;

public interface IBookRepository
{
    Task<int> AddAsync(Book book, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<BookListPage> GetListAsync(
        int page,
        int pageSize,
        string sortField,
        string sortDirection,
        CancellationToken cancellationToken = default);

    Task<BookListPage> SearchAsync(
        string searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

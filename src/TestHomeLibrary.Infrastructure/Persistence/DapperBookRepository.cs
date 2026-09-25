using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using TestHomeLibrary.Domain.Entities;
using TestHomeLibrary.Domain.Interfaces;

namespace TestHomeLibrary.Infrastructure.Persistence;

public sealed class DapperBookRepository : IBookRepository
{
    private readonly string _connectionString;

    public DapperBookRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        var parameters = CreateBookParameters(book);
        parameters.Add("@BookId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            new CommandDefinition("BooksInsert", parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));

        return parameters.Get<int>("@BookId");
    }

    public async Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        var parameters = CreateBookParameters(book);
        parameters.Add("@BookId", book.Id);

        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition("BooksUpdate", parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(
                "BooksDelete",
                new { BookId = id },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return affectedRows > 0;
    }

    public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        var row = await connection.QuerySingleOrDefaultAsync<BookRow>(
            new CommandDefinition(
                "BooksGetById",
                new { BookId = id },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return row?.ToBook();
    }

    public async Task<BookListPage> GetListAsync(
        int page,
        int pageSize,
        string sortField,
        string sortDirection,
        CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition("BooksCount", commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));

        var rows = await connection.QueryAsync<BookRow>(
            new CommandDefinition(
                "BooksGetList",
                new { Page = page, PageSize = pageSize, SortField = sortField, SortDir = sortDirection },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new BookListPage(rows.Select(r => r.ToBook()).ToList(), totalCount);
    }

    public async Task<BookListPage> SearchAsync(
        string searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                "BooksSearchCount",
                new { SearchTerm = searchTerm },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var rows = await connection.QueryAsync<BookRow>(
            new CommandDefinition(
                "BooksSearch",
                new { SearchTerm = searchTerm, Page = page, PageSize = pageSize },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new BookListPage(rows.Select(r => r.ToBook()).ToList(), totalCount);
    }

    private static DynamicParameters CreateBookParameters(Book book)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Title", book.Title);
        parameters.Add("@Author", book.Author);
        parameters.Add("@PublicationYear", (short)book.PublicationYear.Value);
        parameters.Add("@Publisher", book.Publisher);
        parameters.Add("@Isbn", book.Isbn);
        parameters.Add("@PageCount", book.PageCount);
        parameters.Add("@Genre", book.Genre);
        parameters.Add("@Notes", book.Notes);
        parameters.Add("@TableOfContents", book.TableOfContents?.Xml, dbType: DbType.String);
        return parameters;
    }

    private sealed class BookRow
    {
        public int BookId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public short PublicationYear { get; init; }
        public string? Publisher { get; init; }
        public string? Isbn { get; init; }
        public int? PageCount { get; init; }
        public string? Genre { get; init; }
        public string? Notes { get; init; }
        public string? TableOfContents { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }

        public Book ToBook() => Book.Restore(
            BookId,
            Title,
            Author,
            PublicationYear,
            Publisher,
            Isbn,
            PageCount,
            Genre,
            Notes,
            TableOfContents,
            CreatedAt,
            UpdatedAt);
    }
}

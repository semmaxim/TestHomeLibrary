namespace TestHomeLibrary.Domain.Entities;

public sealed record BookListPage(IReadOnlyList<Book> Items, int TotalCount);

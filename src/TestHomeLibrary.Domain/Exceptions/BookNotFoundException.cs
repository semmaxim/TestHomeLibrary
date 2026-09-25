namespace TestHomeLibrary.Domain.Exceptions;

public sealed class BookNotFoundException : DomainException
{
    public int BookId { get; }

    public BookNotFoundException(int bookId)
        : base($"Книга с идентификатором {bookId} не найдена.")
    {
        BookId = bookId;
    }
}

USE [TestHomeLibrary];
GO

CREATE OR ALTER PROCEDURE dbo.BooksInsert
    @Title NVARCHAR(500),
    @Author NVARCHAR(300),
    @PublicationYear SMALLINT,
    @Publisher NVARCHAR(300) = NULL,
    @Isbn NVARCHAR(20) = NULL,
    @PageCount INT = NULL,
    @Genre NVARCHAR(100) = NULL,
    @Notes NVARCHAR(MAX) = NULL,
    @TableOfContents XML = NULL,
    @BookId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Books (Title, Author, PublicationYear, Publisher, Isbn, PageCount, Genre, Notes, TableOfContents, CreatedAt)
    VALUES (@Title, @Author, @PublicationYear, @Publisher, @Isbn, @PageCount, @Genre, @Notes, @TableOfContents, SYSUTCDATETIME());

    SET @BookId = CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.BooksUpdate
    @BookId INT,
    @Title NVARCHAR(500),
    @Author NVARCHAR(300),
    @PublicationYear SMALLINT,
    @Publisher NVARCHAR(300) = NULL,
    @Isbn NVARCHAR(20) = NULL,
    @PageCount INT = NULL,
    @Genre NVARCHAR(100) = NULL,
    @Notes NVARCHAR(MAX) = NULL,
    @TableOfContents XML = NULL
AS
BEGIN
    UPDATE dbo.Books
    SET Title = @Title,
        Author = @Author,
        PublicationYear = @PublicationYear,
        Publisher = @Publisher,
        Isbn = @Isbn,
        PageCount = @PageCount,
        Genre = @Genre,
        Notes = @Notes,
        TableOfContents = @TableOfContents,
        UpdatedAt = SYSUTCDATETIME()
    WHERE BookId = @BookId;
END
GO

CREATE OR ALTER PROCEDURE dbo.BooksDelete
    @BookId INT
AS
BEGIN
    DELETE FROM dbo.Books
    WHERE BookId = @BookId;
END
GO

CREATE OR ALTER PROCEDURE dbo.BooksGetById
    @BookId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT BookId,
           Title,
           Author,
           PublicationYear,
           Publisher,
           Isbn,
           PageCount,
           Genre,
           Notes,
           CAST(TableOfContents AS NVARCHAR(MAX)) AS TableOfContents,
           CreatedAt,
           UpdatedAt
    FROM dbo.Books
    WHERE BookId = @BookId;
END
GO

CREATE OR ALTER PROCEDURE dbo.BooksCount
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*) AS TotalCount
    FROM dbo.Books;
END
GO

CREATE OR ALTER PROCEDURE dbo.BooksGetList
    @Page INT = 1,
    @PageSize INT = 20,
    @SortField NVARCHAR(50) = N'Title',
    @SortDir NVARCHAR(4) = N'ASC'
AS
BEGIN
    SET NOCOUNT ON;

    IF @Page < 1
        SET @Page = 1;

    IF @PageSize < 1
        SET @PageSize = 20;
    ELSE IF @PageSize > 100
        SET @PageSize = 100;

    SET @SortDir = CASE WHEN LOWER(@SortDir) = N'desc' THEN N'DESC' ELSE N'ASC' END;

    DECLARE @OrderBy NVARCHAR(200) = CASE LOWER(@SortField)
        WHEN N'title' THEN N'Title'
        WHEN N'author' THEN N'Author'
        WHEN N'year' THEN N'PublicationYear'
        WHEN N'publisher' THEN N'Publisher'
        WHEN N'isbn' THEN N'Isbn'
        ELSE N'Title'
    END + N' ' + @SortDir + N', BookId ASC';

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    DECLARE @Sql NVARCHAR(MAX) = N'
        SELECT BookId,
               Title,
               Author,
               PublicationYear,
               Publisher,
               Isbn,
               PageCount,
               Genre,
               Notes,
               CAST(TableOfContents AS NVARCHAR(MAX)) AS TableOfContents,
               CreatedAt,
               UpdatedAt
        FROM dbo.Books
        ORDER BY ' + @OrderBy + N'
        OFFSET @OffsetRows ROWS FETCH NEXT @PageSizeRows ROWS ONLY;';

    EXEC sys.sp_executesql
        @Sql,
        N'@OffsetRows INT, @PageSizeRows INT',
        @OffsetRows = @Offset,
        @PageSizeRows = @PageSize;
END
GO

CREATE OR ALTER PROCEDURE dbo.BooksSearchCount
    @SearchTerm NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Pattern NVARCHAR(202) = N'%' + REPLACE(REPLACE(REPLACE(@SearchTerm, N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]') + N'%';

    SELECT COUNT(*) AS TotalCount
    FROM dbo.Books
    WHERE Title LIKE @Pattern ESCAPE N'['
       OR Author LIKE @Pattern ESCAPE N'['
       OR CAST(TableOfContents AS NVARCHAR(MAX)) LIKE @Pattern ESCAPE N'[';
END
GO

CREATE OR ALTER PROCEDURE dbo.BooksSearch
    @SearchTerm NVARCHAR(200),
    @Page INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @Page < 1
        SET @Page = 1;

    IF @PageSize < 1
        SET @PageSize = 20;
    ELSE IF @PageSize > 100
        SET @PageSize = 100;

    DECLARE @Pattern NVARCHAR(202) = N'%' + REPLACE(REPLACE(REPLACE(@SearchTerm, N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]') + N'%';
    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    SELECT BookId,
           Title,
           Author,
           PublicationYear,
           Publisher,
           Isbn,
           PageCount,
           Genre,
           Notes,
           CAST(TableOfContents AS NVARCHAR(MAX)) AS TableOfContents,
           CreatedAt,
           UpdatedAt
    FROM dbo.Books
    WHERE Title LIKE @Pattern ESCAPE N'['
       OR Author LIKE @Pattern ESCAPE N'['
       OR CAST(TableOfContents AS NVARCHAR(MAX)) LIKE @Pattern ESCAPE N'['
    ORDER BY Title ASC, BookId ASC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

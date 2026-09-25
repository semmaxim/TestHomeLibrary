IF DB_ID(N'TestHomeLibrary') IS NULL
BEGIN
    CREATE DATABASE [TestHomeLibrary];
END
GO

USE [TestHomeLibrary];
GO

IF OBJECT_ID(N'dbo.Books', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Books
    (
        BookId           INT IDENTITY (1, 1) NOT NULL,
        Title            NVARCHAR(500)       NOT NULL,
        Author           NVARCHAR(300)       NOT NULL,
        PublicationYear  SMALLINT            NOT NULL,
        Publisher        NVARCHAR(300)       NULL,
        Isbn             NVARCHAR(20)        NULL,
        PageCount        INT                 NULL,
        Genre            NVARCHAR(100)       NULL,
        Notes            NVARCHAR(MAX)       NULL,
        TableOfContents  XML                 NULL,
        CreatedAt        DATETIME2(3)        NOT NULL CONSTRAINT DF_Books_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt        DATETIME2(3)        NULL,
        CONSTRAINT PK_Books PRIMARY KEY CLUSTERED (BookId),
        CONSTRAINT CK_Books_PublicationYear CHECK (PublicationYear BETWEEN 1450 AND 2200),
        CONSTRAINT CK_Books_PageCount CHECK (PageCount IS NULL OR PageCount > 0)
    );

    CREATE NONCLUSTERED INDEX IX_Books_Title ON dbo.Books (Title);
    CREATE NONCLUSTERED INDEX IX_Books_Author ON dbo.Books (Author);
    CREATE NONCLUSTERED INDEX IX_Books_PublicationYear ON dbo.Books (PublicationYear);
END
GO

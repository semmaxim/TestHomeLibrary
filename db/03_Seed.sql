USE [TestHomeLibrary];
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Books)
BEGIN
    INSERT INTO dbo.Books (Title, Author, PublicationYear, Publisher, Isbn, PageCount, Genre, Notes, TableOfContents)
    VALUES
    (N'Война и мир', N'Лев Толстой', 1869, N'Русский вестник', N'978-5-17-087662-4', 1225, N'Классика',
     N'Экземпляр из семейной библиотеки.',
     N'<toc><h2>Том I</h2><ol><li>Часть первая</li><li>Часть вторая</li></ol><h2>Том II</h2><ol><li>Часть первая</li></ol></toc>'),
    (N'Преступление и наказание', N'Фёдор Достоевский', 1866, N'Знание', N'978-5-04-089543-1', 608, N'Классика',
     NULL,
     N'<toc><ol><li>Часть I</li><li>Часть II</li><li>Часть III</li><li>Часть IV</li><li>Часть V</li><li>Часть VI</li><li>Эпилог</li></ol></toc>'),
    (N'Clean Architecture', N'Robert C. Martin', 2017, N'Prentice Hall', N'978-0-13-449416-6', 432, N'Программирование',
     N'Заметки на полях.',
     N'<toc><h2>Part I</h2><ol><li>All the Buzz Words</li><li>Programming Paradigms</li></ol><h2>Part II</h2><ol><li>Component Based Design</li></ol></toc>');
END
GO

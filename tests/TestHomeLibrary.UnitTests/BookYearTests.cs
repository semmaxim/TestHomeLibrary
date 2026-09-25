using TestHomeLibrary.Domain.Exceptions;
using TestHomeLibrary.Domain.ValueObjects;

namespace TestHomeLibrary.UnitTests;

public sealed class BookYearTests
{
    [Theory]
    [InlineData(1450)]
    [InlineData(1900)]
    [InlineData(2200)]
    public void From_ValidYear_ReturnsValue(int year)
    {
        var result = BookYear.From(year);

        Assert.Equal(year, result.Value);
    }

    [Theory]
    [InlineData(1449)]
    [InlineData(2201)]
    [InlineData(-1)]
    public void From_InvalidYear_ThrowsDomainValidationException(int year)
    {
        var ex = Assert.Throws<DomainValidationException>(() => BookYear.From(year));

        Assert.Contains("Год издания", ex.Message);
    }
}

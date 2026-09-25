using TestHomeLibrary.Domain.Exceptions;

namespace TestHomeLibrary.Domain.ValueObjects;

public readonly record struct BookYear
{
    public const int MinValue = 1450;
    public const int MaxValue = 2200;

    public int Value { get; }

    private BookYear(int value) => Value = value;

    public static BookYear From(int value)
    {
        if (value < MinValue || value > MaxValue)
        {
            throw new DomainValidationException(
                $"Год издания должен быть в диапазоне от {MinValue} до {MaxValue}.");
        }

        return new BookYear(value);
    }

    public override string ToString() => Value.ToString();
}

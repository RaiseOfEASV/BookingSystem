namespace BookingService.Domain.ValueObjects;

public sealed class BookingCode : IEquatable<BookingCode>
{
    public string Value { get; }

    private BookingCode(string value) => Value = value;

    public static BookingCode Generate()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return new BookingCode($"BK-{datePart}-{randomPart}");
    }

    public static BookingCode From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Booking code cannot be empty.", nameof(value));

        return new BookingCode(value);
    }

    public bool Equals(BookingCode? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is BookingCode other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static bool operator ==(BookingCode? left, BookingCode? right) => Equals(left, right);
    public static bool operator !=(BookingCode? left, BookingCode? right) => !Equals(left, right);
}
namespace InventoryService.Domain.ValueObjects;

public record SeatRow
{
    public string Value { get; }

    public SeatRow(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Seat row cannot be empty.", nameof(value));
        if (value.Length > 10)
            throw new ArgumentException("Seat row cannot exceed 10 characters.", nameof(value));

        Value = value.Trim().ToUpperInvariant();
    }

    public static implicit operator string(SeatRow row) => row.Value;
    public override string ToString() => Value;
}

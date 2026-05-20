namespace InventoryService.Domain.ValueObjects;

public record SeatSection
{
    public string Value { get; }

    public SeatSection(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Seat section cannot be empty.", nameof(value));
        if (value.Length > 100)
            throw new ArgumentException("Seat section cannot exceed 100 characters.", nameof(value));

        Value = value.Trim();
    }

    public static implicit operator string(SeatSection section) => section.Value;
    public override string ToString() => Value;
}

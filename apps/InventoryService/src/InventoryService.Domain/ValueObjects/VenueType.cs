namespace InventoryService.Domain.ValueObjects;

public record VenueType
{
    public string Value { get; }

    public VenueType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Venue type cannot be empty.", nameof(value));
        if (value.Length > 100)
            throw new ArgumentException("Venue type cannot exceed 100 characters.", nameof(value));

        Value = value.Trim();
    }

    public static implicit operator string(VenueType type) => type.Value;
    public override string ToString() => Value;
}

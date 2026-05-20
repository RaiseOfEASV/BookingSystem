namespace InventoryService.Domain.ValueObjects;

public record VenueName
{
    public string Value { get; }

    public VenueName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Venue name cannot be empty.", nameof(value));
        if (value.Length > 200)
            throw new ArgumentException("Venue name cannot exceed 200 characters.", nameof(value));

        Value = value.Trim();
    }

    public static implicit operator string(VenueName name) => name.Value;
    public override string ToString() => Value;
}

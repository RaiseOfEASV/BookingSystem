namespace InventoryService.Domain.ValueObjects;

public record EventName
{
    public string Value { get; }

    public EventName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Event name cannot be empty.", nameof(value));
        if (value.Length > 300)
            throw new ArgumentException("Event name cannot exceed 300 characters.", nameof(value));

        Value = value.Trim();
    }

    public static implicit operator string(EventName name) => name.Value;
    public override string ToString() => Value;
}

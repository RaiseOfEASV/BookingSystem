namespace InventoryService.Domain.ValueObjects;

public record Address
{
    public string Value { get; }

    public Address(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Address cannot be empty.", nameof(value));
        if (value.Length > 500)
            throw new ArgumentException("Address cannot exceed 500 characters.", nameof(value));

        Value = value.Trim();
    }

    public static implicit operator string(Address address) => address.Value;
    public override string ToString() => Value;
}

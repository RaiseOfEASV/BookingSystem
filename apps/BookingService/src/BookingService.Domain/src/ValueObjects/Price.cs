namespace BookingService.Domain.ValueObjects;

public record Price
{
    public decimal Amount   { get; init; }
    public string  Currency { get; init; }

    public Price(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        Amount   = amount;
        Currency = currency;
    }
}

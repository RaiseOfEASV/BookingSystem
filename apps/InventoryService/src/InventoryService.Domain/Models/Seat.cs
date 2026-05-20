using InventoryService.Domain.ValueObjects;

namespace InventoryService.Domain.Models;

public sealed class Seat
{
    public Guid        Id       { get; }
    public Guid        VenueId  { get; }
    public SeatRow     Row      { get; }
    public int         Number   { get; }
    public SeatSection Section  { get; }

    private Seat(Guid id, Guid venueId, SeatRow row, int number, SeatSection section)
    {
        Id      = id;
        VenueId = venueId;
        Row     = row;
        Number  = number;
        Section = section;
    }

    public static Seat Create(Guid venueId, string row, int number, string section)
    {
        if (venueId == Guid.Empty)
            throw new ArgumentException("VenueId cannot be empty.", nameof(venueId));
        if (number <= 0)
            throw new ArgumentException("Seat number must be greater than zero.", nameof(number));

        return new(Guid.NewGuid(), venueId, new SeatRow(row), number, new SeatSection(section));
    }
}

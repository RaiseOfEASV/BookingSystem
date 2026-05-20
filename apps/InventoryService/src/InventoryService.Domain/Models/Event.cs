using InventoryService.Domain.ValueObjects;

namespace InventoryService.Domain.Models;

public sealed class Event
{
    public Guid      Id        { get; }
    public Guid      VenueId   { get; }
    public EventName EventName { get; }
    public DateTime  StartDate { get; }
    public DateTime  EndDate   { get; }

    private Event(Guid id, Guid venueId, EventName eventName, DateTime startDate, DateTime endDate)
    {
        Id        = id;
        VenueId   = venueId;
        EventName = eventName;
        StartDate = startDate;
        EndDate   = endDate;
    }

    public static Event Create(Guid venueId, string eventName, DateTime startDate, DateTime endDate)
    {
        if (venueId == Guid.Empty)
            throw new ArgumentException("VenueId cannot be empty.", nameof(venueId));
        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date.", nameof(startDate));
        if (startDate < DateTime.UtcNow)
            throw new ArgumentException("Start date cannot be in the past.", nameof(startDate));

        return new(Guid.NewGuid(), venueId, new EventName(eventName), startDate, endDate);
    }
}

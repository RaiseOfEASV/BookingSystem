using InventoryService.Domain.ValueObjects;

namespace InventoryService.Domain.Models;

public sealed class Venue
{
    public Guid       Id      { get; }
    public VenueName  Name    { get; }
    public VenueType  Type    { get; }
    public Address    Address { get; }

    private Venue(Guid id, VenueName name, VenueType type, Address address)
    {
        Id      = id;
        Name    = name;
        Type    = type;
        Address = address;
    }

    public static Venue Create(string name, string type, string address)
        => new(Guid.NewGuid(), new VenueName(name), new VenueType(type), new Address(address));
}

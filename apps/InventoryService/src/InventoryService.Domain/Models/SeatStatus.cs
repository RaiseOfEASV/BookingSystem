using Exceptions;

public enum SeatStatus
{
    Available,
    Held,
    Reserved
}

public static class SeatStatusMapper
{
    private static readonly Dictionary<string, SeatStatus> _map = new()
    {
        { "available", SeatStatus.Available },
        { "held",      SeatStatus.Held      },
        { "reserved",  SeatStatus.Reserved  }
    };

    public static SeatStatus FromString(string status)
    {
        if (_map.TryGetValue(status.ToLower(), out var result))
            return result;

        throw new DomainException($"Unknown seat status: '{status}'");
    }

    public static string ToString(SeatStatus status) => status.ToString().ToLower();
}
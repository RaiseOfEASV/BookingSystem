namespace BookingService.Application.Exceptions;

public sealed class DuplicateBookingException : Exception
{
    public DuplicateBookingException()
        : base("A booking for the requested seat already exists.") { }
}
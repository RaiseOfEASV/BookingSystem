namespace BookingService.Application.Exceptions;

public sealed class BookingPersistenceException : Exception
{
    public BookingPersistenceException(string message, Exception? inner = null)
        : base(message, inner) { }
}
using BookingService.Application.Events.Definitions;
using OneOf;

public class BookingResult : OneOfBase<BookingCreatedEvent, BookingCreationFailedEvent>
{
    protected BookingResult(OneOf<BookingCreatedEvent, BookingCreationFailedEvent> input) : base(input) {}
    
    public bool IsSuccess => IsT0;
    public bool IsFailure => IsT1;

    public BookingCreatedEvent Success => AsT0;
    public BookingCreationFailedEvent Failure => AsT1;

    public static BookingResult FromSuccess(BookingCreatedEvent e) => new(e);
    public static BookingResult FromFailure(BookingCreationFailedEvent e) => new(e);

    public static implicit operator BookingResult(BookingCreatedEvent success) => new(success);
    public static implicit operator BookingResult(BookingCreationFailedEvent failure) => new(failure);
}
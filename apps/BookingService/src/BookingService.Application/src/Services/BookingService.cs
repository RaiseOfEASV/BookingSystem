using BookingService.Application.Interfaces;
using BookingService.Application.Models;
using BookingService.Domain.Models;
using BookingService.Domain.ValueObjects;

namespace BookingService.Application.Services;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;

    public BookingService(IBookingRepository repository)
    {
        _repository = repository;
    }

    public async Task<BookingDto> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        var booking = Booking.Create(
            customerId: request.CustomerId,
            eventId:    request.EventId,
            seatId:     request.SeatId,
            price:      new Price(request.Amount.Amount, request.Currency),
            code:       BookingCode.Generate(),
            notes:      request.Notes);

        var created = await _repository.CreateAsync(booking, cancellationToken);

        return ToDto(created);
    }

    public async Task<BookingDto> ConfirmAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var booking = await GetOrThrowAsync(bookingId, cancellationToken);
        booking.Confirm();
        var updated = await _repository.UpdateAsync(booking, cancellationToken);
        return ToDto(updated);
    }

    public async Task<BookingDto> CancelAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var booking = await GetOrThrowAsync(bookingId, cancellationToken);

        booking.Cancel();

        var updated = await _repository.UpdateAsync(booking, cancellationToken);

        return ToDto(updated);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private async Task<Booking> GetOrThrowAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var booking = await _repository.GetByIdAsync(bookingId, cancellationToken);

        if (booking is null)
            throw new KeyNotFoundException($"Booking {bookingId} not found.");

        return booking;
    }

    private static BookingDto ToDto(Booking b) => new(
        Id:         b.Id,
        CustomerId: b.CustomerId,
        EventId:    b.EventId,
        SeatId:     b.SeatId,
        Code:       b.Code.Value,
        Status:     b.Status.ToString(),
        Amount:     b.Price.Amount,
        Currency:   b.Price.Currency,
        Notes:      b.Notes,
        CreatedAt:  b.CreatedAt,
        UpdatedAt:  b.UpdatedAt);
}

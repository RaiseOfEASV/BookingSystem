using UserService.Domain.Entities;

namespace UserService.Application.Models;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string PhoneNumber,
    DateOnly? DateOfBirth,
    UserRole Role,
    string RoleName,
    UserStatus Status,
    string StatusName,
    AddressDto? Address,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record AddressDto(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country
);

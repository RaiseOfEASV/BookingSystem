using System.ComponentModel.DataAnnotations;
using UserService.Domain.Entities;

namespace UserService.Application.Models;

public record CreateUserRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [Required][EmailAddress] string Email,
    [Required][Phone] string PhoneNumber,
    DateOnly? DateOfBirth,
    UserRole Role = UserRole.Customer,
    CreateAddressRequest? Address = null
);

public record CreateAddressRequest(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country
);

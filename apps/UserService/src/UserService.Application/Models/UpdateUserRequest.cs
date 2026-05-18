using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Models;

public record UpdateUserRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [Required][Phone] string PhoneNumber,
    DateOnly? DateOfBirth,
    UpdateAddressRequest? Address
);

public record UpdateAddressRequest(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country
);

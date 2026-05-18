using UserService.Application.Interfaces;
using UserService.Application.Models;
using UserService.Domain.Entities;

namespace UserService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _repository.GetAllAsync(cancellationToken);
        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByEmailAsync(email, cancellationToken);
        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (await _repository.EmailExistsAsync(request.Email, cancellationToken))
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Role = request.Role,
            Status = UserStatus.Active,
            Address = request.Address is null ? null : new Address
            {
                Street = request.Address.Street,
                City = request.Address.City,
                State = request.Address.State,
                PostalCode = request.Address.PostalCode,
                Country = request.Address.Country
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(user, cancellationToken);
        return MapToDto(created);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user is null) return null;

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.DateOfBirth = request.DateOfBirth;
        user.Address = request.Address is null ? null : new Address
        {
            Street = request.Address.Street,
            City = request.Address.City,
            State = request.Address.State,
            PostalCode = request.Address.PostalCode,
            Country = request.Address.Country
        };
        user.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(user, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<UserDto?> UpdateStatusAsync(Guid id, UpdateUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user is null) return null;

        user.Status = request.Status;
        user.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(user, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user is null) return false;

        await _repository.DeleteAsync(id, cancellationToken);
        return true;
    }

    private static UserDto MapToDto(User u) => new(
        u.Id,
        u.FirstName,
        u.LastName,
        $"{u.FirstName} {u.LastName}",
        u.Email,
        u.PhoneNumber,
        u.DateOfBirth,
        u.Role,
        u.Role.ToString(),
        u.Status,
        u.Status.ToString(),
        u.Address is null ? null : new AddressDto(
            u.Address.Street,
            u.Address.City,
            u.Address.State,
            u.Address.PostalCode,
            u.Address.Country
        ),
        u.CreatedAt,
        u.UpdatedAt
    );
}

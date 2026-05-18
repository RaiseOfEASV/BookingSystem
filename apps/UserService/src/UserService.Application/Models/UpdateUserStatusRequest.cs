using System.ComponentModel.DataAnnotations;
using UserService.Domain.Entities;

namespace UserService.Application.Models;

public record UpdateUserStatusRequest(
    [Required] UserStatus Status
);

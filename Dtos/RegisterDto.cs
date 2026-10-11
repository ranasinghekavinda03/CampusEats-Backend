using System.ComponentModel.DataAnnotations;

namespace CampusEats.Api.Dtos;

public record RegisterDto(
    [Required] string Name,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);
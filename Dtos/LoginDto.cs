using System.ComponentModel.DataAnnotations;

namespace CampusEats.Api.Dtos;

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password
);
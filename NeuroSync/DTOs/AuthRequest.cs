using System.ComponentModel.DataAnnotations;

namespace NeuroSync.Api.DTOs;

public class UserRegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string FullName { get; set; }

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class UserLoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}
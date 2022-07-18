using System.ComponentModel.DataAnnotations;

namespace mark.webApi.AuthFolder.Models;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}

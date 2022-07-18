using System.ComponentModel.DataAnnotations;

namespace mark.webApi.AuthFolder.Models;

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; }
}

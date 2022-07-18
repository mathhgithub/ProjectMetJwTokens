using System.ComponentModel.DataAnnotations;

namespace mark.webApi.AuthFolder.Models;

public class LogoutRequest
{
    public string Token { get; set; }
}

using aMongoLibrary;
using System.Text.Json.Serialization;

namespace mark.webApi.AuthFolder.Models;

public class AuthResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }

}

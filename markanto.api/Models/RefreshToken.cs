namespace mark.webApi.AuthFolder.Models;

public class RefreshToken
{
    public string Token { get; set; }
    public string UserId { get; set; }
    public string CreatedByIp { get; set; }
    public DateTime Expires { get; set; }
    public DateTime? Revoked { get; set; }
    public string RevokedByIp { get; set; }
    public string ReasonRevoked { get; set; }

}

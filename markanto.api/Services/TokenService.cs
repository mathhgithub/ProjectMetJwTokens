using mark.webApi.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace mark.webApi.Services;

public class TokenService
{
    private readonly MongoRepo<UserRefreshToken> _mongoService;
    private readonly AppSettings _appSettings;
    public TokenService(IOptions<AppSettings> appSettings, MongoRepo<UserRefreshToken> mongoService)
    {
        _mongoService = mongoService;
        _appSettings = appSettings.Value;
    }

    public async Task<string> GenerateAccessToken(string accountID)
    {
        // generate token that is valid for 5 minutes
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", accountID) }),
            Expires = DateTime.Now.AddMinutes(5),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Audience = _appSettings.Audience,
            Issuer = _appSettings.Issuer
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return await Task.FromResult(tokenHandler.WriteToken(token));
    }

    public async Task<string> GenerateRefreshToken(string accountID, string ipAddress)
    {
        // generate refreshtoken that is valid for 5000 minutes
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.RefreshSecret));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.Now.AddMinutes(5000),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            Audience = _appSettings.Audience,
            Issuer = _appSettings.Issuer
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        string rawToken = await Task.FromResult(tokenHandler.WriteToken(token));

        var refreshToken = new UserRefreshToken
        {
            Token = rawToken,
            UserId = accountID,
            Expires = DateTime.Now.AddMinutes(5000),
            CreatedByIp = ipAddress
        };
        await _mongoService.InsertOneAsync(refreshToken);

        return rawToken;
    }

    public bool ValidateRefreshToken(string token)
    {
        TokenValidationParameters validationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.RefreshSecret)),
            ValidIssuer = _appSettings.Issuer,
            ValidAudience = _appSettings.Audience,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero
        };
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<string> GetRefreshTokenAndRevoke(string token, string reason, string ipAdres)
    {
        var _record = await _mongoService.FindOneAsync(x => x.Token == token);
        if (_record == null) return null;
        _record.RevokeDate = DateTime.Now;
        _record.RevokeIP = ipAdres;
        _record.RevokeReason = reason;
        await _mongoService.UpdateRecordAsync(_record.Id.ToString(), _record);
        return _record.UserId;
    }




}

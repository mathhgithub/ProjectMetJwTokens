using mark.webApi.AuthFolder.Models;
using mark.webApi.Models;
using mark.webApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace mark.webApi.Controllers;

[Route("api/account/")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;
    private readonly TokenService _tokenService;
    public AccountController(AccountService accountService, TokenService tokenService)
    { _accountService = accountService; _tokenService = tokenService; }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<string> Register([FromBody] RegisterRequest model)
    {
        string message = "";
        if (!ModelState.IsValid) { message = "hellow"; return message ; }
        bool isValid = await _accountService.Register(model, Request.Headers["origin"]);
        if (!isValid) { message = "grrrmp"; return message; }
        message = "mieauw"; return message;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        var account = await _accountService.Login(model);
        var accesToken = await _tokenService.GenerateAccessToken(account.Id.ToString());
        var refreshToken = await _tokenService.GenerateRefreshToken(account.Id.ToString(), ipAddress());
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.Now.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        return Ok(accesToken);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest(new { message = "Token is required" });

        string userID = await _tokenService.GetRefreshTokenAndRevoke(refreshToken, "logout", ipAddress());
        if (userID == null) { return BadRequest(); }

        return Ok(userID + "logged out");
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        if (!ModelState.IsValid) { return BadRequest("modelstate fout"); }
        bool isValidRefreshToken = _tokenService.ValidateRefreshToken(request.RefreshToken);
        if (!isValidRefreshToken) { return BadRequest("non valid token"); }

        //bewerk database token met reason "refreshed"
        string userID = await _tokenService.GetRefreshTokenAndRevoke(request.RefreshToken, "refresh", ipAddress());
        if (userID == null) { return BadRequest("bewerken database niet gelukt"); }

        //maak nieuw refresh en accesstoken
        var accesToken = await _tokenService.GenerateAccessToken(userID);
        var refreshToken = await _tokenService.GenerateRefreshToken(userID, ipAddress());
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.Now.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        return Ok(accesToken);
    }

    private string ipAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        else
            return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
    }
}
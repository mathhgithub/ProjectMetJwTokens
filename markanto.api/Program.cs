global using aMongoLibrary;
global using mark.webApi.Helpers;
global using mark.webApi.Services;
using markantoApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// configure strongly typed settings object
services.Configure<AppSettings>(builder.Configuration.GetSection("Jwt"));

// database config
services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoDB2"));
services.AddSingleton<MongoRepo<UserRefreshToken>>();
services.AddSingleton<MongoRepo<UserModel>>();
services.AddSingleton<MongoRepo<ProductDAL>>();

// configure DI for application services
services.AddScoped<TokenService>();
services.AddScoped<AccountService>();
services.AddScoped<ProductService>();

//moet in ieder geval in dev
services.Configure<KestrelServerOptions>(options =>
{
    options.ConfigureHttpsDefaults(options =>
        options.ClientCertificateMode = ClientCertificateMode.NoCertificate);
});


var securityScheme = new OpenApiSecurityScheme()
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JSON Web Token based security",
};

var securityReq = new OpenApiSecurityRequirement()
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }
};

services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(securityReq);
});

services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddCookie(config => {
    config.Cookie.Name = "refreshToken";
    config.Cookie.HttpOnly = true;//The cookie cannot be obtained by the front-end or the browser, and can only be modified on the server side
    //config.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;//This cookie cannot be used as a third-party cookie under any circumstances, without exception. For example, suppose b.com sets the following cookies:
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidateLifetime = false
    };
});

//services.AddSingleton(tokenValidation)

services.AddEndpointsApiExplorer();
services.AddControllers();
services.AddAuthorization();

var app = builder.Build();

// global cors policy
app.UseCors(x => x
    //  .SetIsOriginAllowed(origin => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowAnyOrigin()); 

app.UseSwagger();
app.UseSwaggerUI();
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// github.com/mohamadlawand087/MinimalApi-JWT/tree/main/TodoApi
// github.com/cornflourblue/dotnet-6-signup-verification-api
// github.com/SingletonSean/authentication-server/blob/master/AuthenticationServer.API/Services/TokenGenerators/RefreshTokenGenerator.cs
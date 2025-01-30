## install nugget package dependencies: 
Commands for packages
```
install-package bcrypt.net-core
install-package Microsoft.AspNetCore.Authentication.Jwtbearer
install-package Microsoft.AspNetCore.OpenApi
install-package Microsoft.EntityFrameworkCore
install-package Microsoft.EntityFrameworkCore.Design
install-package Microsoft.EntityFrameworkCore.InMemory
install-package Microsoft.EntityFrameworkCore.Tools
install-package NpgSql.EntityFrameworkCore.PostGreSql
install-package Swashbuckle.AspNetCore
install-package System.IdentityModel.Tokens.Jwt
```
Versions used for each package:
```
bcrypt.net-core -version 4.0.3 
Microsoft.AspNetCore.Authentication.Jwtbearer -version 7.0.13 
Microsoft.AspNetCore.OpenApi -version 7.0.13 
Microsoft.EntityFrameworkCore -version 7.0.13 
Microsoft.EntityFrameworkCore.Design -version 7.0.13 
Microsoft.EntityFrameworkCore.InMemory -version 7.0.13 
Microsoft.EntityFrameworkCore.Tools -version 7.0.13 
NpgSql.EntityFrameworkCore.PostGreSql -version 7.0.11 
Swashbuckle.AspNetCore -version 6.5.0 
System.IdentityModel.Tokens.Jwt -version 7.0.3
```

## Setup Auth in program.cs
- Add Authentication service with JWTBearer to the WebApplicationBuilder: 
- --
```csharp
var config = new ConfigurationSettings(); // See section `Create a Configuration class + Interface`  further down...

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetValue("AppSettings:Token"))),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = false

    };
});
```
  - Make sure there's a `appsettings.json` containing `AppSettings : {"Token": "secret phrase"}`
-  Add Authorization service to the WebApplicationBuilder:
-  --
```csharp
builder.Services.AddAuthorization();
```
- Add definitions for Cors, and define usages of HttpsRedirections, Authentication, Authorization: 
- -- 
```csharp
app.UseCors(x => x
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .SetIsOriginAllowed(origin => true) // allow any origin
                  .AllowCredentials()); // allow credentials

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

```

## Create a Configuration class + Interface
To easily get contents defined in our `appsettings.json` we may create a `ConfigurationSettings` class and Interface: 
```csharp 
// interface 
public interface IConfigurationSettings
{
    string GetValue(string key);
}

// class
public class ConfigurationSettings : IConfigurationSettings{
    IConfiguration _configuration;
    public ConfigurationSettings()
    {_configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();}
    
    public string GetValue(string key)
    {return _configuration.GetValue<string>(key)!;}
}
```
By adding this with the `builder.Servies.AddScoped<IConfigurationSettings, ConfigurationSettings>()` lets us easily inject it to our endpoints, example: 
```csharp
private static async Task<IResult> Login(UserRequestDto request, IRepository<User> service, IConfigurationSettings config)
{
    //user doesn't exist
    if (!service.GetAll().Where(u => u.Username == request.Username).Any()) return Results.BadRequest(new Payload<UserRequestDto>() { status = "User does not exist", data = request });

    User user = service.GetAll().FirstOrDefault(u => u.Username == request.Username)!;
   

    if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    {
        return Results.BadRequest(new Payload<UserRequestDto>() { status = "Wrong Password", data = request });
    }
    string token = CreateToken(user, config);
    return Results.Ok(new Payload<string>() { data =  token }) ;
}

private static string CreateToken(User user, IConfigurationSettings config)
{
    List<Claim> claims = new List<Claim>
    {
        new Claim(ClaimTypes.Sid, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email),
        
    };
    
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetValue("AppSettings:Token")));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
        );
    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
    return jwt;
}
```



## Enabling Authentication with Bearer scheme in Swagger
- When adding the SwaggerGen service during the builder setup, add `SercurityDefinition` and `SecurityRequirement` as arguments: 
- -- 
```csharp
builder.Services.AddSwaggerGen(s =>
{
    s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Add an Authorization header with a JWT token using the Bearer scheme see the app.http file for an example",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    s.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                    Array.Empty<string>()
                }
            });
}); 
```

## 

## Create/Register User 
- does user exist? 
- BCrypt to hash password 
- create User Instance; Insert into database; Save 

## Login 
- Does user exist? 
- Check password with BCrypt.Verify
- Create Token based on Fetched User instance (from db)
  - List of claims; id, name, mail
  - `Key` = new SymmetricSecurityKey UTF8 encoded bytes; config.GetValue(AppSettings:Token)
    - Token is stored in appsettings.json : "AppSettings": {"Token": "Some long phrase"}
  - `Credentials` = new SigningCredentials based on `Key`, select SecurityAlgorithms.HmacSha512Signature
  - `token` = new JWTSecurityToken: use list of claims, set token expiration DateTime, pass the `Credentials`
  - Create the `JWTToken` using a new JwtSecurityTokenHandler, with WriteToken(`token`)
  - return `JWTToken`
- return a payload with the `JWTToken` as data

## GetUsers and GetMessage 
- Endpoint function declared with `[Authorize]` attribute; Not allowed unless logged in
- **When doing the GET**: User must provide `authorization` to prove they're logged in (provide `authorization` header): 
  - --
  ```curl --request GET \
  --url https://localhost:7233/users \
  --header 'authorization: Bearer <TOKEN_FROM_LOGIN>'```
- Function declaration: async, that may take a `ClaimsPrincipal` as parameter (Even though not used)
- ClaimsPrincipal: 
  - May be provided, lets you access user information through the claim: 
  - --
  ```csharp
    ClaimsPrincipal user = ...; //Get the ClaimsPrincipal, probably as argument
    // get Real Id 
    Claim? claim = user.FindFirst(ClaimTypes.Sid);
    return int.Parse(claim?.Value);

    // get user id 
    IEnumerable<Claim> claims = user.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier);
    return claims.Count() >= 2 ? claims.ElementAt(1).Value : null;

    // get other member; email, Role...
    Claim? email_claim = user.FindFirst(ClaimTypes.Email);
    Claim? role_claim = user.FindFirst(ClaimTypes.Role);
    return email_claim?.Value;
    return role_claim?.Value;
  ```
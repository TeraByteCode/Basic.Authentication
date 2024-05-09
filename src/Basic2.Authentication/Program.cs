using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection();
var app = builder.Build();


app.MapGet("/login", (HttpContext ctx, IDataProtectionProvider dataProtection) =>
{
    var protector = dataProtection.CreateProtector("auth-cookie");
    var protectedPayload = protector.Protect("Username:Musteruser");
    ctx.Response.Headers.SetCookie = $"auth={protectedPayload}";

    return "Login successfull";
});

app.MapGet("/user", (HttpContext ctx, IDataProtectionProvider dataProtection) =>
{
    var protector = dataProtection.CreateProtector("auth-cookie");
    var authCookie = ctx.Request.Headers.Cookie.FirstOrDefault(x => x.StartsWith("auth="));
    if (authCookie == null)
    {
        ctx.Response.StatusCode = 401;
        return "Unauthorized";
    }

    var protectedPayload = authCookie.Split('=').Last();
    var payload = protector.Unprotect(protectedPayload);
    var parts = payload.Split(':');
    var key = parts[0];
    var value = parts[1];

    return $"User: {value}";
});

app.MapGet("/logout", (HttpContext ctx) => ctx.Response.Headers.SetCookie = "auth=; expires=Thu, 01 Jan 1970 00:00:00 GMT");

app.Run();
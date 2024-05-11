using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

app.Use((ctx, next) =>
{
    try
    {
        var dataProtection = ctx.RequestServices.GetRequiredService<IDataProtectionProvider>().CreateProtector("auth-cookie");
        var authCookie = ctx.Request.Headers.Cookie.FirstOrDefault(x => x.StartsWith("auth=")) ?? "=";
        var protectedPayload = authCookie.Split('=').Last();
        var payload = dataProtection.Unprotect(protectedPayload);
        var parts = payload.Split(':');
        var key = parts[0];
        var value = parts[1];

        var claims = new List<Claim>
        {
            new(key, value)
        };
        var identity = new ClaimsIdentity(claims);
        ctx.User = new ClaimsPrincipal(identity);
    }
    catch
    {
    }

    return next();
});

app.MapGet("/login", (HttpContext ctx, IDataProtectionProvider protectionProvider) =>
{
    var dataProtection = protectionProvider.CreateProtector("auth-cookie");
    var protectedPayload = dataProtection.Protect("Username:Musteruser");
    ctx.Response.Headers.SetCookie = $"auth={protectedPayload}";
    return "Login successfull";
});

app.MapGet("/user", (HttpContext ctx) =>
{
    var user = ctx.User.FindFirstValue("Username");
    return $"User: {user}";
});

app.MapGet("/logout", (HttpContext ctx) => ctx.Response.Headers.SetCookie = "auth=; expires=Thu, 01 Jan 1970 00:00:00 GMT");
app.Run();
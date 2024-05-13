using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication("auth-cookie") // Name der Authentifizierer (Authentifizierungsschema)
    .AddCookie("auth-cookie"); // Name des Cookie-Handlers, der für das Lesen und Schreiben des Cookies zuständig ist

var app = builder.Build();
app.UseAuthentication(); // Diese entspricht in etwa in unsere vorherige Beispiel dem app.Use((ctx, next) => { ... });

app.MapGet("/login", async (HttpContext ctx, IDataProtectionProvider protectionProvider) =>
{
    var claims = new List<Claim> { new("Username", "Musteruser") };
    var identity = new ClaimsIdentity(claims, "auth-cookie2");
    var principal = new ClaimsPrincipal(identity);
    await ctx.SignInAsync("auth-cookie", principal); // Hier wird das Cookie gesetzt

    return "Login successfull";
});

app.MapGet("/user", (HttpContext ctx) =>
{
    var user = ctx.User.FindFirstValue("Username");
    return $"User: {user}";
});

app.MapGet("/logout", async (HttpContext ctx) => await ctx.SignOutAsync("auth-cookie"));
app.Run();
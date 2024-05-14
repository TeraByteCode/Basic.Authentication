using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
const string AuthSchema = "authCookie";

builder.Services
    .AddAuthentication(AuthSchema) // Name der Authentifizierer (Authentifizierungsschema)
    .AddCookie(AuthSchema); // Name des Cookie-Handlers, der f�r das Lesen und Schreiben des Cookies zust�ndig ist

builder.Services.AddAuthorization(builder =>
{
    builder.AddPolicy("Adult", policy =>
    {
        policy
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(AuthSchema)
            .RequireClaim(ClaimTypes.DateOfBirth)
            .RequireAssertion(ctx =>
            {
                var birthDate = DateTime.Parse(ctx.User.FindFirstValue(ClaimTypes.DateOfBirth) ?? DateTime.Now.ToString());
                var age = DateTime.Now.Year - birthDate.Year;
                return age >= 18;
            });
    });
});

var app = builder.Build();
app.UseAuthentication(); // Diese entspricht in etwa in unsere vorherige Beispiel dem app.Use((ctx, next) => { ... });
app.UseAuthorization();

app.MapGet("/login", async (HttpContext ctx, IDataProtectionProvider protectionProvider) =>
{
    var claims = new List<Claim>
    {
        new("Username", "Musteruser"),
        new(ClaimTypes.DateOfBirth, new DateTime(2000, 01, 01).ToString("u"))
    };
    var identity = new ClaimsIdentity(claims, AuthSchema);
    var principal = new ClaimsPrincipal(identity);
    await ctx.SignInAsync(AuthSchema, principal); // Hier wird das Cookie gesetzt

    return "Login successfull";
});

//[Authorize("Adult")]
app.MapGet("/login2", async (HttpContext ctx, IDataProtectionProvider protectionProvider) =>
{
    var claims = new List<Claim>
    {
        new("Username", "Musteruser"),
        new(ClaimTypes.DateOfBirth, new DateTime(2020, 01, 01).ToString("u"))
    };
    var identity = new ClaimsIdentity(claims, AuthSchema);
    var principal = new ClaimsPrincipal(identity);
    await ctx.SignInAsync(AuthSchema, principal); // Hier wird das Cookie gesetzt

    return "Login successfull";
});

app.MapGet("adult", (HttpContext ctx) =>
{
    if (!ctx.User.Identities.Any(x => x.AuthenticationType == AuthSchema))
    {
        ctx.Response.StatusCode = 401;
        return "You are not authenticated";
    }
    var user = ctx.User.FindFirstValue(ClaimTypes.DateOfBirth);
    var birthDate = DateTime.Parse(user ?? DateTime.Now.ToString());
    var age = DateTime.Now.Year - birthDate.Year;

    if (age >= 18)
    {
        return "You are an adult";
    }

    ctx.Response.StatusCode = 401;
    return "";//You are not an adult

});

app.MapGet("adult2", (HttpContext ctx) => "You are an adult").RequireAuthorization("Adult");


app.MapGet("/user", (HttpContext ctx) =>
{
    var user = ctx.User.FindFirstValue("Username");
    return $"User: {user}";
});

app.MapGet("/logout", async (HttpContext ctx) => await ctx.SignOutAsync(AuthSchema));
app.Run();
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection();
builder.Services.AddScoped<AuthService>();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();


app.MapGet("/login", (AuthService service) =>
{
    service.SigIn();
    return "Login successfull";
});
app.MapGet("/user", (AuthService service) => $"User: {service.GetUser()}");
app.MapGet("/logout", (HttpContext ctx) => ctx.Response.Headers.SetCookie = "auth=; expires=Thu, 01 Jan 1970 00:00:00 GMT");

app.Run();

public class AuthService(IDataProtectionProvider dataProtection, IHttpContextAccessor accessor)
{
    private readonly IDataProtector _dataProtection = dataProtection.CreateProtector("auth-cookie");
    private readonly HttpContext _ctx = accessor.HttpContext ?? throw new Exception("Login nicht möglich...");

    public void SigIn()
    {
        var protectedPayload = _dataProtection.Protect("Username:Musteruser");
        _ctx.Response.Headers.SetCookie = $"auth={protectedPayload}";
    }

    public string GetUser()
    {
        try
        {
            var authCookie = _ctx.Request.Headers.Cookie.FirstOrDefault(x => x.StartsWith("auth="));
            var protectedPayload = authCookie.Split('=').Last();
            var payload = _dataProtection.Unprotect(protectedPayload);
            var parts = payload.Split(':');
            var key = parts[0];
            var value = parts[1];

            return value;

        }
        catch (Exception) { }

        _ctx.Response.StatusCode = 401;
        return "Unauthorized";
    }
}

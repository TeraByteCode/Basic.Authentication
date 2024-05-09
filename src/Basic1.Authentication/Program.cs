var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


app.MapGet("/login", (HttpContext ctx) =>
{
    ctx.Response.Headers.SetCookie = "auth=Username:Musteruser";
    return "Login successfull";
});

app.MapGet("/user", (HttpContext ctx) =>
{
    var authCookie = ctx.Request.Headers.Cookie.FirstOrDefault(x => x.StartsWith("auth="));
    if (authCookie == null)
    {
        ctx.Response.StatusCode = 401;
        return "Unauthorized";
    }

    var payload = authCookie.Split('=').Last();
    var parts = payload.Split(':');
    var key = parts[0];
    var value = parts[1];

    return $"User: {value}";
});

app.MapGet("/logout", (HttpContext ctx) =>
{
    ctx.Response.Headers.SetCookie = "auth=; expires=Thu, 01 Jan 1970 00:00:00 GMT";
});

app.Run();
## Basic3.Authentication 

Die Hauptunterschiede zwischen Basic2.Authentication\Program.cs und Basic3.Authentication\Program.cs liegen in der Struktur und Organisation des Codes.

In Basic2.Authentication\Program.cs wird die Authentifizierung direkt in den Routen-Handler-Funktionen implementiert. Das bedeutet, dass der Code, der für die Erstellung des geschützten Payloads, das Auslesen des Cookies und das Entschlüsseln des Payloads verantwortlich ist, direkt in den Routen-Handler-Funktionen steht.


In Basic3.Authentication\Program.cs wurde eine separate Klasse AuthService erstellt, die für die Authentifizierung verantwortlich ist. Diese Klasse kapselt die Logik für das Einloggen und das Abrufen des Benutzernamens. 

```csharp
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
```

Dadurch wird der Code in den Routen-Handler-Funktionen sauberer und einfacher zu lesen, da die Details der Authentifizierung in der AuthService-Klasse verborgen sind.

```csharp
app.MapGet("/login", (AuthService service) =>
{
    service.SigIn();
    return "Login successfull";
});
app.MapGet("/user", (AuthService service) => $"User: {service.GetUser()}");
app.MapGet("/logout", (HttpContext ctx) => ctx.Response.Headers.SetCookie = "auth=; expires=Thu, 01 Jan 1970 00:00:00 GMT");
```

Die Verbesserungen in der Basic3-Version sind also hauptsächlich struktureller Natur. Durch die Verwendung der AuthService-Klasse wird der Code besser organisiert, einfacher zu lesen und zu warten. Es ist auch einfacher, Tests für den Authentifizierungsdienst zu schreiben, da er nun in einer separaten Klasse gekapselt ist.
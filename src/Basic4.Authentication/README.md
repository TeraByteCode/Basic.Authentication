## basic4.Authentication 
In diesem Beispiel zeige ich, wie eine einfache Authentifizierung mit einer Middleware implementiert werden kann.
Das Beispiel soll zeigen, wie die Authentifizierung in ASP.NET Core in etwa funktioniert. 
In weiteren Beispielen werde ich darauf eingehen, wie die Authentifizierung in .NET besser und mit Standardwerkzeugen implementiert wird, so dass klarer wird, wie es intern funktioniert.

Um die Benutzerdaten zu speichern verwende ich die Klassen `ClaimsIdentity` und `ClaimsPrincipal`. Ähnlich wie es in ASP.NET gemacht wird.

```csharp
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
```

**Zur Info:** Die Speicherung der Benutzerangaben in einem ClaimsIdentity-Objekt ist ein Standard in der ASP.NET Core-Authentifizierung. 
Es basiert auf dem Konzept der Claims-basierten Authentifizierung, bei dem Benutzerinformationen als Ansprüche (Claims) repräsentiert werden.

Ein Anspruch (Claim) ist eine einzelne Information über den Benutzer, z. B. der Benutzername, die E-Mail-Adresse, die Rolle oder andere benutzerdefinierte Informationen. 
Durch die Verwendung von Ansprüchen können Sie die Benutzerinformationen in einer strukturierten und erweiterbaren Weise darstellen.

* Die **ClaimsIdentity-Klasse** repräsentiert die Identität eines Benutzers und enthält eine Sammlung von Ansprüchen. Sie können Ansprüche hinzufügen, entfernen oder abrufen, um auf die Benutzerinformationen zuzugreifen. 
* Die **ClaimsPrincipal-Klasse** wiederum repräsentiert den Benutzer selbst und enthält die ClaimsIdentity-Objekte.

Die Verwendung von ClaimsIdentity und ClaimsPrincipal bietet viele Vorteile, wie z. B. die Möglichkeit, Benutzerinformationen einfach zu überprüfen, zu autorisieren und zu übertragen. 
Darüber hinaus ermöglicht es eine flexible und erweiterbare Authentifizierungslösung, da Sie benutzerdefinierte Ansprüche hinzufügen können, um zusätzliche Informationen zu speichern.

Insgesamt ist die Verwendung von ClaimsIdentity und ClaimsPrincipal ein bewährtes Muster in der ASP.NET Core-Authentifizierung und bietet eine effektive Möglichkeit, Benutzerinformationen zu verwalten und zu verwenden.
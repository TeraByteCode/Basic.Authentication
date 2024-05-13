## Basic1.Authentication

Dieses Projekt zeigt eine einfache Implementierung einer Authentifizierungsfunktion in einer Webanwendung. 
Es verwendet das ASP.NET Minimal API, um einen Webserver zu erstellen und Endpunkte für verschiedene Aktionen bereitzustellen.

```csharp

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

```

Ich definiere drei Endpunkte: "/login", "/user" und "/logout", um es übersichtlicher und einfach zu machen.

### Der "/login"-Endpunkt 
Dieses Endpunkt wird verwendet, um einen Benutzer einzuloggen. <br>Wenn dieser Endpunkt aufgerufen wird, wird ein Cookie mit dem Namen "auth" und dem Wert "Username:Musteruser" in den Antwort-Headern gesetzt. Es wird auch die Meldung "Login erfolgreich" zurückgegeben.
```csharp
app.MapGet("/login", (HttpContext ctx) =>
{
    ctx.Response.Headers.SetCookie = "auth=Username:Musteruser";
    return "Login successfull";
});
```

Der **HttpContext** ist ein zentrales Objekt in ASP.NET Core, das Informationen über eine HTTP-Anfrage und -antwort enthält. 
Es stellt eine Schnittstelle bereit, um auf verschiedene Aspekte einer Webanwendung zuzugreifen, wie z.B. den Anfragepfad, die Anfrageparameter, die Anfrageheader, die Sitzungsdaten und vieles mehr.



### Der "/user"-Endpunkt 
Dieses Endpunkt wird verwendet, um Informationen über den angemeldeten Benutzer abzurufen. 
Zuerst wird überprüft, ob ein Cookie mit dem Namen "auth" in den Anfrage-Headern vorhanden ist. 
Wenn nicht, wird der Statuscode 401 (Unauthorized) zurückgegeben und die Meldung "Nicht autorisiert" angezeigt. 
Andernfalls wird der Wert des "auth"-Cookies analysiert, um den Benutzernamen zu extrahieren, und die Meldung "Benutzer: {Benutzername}" zurückgegeben.

```csharp
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
```


### Der "/logout"-Endpunkt 
Dieses Endpunkt wird verwendet, um den Benutzer abzumelden. 
Hier wird das "auth"-Cookie gelöscht, indem der Wert auf eine leere Zeichenkette gesetzt und das Ablaufdatum auf den 1. Januar 1970 gesetzt wird.

```csharp
app.MapGet("/logout", (HttpContext ctx) =>
{
    ctx.Response.Headers.SetCookie = "auth=; expires=Thu, 01 Jan 1970 00:00:00 GMT";
});
```

Schliesslich wird die app.Run()-Methode aufgerufen, um den Webserver zu starten und auf eingehende Anfragen zu warten.

Dieser Code zeigt also auf primitive Weise, wie die Authentifizierung in einer Webanwendung mit ASP.NET Core implementiert werden kann.

## Nachteile
Dieser Code hat jedoch einige Nachteile:
- Es speichert den Benutzernamen und das Passwort im Klartext im Cookie, was ein Sicherheitsrisiko darstellt.
- Es verwendet Cookies zur Speicherung von Authentifizierungsdaten, was anfällig für CSRF-Angriffe ist.
- Es verwendet keine Verschlüsselung oder Hashing für die Speicherung von Authentifizierungsdaten.
- Es verwendet keine Benutzerdatenbank oder Benutzerverwaltungsfunktionen.
- Es verwendet keine sichere Übertragung von Daten (HTTPS).
- Es verwendet keine sichere Authentifizierungsmethoden wie OAuth oder OpenID Connect.
- Es verwendet keine sicheren Passwortrichtlinien oder -verfahren.
- Es verwendet keine sicheren Sitzungsverwaltungsfunktionen.
- Es verwendet keine sicheren Authentifizierungsprotokolle wie JWT oder OAuth2.
- Es verwendet keine sicheren Authentifizierungsbibliotheken oder -frameworks.
- Es verwendet keine sicheren Authentifizierungsmethoden wie Zwei-Faktor-Authentifizierung oder Biometrie.
- Usw...

In den nächsten Projekten werde ich einige dieser Fehler beheben, je tiefer das Projekt geht, desto mehr Nachteile werde ich beheben.
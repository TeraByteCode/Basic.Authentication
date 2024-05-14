## Basic6.Authentication 
In diesem Projekt zeige ich, wie Policy funktionieren. Dazu zeige ich den Code Block für Block und gebe dazu eine kurze Erklärung. 

Im Projekt Basic6 konfigurieren wir grundsätzlich eine Autorisierungsrichtlinie mit dem Namen "Adult". 
Mit dieser Richtlinie stellen wir sicher, dass nur authentifizierte Benutzer, die mindestens 18 Jahre alt sind, auf bestimmte Ressourcen zugreifen können. Diese Vorgehensweise ist gut, da sie eine flexible und erweiterbare Möglichkeit bietet, benutzerdefinierte Autorisierungsrichtlinien zu definieren. 


Mit der ``AddPolicy-Methode`` erstellen wir eine benutzerdefinierte Policy (Autorisierungsrichtlinie). 
In unser Fall wird die Richtlinie "Adult" erstellt. Innerhalb der Richtlinie werden mehrere Anforderungen definiert, die erfüllt sein müssen, damit ein Benutzer als "erwachsen" betrachtet wird. Dadurch wird der Code übersichtlicher und leichter zu warten.

### Quellcode

Hier verwendet wir die eingebaute Authentifizierungsfunktionalität von ASP.NET Core. 
Wir nutzen AddAuthentication und AddCookie, um ein Authentifizierungsschema zu konfigurieren und Cookies zu verwalten.

```csharp
const string AuthSchema = "authCookie";
builder.Services
    .AddAuthentication(AuthSchema) // Name der Authentifizierer (Authentifizierungsschema)
    .AddCookie(AuthSchema); // Name des Cookie-Handlers, der für das Lesen und Schreiben des Cookies zuständig ist

```

 Hier erstellen wir die Adult Police:
  * Die ``RequireAuthenticatedUser-Methode`` stellt sicher, dass der Benutzer authentifiziert sein muss, um die Richtlinie zu erfüllen. 
  * Die ``AddAuthenticationSchemes-Methode`` gibt an, dass die Authentifizierung mit dem angegebenen Authentifizierungsschema erfolgen muss, in diesem Fall "authCookie". 
  * Die ``RequireClaim-Methode`` stellt sicher, dass der Benutzer den Anspruch "DateOfBirth" hat.
  * Die ``RequireAssertion-Methode`` ermöglicht es uns, eine benutzerdefinierte Überprüfung durchzuführen, um festzustellen, ob der Benutzer die Richtlinie erfüllt. <br> In diesem Fall wird überprüft, ob das Geburtsdatum des Benutzers vorhanden ist und ob das Alter des Benutzers mindestens 18 Jahre beträgt.

```csharp
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

```
* Die erste Zeile ``app.UseAuthentication();`` aktiviert die Authentifizierung für die Anwendung. Dies bedeutet, dass die Anwendung in der Lage sein wird, Benutzer zu authentifizieren.

* Die zweite Zeile ``app.UseAuthorization();`` aktiviert die Autorisierung für die Anwendung. Dies bedeutet, dass die Anwendung in der Lage sein wird, zu überprüfen, ob ein authentifizierter Benutzer berechtigt ist, auf bestimmte Ressourcen zuzugreifen.

```csharp
var app = builder.Build();
app.UseAuthentication(); 
app.UseAuthorization();
```

Anmeldung eines erwachsenen Benutzers

```csharp
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

```
Anmeldung eines minderjährigen Benutzers

```csharp

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
```

Dieses Beispiel zeigt, wie die Autorisierung (Genehmigung) ohne Police erfolgen könnte.

```csharp
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
```

Dieses Beispiel zeigt, wie die Autorisierung (Genehmigung) über Police erfolgt.
Warum dies übersichtlicher und einfacher zu handhaben ist, bedarf keiner grossen Erklärung.

```csharp
app.MapGet("adult2", (HttpContext ctx) => "You are an adult").RequireAuthorization("Adult");

```


**Achtung:** Es gibt andere Wege, das gleiche Ziel zu erreichen. Eine noch sauberer alternative besteht darin, eine benutzerdefinierte Klasse zu erstellen, die das ``IAuthorizationRequirement-Interface`` implementiert und die Logik für die Überprüfung der Autorisierung enthält. Diese Klasse kann dann in der ``AddPolicy-Methode`` verwendet werden, um die Richtlinie zu konfigurieren. Dies kann nützlich sein, wenn die Überprüfung komplexer wird und mehrere Schritte oder externe Ressourcen erfordert. In diesem Projekt sehen wir diese alternative nicht, aber vielleicht im nächsten.

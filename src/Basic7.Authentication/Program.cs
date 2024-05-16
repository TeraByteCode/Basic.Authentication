using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Fügt den Datenbankkontext hinzu. 
//Der IdentityDbContext ist die Standardimplementierung eines Datenbankkontexts, die von der Identitätsbibliothek bereitgestellt wird.
builder.Services.AddDbContext<IdentityDbContext>(options => options.UseInMemoryDatabase("myDB"));

//konfiguriert die Benutzer- und Passwortanforderungen.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(o =>
    {
        o.User.RequireUniqueEmail = false;
        o.Password.RequireDigit = false;
        o.Password.RequireLowercase = false;
        o.Password.RequireUppercase = false;
        o.Password.RequireNonAlphanumeric = false;
        o.Password.RequiredLength = 3;
    })
    .AddEntityFrameworkStores<IdentityDbContext>() // EF-Core-Datenbankkontext zur Speicherung von Identitätsdaten
    .AddDefaultTokenProviders(); // fügt Standard-Token-Anbieter hinzu.

var app = builder.Build();

//Initialisiert die Datenbank mit ein Standard Benutzer- und Rollendaten.
#region Add_User_And_Role
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (await roleManager.FindByNameAsync("Admin") == null)
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    if (await userManager.FindByNameAsync("admin") == null)
    {
        var user = new IdentityUser("admin");
        await userManager.CreateAsync(user, "admin");
        await userManager.AddToRoleAsync(user, "Admin");
    }
}
#endregion

//Hier sehen wir die Verwendung von der Rolle "Admin" als Autorisierungsanforderung.
app.MapGet("secret", [Authorize(Roles = "Admin")] (HttpContext ctx) =>
{
    return Results.Text("Hello Admin");
    //Alternativ zur Verwendung von Attributen kann die Berechtigung auch in der Methode selbst überprüft werden. (wird nicht empfohlen)
    //if (ctx.User.IsInRole("Admin")) return Results.Text("Hello Admin");
    //return Results.Text("Access denied");
});

//Es bedarf keines Kommentars, um zu verstehen, was hier vor sich geht, aber man kann sehen, wie einfach die Authentifizierung zu implementieren ist.
app.MapGet("Account/Login", async (HttpContext ctx, SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.PasswordSignInAsync("admin", "admin", true, false);
    return Results.Redirect("/");
});

app.MapGet("Account/Logout", async (HttpContext ctx, SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/");
});

//Hier wird der angemeldete Benutzername angezeigt, um anzuzeigen, dass die Authentifizierung erfolgreich war.
app.MapGet("/", (HttpContext ctx) =>
{
    ctx.Response.ContentType = "text/html";
    var user = ctx.User.Identity?.IsAuthenticated ?? false ? ctx.User.Identity.Name : "Anonym";
    var content = $"Hello {user}! <br> <a href='/Account/Login'>Login</a> | <a href='/Account/Logout'>Logout</a> | <a href='secret'>secret</a>";

    return Results.Text(content);
    //return $"<html><head></head><body>{content}</body></html>";
});

app.UseAuthorization();
app.Run();

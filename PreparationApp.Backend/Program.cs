// Point d'entrée de l'application. Config. des services, du middleware, et démarre le serveur.
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PreparationApp.Backend.ModelsBD;
using PreparationApp.Backend.Services;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;

// Point d'entrée principal de l'application
// Ce fichier configure les services, le middleware, et démarre le serveur web.

// --> Force l'environnement à "Development" si non défini (contourne le bug Visual Studio
// ??? il ne transmet pas ASPNETCORE_ENVIRONMENT depuis launchSettings.json).???
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
{
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
}

var builder = WebApplication.CreateBuilder(args);

// --> Force Kestrel à utiliser uniquement les ports définis dans launchSettings.json

builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:5001");


// --> Config. des services
// Active les contrôleurs API pour gérer les requêtes HTTP
builder.Services.AddControllers();

// Active la découverte des endpoints pour Swagger
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger pour la documentation de l'API
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PreparationApp API", Version = "v1" });
});

// --> Config. de l'authentification Google
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    var clientId = builder.Configuration["Google:ClientId"] ?? string.Empty;
    var clientSecret = builder.Configuration["Google:ClientSecret"] ?? string.Empty;
    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
    {
        throw new InvalidOperationException(
            "Les clefs 'Google:ClientId' et 'Google:ClientSecret' doivent être définies dans appsettings.json.");
    }
    options.ClientId = clientId;
    options.ClientSecret = clientSecret;
});

// --> Config. de la base de données avec Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            "La chaîne de connexion 'DefaultConnection' est manquante dans appsettings.json.");
    }
    options.UseSqlServer(connectionString);
});

// --> Ajout des services personnalisés
builder.Services.AddScoped<GoogleCalendarService>();
builder.Services.AddScoped<GoogleSheetsService>();

// --> Config. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// --> Ouverture automatique de Swagger dans le navigateur au démarrage
// Tente d'abord HTTPS (5001), puis HTTP (5000) si HTTPS échoue.
// Task.Delay(4000) attend que Kestrel soit prêt avant d'ouvrir le navigateur.
// ServerCertificateCustomValidationCallback : ignore la validation SSL pour le
// HttpClient uniquement (le navigateur, lui, fait confiance au certificat via --trust).
/*renouvellement du certificat le 14/06
dotnet dev-certs https --clean
dotnet dev-certs https --trust
*/



_ = Task.Run(async () =>
{
    await Task.Delay(4000);
    var urls = new[] { "https://localhost:5001/swagger", "http://localhost:5000/swagger" };
    foreach (var url in urls)
    {
        try
        {
            // Ignore la validation du certificat SSL pour les tests en développement
            var handler = new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            using var http = new System.Net.Http.HttpClient(handler);
            await http.GetAsync(url);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
            break; // Si HTTPS répond, on n'essaie pas HTTP
        }
        catch
        {
            continue; // Si HTTPS ne répond pas, on passe à HTTP
        }
    }
});

// --> Config. du middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PreparationApp API v1");
});

// Redirige HTTP vers HTTPS en production (désactivé en développement si pas de certificat SSL)
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontend");
app.MapControllers();
app.Run();
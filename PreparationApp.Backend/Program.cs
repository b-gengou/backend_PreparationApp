// Point d'entrée de l'application.
// Ce fichier configure les services, le middleware, et démarre le serveur web.
// Il inclut la configuration de JWT, CORS, Swagger, et l'authentification Google.

using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PreparationApp.Backend.ModelsBD;
using PreparationApp.Backend.Services;
using System.Text;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;

// --> Force l'environnement à "Development" (si non défini)
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
{
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
}

var builder = WebApplication.CreateBuilder(args);

// --> Config. des services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignore les références circulaires (ex. : Preparation -> Formateur -> Preparation)
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true; // Optionnel : pour un JSON plus lisible
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PreparationApp API", Version = "v1" });
});

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

builder.Services.AddScoped<GoogleCalendarService>();
builder.Services.AddScoped<GoogleSheetsService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("La clef JWT 'Jwt:Key' est manquante dans appsettings.json.");
}

// --> Configuration JWT (pour l'authentification des utilisateurs)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// --> Configuration Google (pour les services internes comme Calendar et Sheets)
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("1"));
    options.AddPolicy("FormateurOnly", policy => policy.RequireRole("1", "2"));
});

var app = builder.Build();

// --> Config. du pipeline HTTP
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontend");

// Config. Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PreparationApp API v1");
    c.RoutePrefix = "swagger";
});

// Redirection explicite pour /swagger
app.MapFallback("/swagger", context =>
{
    context.Response.Redirect("/swagger/index.html");
    return Task.CompletedTask;
});

// Middleware pour forcer la redirection de la racine vers Swagger
app.MapFallback(context =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger/index.html");
        return Task.CompletedTask;
    }
    return Task.CompletedTask;
});

app.MapControllers();

// Ouverture autom. de Swagger (désactivée)
/*
_ = Task.Run(async () =>
{
    await Task.Delay(4000);
    var urls = new[] { "https://localhost:5001/swagger/index.html", "http://localhost:5000/swagger/index.html" };
    foreach (var url in urls)
    {
        try
        {
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
            break;
        }
        catch
        {
            continue;
        }
    }
});
*/

// Ouverture automatique de Swagger au démarrage
_ = Task.Run(async () =>
{
    // Attendre 3 secondes que le serveur démarre
    await Task.Delay(3000);

    // URL à ouvrir (correspondant à votre configuration réelle)
    string swaggerUrl = "http://localhost:5000/swagger/index.html";

    try
    {
        // Vérifier que le serveur est accessible
        using var http = new System.Net.Http.HttpClient();
        var response = await http.GetAsync(swaggerUrl);

        if (response.IsSuccessStatusCode)
        {
            // Ouvrir le navigateur
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = swaggerUrl,
                UseShellExecute = true
            });
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Impossible d'ouvrir Swagger automatiquement : {ex.Message}");
    }
});

app.Run();
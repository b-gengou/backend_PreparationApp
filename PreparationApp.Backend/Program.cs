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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


// Configuration de Swagger.
// Définition du schéma de sécurité JWT (Bearer Token),
// pour que "Authorize" apparaisse en haut de la page Swagger
// et permette de tester les routes protégées par [Authorize]
// (ex: GET /api/preparations/sync, POST /api/preparations, etc.).
// !!!! avec Swashbuckle.AspNetCore version 10.x (utilisée dans ce
// projet, voir le .csproj), les classes ont changé de namespace par rapport
// aux anciens tutoriels qu'on trouve sur internet. Utilisation ici de 
// "Microsoft.OpenApi" (sans ".Models") et la classe "OpenApiSecuritySchemeReference",
// qui remplace l'ancienne combinaison "OpenApiSecurityScheme + OpenApiReference".

builder.Services.AddSwaggerGen(c =>
{
    // Informations générales affichées en haut de la page Swagger.
    c.SwaggerDoc("v1", new() { Title = "PreparationApp API", Version = "v1" });

    // Déclare à Swagger qu'il existe un schéma de sécurité nommé "Bearer",
    // basé sur un token JWT envoyé dans l'en-tête HTTP "Authorization".
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",                                 // Nom de l'en-tête HTTP utilisé pour le token.
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,        // Type "Http" pour un schéma Bearer standard.
        Scheme = "Bearer",                                        // Préfixe attendu devant le token (ex: "Bearer eyJ...").
        BearerFormat = "JWT",                                     // Précise le format du token : JWT.
        In = Microsoft.OpenApi.ParameterLocation.Header,         // Le token est transmis dans un en-tête HTTP, pas dans l'URL.
        Description = "Entrez votre token JWT (sans taper 'Bearer' devant, Swagger l'ajoute automatiquement)."
    });

    // Swagger applique ce schéma de sécurité "Bearer" à toutes
    // les routes marquées [Authorize] dans les contrôleurs, pour que le petit 
    // cadenas apparaisse sur ces routes spécifiques dans l'interface.
    c.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});


// Config. dela BD SQL Server via Entity Framework Core.
// La chaîne de connexion est lue depuis appsettings.json (ou
// appsettings.Development.json en environnement de développement).

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

// Enregistrement des services Google (Calendar et Sheets) pour qu'ils puissent
// être injectés automatiquement dans les contrôleurs qui en ont besoin.
builder.Services.AddScoped<GoogleCalendarService>();
builder.Services.AddScoped<GoogleSheetsService>();


// Configuration CORS : autorise le frontend React (lancé sur le port 5173
// par Vite) à appeler ce backend depuis le navigateur. Sans cela, le navigateur
// bloquerait les requêtes par sécurité (politique "same-origin").

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});


// Config. de l'authentification JWT (token applicatif généré par
// AuthController.cs lors du login/register).

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("La clef JWT 'Jwt:Key' est manquante dans appsettings.json.");
}

builder.Services.AddAuthentication(options =>
{
    // Définit JWT comme schéma d'authentification par défaut pour vérifier
    // les requêtes entrantes (lecture du token dans l'en-tête Authorization).
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Règles de validation appliquées à chaque token JWT reçu.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,             // Vérifie que le token a été émis par la bonne source.
        ValidateAudience = true,           // Vérifie que le token est destiné à cette application.
        ValidateLifetime = true,           // Vérifie que le token n'est pas expiré.
        ValidateIssuerSigningKey = true,   // Vérifie que la signature du token est valide.
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});


// Règles d'autorisation : définissent qui a le droit d'accéder à quoi,
// en se basant sur le rôle stocké dans le token JWT (claim "Role").
// "1" = administrateur, "2" = formateur simple.

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("1"));
    options.AddPolicy("FormateurOnly", policy => policy.RequireRole("1", "2"));
});


// Super imortant (voir ECARTS_CAHIER_DES_CHARGES.md) :
// Ce deuxième appel à AddAuthentication configure le schéma d'authentification
// Google OAuth, mais il n'est actuellement - pas - branché à un endpoint réel
// (aucun contrôleur ne déclenche ce flux Google pour l'instant).
// Le login fonctionnel actuel reste celui par email/mot de passe (AuthController).
// Ce bloc est conservé pour préparer la migration future vers OAuth 2.0 Google,
// exigée par le cahier des charges (section 4.2 et 10).

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

// Construction de l'application avec tous les services configurés ci-dessus.
var app = builder.Build();

// --> Config. du pipeline HTTP
// Ordre des lignes important : authentification avant autorisation,
// avant CORS, avant le mapping des contrôleurs.
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontend");


// Activation de Swagger : génère la documentation interactive de l'API,
// accessible à l'adresse /swagger.

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PreparationApp API v1");
    c.RoutePrefix = "swagger";
});

// Redirection explicite pour /swagger (sans slash final) vers la vraie page.
app.MapFallback("/swagger", context =>
{
    context.Response.Redirect("/swagger/index.html");
    return Task.CompletedTask;
});

// Middleware pour forcer la redirection de la racine "/" vers Swagger,
// pour que les visiteurs arrivent directement sur la documentation de l'API.
app.MapFallback(context =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger/index.html");
        return Task.CompletedTask;
    }
    return Task.CompletedTask;
});

// Active le routage vers les contrôleurs (Controllers/*.cs).
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
});*/

// Ouverture automatique de Swagger au démarrage : tente d'ouvrir le navigateur
// 3 secondes après le lancement du serveur, sur la page Swagger.
_ = Task.Run(async () =>
{
    // Attend 3 secondes que le serveur démarre
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
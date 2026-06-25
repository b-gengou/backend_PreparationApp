// Contrôleur pour gérer l'authentification des utilisateurs (formateurs).
// Permet la connexion, l'inscription, et la gestion des rôles (Admin = "1", Formateur = "2").
// ce contrôleur lit et écrit directement formateur.Role


using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PreparationApp.Backend.ModelsBD;
using Microsoft.EntityFrameworkCore;

namespace PreparationApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    // Constructeur avec injection de dépendances.
    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // Connexion d'un formateur.
    // POST: api/auth/login

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        try
        {
            // Cherche le formateur par courriel.
            var formateur = await _context.Formateurs
                .FirstOrDefaultAsync(f => f.Email == model.Email);

            // Vérifie si le formateur existe et si le mot de passe correspond.
            if (formateur == null || formateur.Password != model.Password)
            {
                return Unauthorized(new { message = "Email ou mot de passe incorrect." });
            }

            // Un compte désactivé (ex. : formateur ou admin qui a quitté
            // l'entreprise) ne peut plus se connecter. Ses données restent
            // toutefois intactes en base et visibles par les autres (voir
            // Formateur.IsActive et Formateur.DisplayName).
            if (!formateur.IsActive)
            {
                return Unauthorized(new { message = "Ce compte a été désactivé. Contactez un administrateur." });
            }

            // Génère le token JWT avec le rôle (Admin = "1", Formateur = "2").
            var token = GenerateJwtToken(formateur);

            return Ok(new
            {
                token,
                formateur.Id,
                formateur.Name,
                formateur.Email,
                // renvoie directement le rôle texte stocké en base ("1" ou "2"),
                // pour que le frontend sache tout de suite à qui il a affaire,
                // sans avoir besoin de décoder le token JWT lui-même.
                role = formateur.Role
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la connexion : {ex.Message}" });
        }
    }

    // Inscription d'un nouveau formateur.
    // POST: api/auth/register

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        try
        {
            // Vérifie si le courriel existe déjà.
            if (await _context.Formateurs.AnyAsync(f => f.Email == model.Email))
            {
                return BadRequest(new { message = "Un formateur avec cet email existe déjà." });
            }

            // Crée un nouveau formateur (par défaut, rôle = "2" = formateur simple).
            // Personne ne peut s'auto-inscrire en tant qu'admin via ce formulaire public :
            // promouvoir quelqu'un en admin se fait uniquement via une action manuelle
            // d'un admin existant (il faudra le prévoir plus tard dans FormateursController.
            // Pour le moment, je l'ai fait avec Swagger : Charles Martel --> charles@cognitic.be
            var formateur = new Formateur
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password, // À hasher en production (BCrypt ?)
                Role = "2", // Par défaut, formateur simple.
                GoogleCalendarId = string.Empty
            };

            _context.Formateurs.Add(formateur);
            await _context.SaveChangesAsync();

            // Génère le token JWT.
            var token = GenerateJwtToken(formateur);

            return Ok(new
            {
                token,
                formateur.Id,
                formateur.Name,
                formateur.Email,
                role = formateur.Role
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de l'inscription : {ex.Message}" });
        }
    }

    // Méthode privée pour générer un token JWT contenant le rôle du formateur.
    // Ici se joue le mécanisme clef : le rôle ("1" ou "2"), lu en
    // BD, est inscrit dans le token sous forme de "claim"
    // (ClaimTypes.Role). Ensuite, à chaque requête protégée, le backend
    // n'a plus besoin de retourner consulter la base : il lit directement
    // ce claim depuis le token reçu (cf.  [Authorize(Policy = "AdminOnly")]
    // dans les contrôleurs, ou User.FindFirst(ClaimTypes.Role) dans le code).

    private string GenerateJwtToken(Formateur formateur)
    {
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("La clé JWT est manquante dans appsettings.json.");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Définit les claims (infos encodées à l'intérieur du token).
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, formateur.Id.ToString()),
            new Claim(ClaimTypes.Name, formateur.Name),
            new Claim(ClaimTypes.Email, formateur.Email),
            // lit directement formateur.Role (déjà stocké en "1" ou "2" en base),
           
            new Claim(ClaimTypes.Role, formateur.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(12), // Token valide pour 12 heures.
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Modèles pour le login et le register (forme attendue du json envoyé via le frontend).
    public class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
// Contrôleur qui gère les requêtes liées aux comptes-rendus des préparations.
// Permet de créer, lire, mettre à jour et supprimer des comptes-rendus.
// règles (rôle "1" = admin, rôle "2" = formateur) :

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreparationApp.Backend.ModelsBD;
using System.Security.Claims;

namespace PreparationApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PreparationReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    // Constructeur avec injection de dépendances.
    public PreparationReportsController(AppDbContext context)
    {
        _context = context;
    }

    // Récupère tous les comptes-rendus.
    // GET: api/preparationreports
    [Authorize(Policy = "FormateurOnly")] // Tout utilisateur connecté (rôle "1" ou "2")
    [HttpGet]
    public async Task<IActionResult> GetPreparationReports()
    {
        try
        {
            var reports = await _context.Reports
                .Include(r => r.Preparation) // Inclut la préparation associée
                .ToListAsync();
            return Ok(reports);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la récupération des comptes-rendus : {ex.Message}" });
        }
    }

    // Récupère un compte-rendu spécifique par son ID.
    // GET: api/preparationreports/5
    [Authorize(Policy = "FormateurOnly")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPreparationReport(int id)
    {
        try
        {
            var report = await _context.Reports
                .Include(r => r.Preparation)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                return NotFound(new { Error = $"Aucun compte-rendu trouvé avec l'ID {id}." });
            }
            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur : {ex.Message}" });
        }
    }

    // Crée un nouveau compte-rendu.
    // POST: api/preparationreports
    // Règle : seul le formateur assigné à la préparation concernée, ou un
    // admin, peut créer un compte-rendu pour cette préparation. On ne peut
    // pas créer un compte-rendu "au nom" d'un autre formateur (sauf en admin).
   
    
    [Authorize(Policy = "FormateurOnly")]
    [HttpPost]
    public async Task<IActionResult> PostPreparationReport(PreparationReport report)
    {
        try
        {
            // Vérifie que la préparation existe.
            var preparation = await _context.Preparations.FindAsync(report.PreparationId);
            if (preparation == null)
            {
                return BadRequest(new { Error = $"Aucune préparation trouvée avec l'ID {report.PreparationId}." });
            }

            // Récupère l'identifiant et le rôle de l'utilisateur connecté
            // depuis son token JWT.
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { Error = "Utilisateur non connecté." });
            }
            var currentUserId = int.Parse(currentUserIdClaim);

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(currentUserRole))
            {
                return Unauthorized(new { Error = "Rôle utilisateur manquant." });
            }

            // Vérifie que l'utilisateur connecté est le formateur associé
            // à la préparation, ou un admin.
            if (preparation.FormateurId != currentUserId && currentUserRole != "1")
            {
                return Forbid(); // 403 Interdit
            }

            // Il faut remplir automatiquement le courriel du formateur connecté
            // et la date de création, plutôt que de dépendre du frontend pour
            // ces deux champs. C'est plus fiable et cela évite des erreurs de
            // validation si le frontend oublie de les envoyer
            // (le champ Email est [Required] dans PreparationReport.cs).

            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            report.Email = currentUserEmail ?? string.Empty;
            report.CreatedAt = DateTime.UtcNow;

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPreparationReport), new { id = report.Id }, report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la création : {ex.Message}" });
        }
    }

    // Met à jour un compte-rendu existant.
    // PUT: api/preparationreports/5
    // règle : le formateur assigné à la préparation concernée, ou un admin.


    [Authorize(Policy = "FormateurOnly")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPreparationReport(int id, PreparationReport report)
    {
        try
        {
            if (id != report.Id)
            {
                return BadRequest(new { Error = "L'ID du compte-rendu ne correspond pas." });
            }

            var existingReport = await _context.Reports
                .Include(r => r.Preparation)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingReport == null)
            {
                return NotFound(new { Error = $"Aucun compte-rendu trouvé avec l'ID {id}." });
            }

            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { Error = "Utilisateur non connecté." });
            }
            var currentUserId = int.Parse(currentUserIdClaim);

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(currentUserRole))
            {
                return Unauthorized(new { Error = "Rôle utilisateur manquant." });
            }

            // Vérifie que l'utilisateur connecté est le formateur associé
            // à la préparation, ou un admin.
            if (existingReport.Preparation.FormateurId != currentUserId && currentUserRole != "1")
            {
                return Forbid(); // 403 Interdit
            }

            // On conserve le courriel et la date de création d'origine, pour ne
            // pas les écraser par erreur lors d'une modification.
            report.Email = existingReport.Email;
            report.CreatedAt = existingReport.CreatedAt;

            _context.Entry(report).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PreparationReportExists(id))
            {
                return NotFound(new { Error = $"Le compte-rendu avec l'ID {id} n'existe plus." });
            }
            else
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la mise à jour : {ex.Message}" });
        }
    }

    // Supprime un compte-rendu.
    // Delete: api/preparationreports/5
    // Règle : le formateur assigné à la préparation concernée, ou un admin.
    
    [Authorize(Policy = "FormateurOnly")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePreparationReport(int id)
    {
        try
        {
            var report = await _context.Reports
                .Include(r => r.Preparation)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                return NotFound(new { Error = $"Aucun compte-rendu trouvé avec l'ID {id}." });
            }

            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { Error = "Utilisateur non connecté." });
            }
            var currentUserId = int.Parse(currentUserIdClaim);

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(currentUserRole))
            {
                return Unauthorized(new { Error = "Rôle utilisateur manquant." });
            }

            if (report.Preparation.FormateurId != currentUserId && currentUserRole != "1")
            {
                return Forbid(); // 403 Interdit
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la suppression : {ex.Message}" });
        }
    }

    // Vérifie si un compte-rendu existe.
    private bool PreparationReportExists(int id)
    {
        return _context.Reports.Any(e => e.Id == id);
    }
}
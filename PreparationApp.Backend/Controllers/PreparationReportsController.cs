// Contrôleur qui gère les requêtes liées aux comptes-rendus des préparations.
// Permet de créer, lire, mettre à jour et supprimer des comptes-rendus.
// règles (rôle "1" = admin, rôle "2" = formateur) :

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreparationApp.Backend.ModelsBD;
using PreparationApp.Backend.ModelsBD.Dtos;
using System.ComponentModel.DataAnnotations;
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
    public async Task<IActionResult> PostPreparationReport(PreparationReportDto dto)
    {
        try
        {
            // Validation manuelle des URLs : on n'applique [Url] que si le
            // champ n'est pas vide, car un lien de répertoire est optionnel
            // (contrairement à [Url] sur le modèle EF Core, qui rejetait
            // aussi une chaîne vide selon la version de .NET utilisée).
            var urlAttribute = new UrlAttribute();
            if (!string.IsNullOrEmpty(dto.DirectoryLink) && !urlAttribute.IsValid(dto.DirectoryLink))
            {
                ModelState.AddModelError(nameof(dto.DirectoryLink), "Le lien vers le répertoire doit être une URL valide.");
            }
            if (!string.IsNullOrEmpty(dto.WorkDirectoryLink) && !urlAttribute.IsValid(dto.WorkDirectoryLink))
            {
                ModelState.AddModelError(nameof(dto.WorkDirectoryLink), "Le lien vers le répertoire de travail doit être une URL valide.");
            }
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // Vérifie que la préparation existe.
            var preparation = await _context.Preparations.FindAsync(dto.PreparationId);
            if (preparation == null)
            {
                return BadRequest(new { Error = $"Aucune préparation trouvée avec l'ID {dto.PreparationId}." });
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

            // Mappe le DTO (champs saisis par le formateur) vers l'entité
            // EF Core, puis il faut compléter nous-mêmes les champs serveur
            // (Email et CreatedAt) qui ne doivent jamais venir du frontend.
            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            var report = new PreparationReport
            {
                SubjectsCovered = dto.SubjectsCovered,
                DailyObjectives = dto.DailyObjectives,
                ReferenceSupports = dto.ReferenceSupports,
                DirectoryLink = dto.DirectoryLink,
                ModifiedFiles = dto.ModifiedFiles,
                NewExercises = dto.NewExercises,
                WorkDirectoryLink = dto.WorkDirectoryLink,
                PlannedDate = dto.PlannedDate,
                CourseDurationDays = dto.CourseDurationDays,
                TechnicalIssues = dto.TechnicalIssues,
                SecondOpinionNeed = dto.SecondOpinionNeed,
                SecondOpinionAction = dto.SecondOpinionAction,
                TimeSpent = dto.TimeSpent,
                PreparationId = dto.PreparationId,
                Email = currentUserEmail ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
            };

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
    public async Task<IActionResult> PutPreparationReport(int id, PreparationReportDto dto)
    {
        try
        {
            // Même validation manuelle des URLs que pour le POST (cf. 
            // commentaire détaillé dans PostPreparationReport).
            var urlAttribute = new UrlAttribute();
            if (!string.IsNullOrEmpty(dto.DirectoryLink) && !urlAttribute.IsValid(dto.DirectoryLink))
            {
                ModelState.AddModelError(nameof(dto.DirectoryLink), "Le lien vers le répertoire doit être une URL valide.");
            }
            if (!string.IsNullOrEmpty(dto.WorkDirectoryLink) && !urlAttribute.IsValid(dto.WorkDirectoryLink))
            {
                ModelState.AddModelError(nameof(dto.WorkDirectoryLink), "Le lien vers le répertoire de travail doit être une URL valide.");
            }
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var existingReport = await _context.Reports
                .Include(r => r.Preparation)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingReport == null)
            {
                return NotFound(new { Error = $"Aucun compte-rendu trouvé avec l'ID {id}." });
            }

            // Vérifie que le compte-rendu modifié reste bien rattaché à
            // la même préparation que celle envoyée dans le DTO.
            if (dto.PreparationId != existingReport.PreparationId)
            {
                return BadRequest(new { Error = "Impossible de rattacher ce compte-rendu à une autre préparation." });
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

            // Met à jour uniquement les champs modifiables par le
            // formateur. Email, CreatedAt, Id et PreparationId restent
            // ceux de l'entité existante en base, jamais ceux du DTO.
            existingReport.SubjectsCovered = dto.SubjectsCovered;
            existingReport.DailyObjectives = dto.DailyObjectives;
            existingReport.ReferenceSupports = dto.ReferenceSupports;
            existingReport.DirectoryLink = dto.DirectoryLink;
            existingReport.ModifiedFiles = dto.ModifiedFiles;
            existingReport.NewExercises = dto.NewExercises;
            existingReport.WorkDirectoryLink = dto.WorkDirectoryLink;
            existingReport.PlannedDate = dto.PlannedDate;
            existingReport.CourseDurationDays = dto.CourseDurationDays;
            existingReport.TechnicalIssues = dto.TechnicalIssues;
            existingReport.SecondOpinionNeed = dto.SecondOpinionNeed;
            existingReport.SecondOpinionAction = dto.SecondOpinionAction;
            existingReport.TimeSpent = dto.TimeSpent;

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
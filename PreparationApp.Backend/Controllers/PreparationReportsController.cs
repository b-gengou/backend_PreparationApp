// Contrôleur pour gérer les requêtes liées aux comptes-rendus.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreparationApp.Backend.ModelsBD; 
using PreparationApp.Backend.Services;



namespace PreparationApp.Backend.Controllers;

// Contrôleur pour gérer les requêtes HTTP liées aux comptes-rendus de préparation.
[ApiController]
[Route("api/[controller]")]
public class PreparationReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    // Constructeur du contrôleur.
    // L'injection de dépendance fournit automatiquement le contexte de la base de données.
    public PreparationReportsController(AppDbContext context)
    {
        _context = context;
    }

    // Crée un nouveau compte-rendu de préparation.
    // POST: api/preparationreports
    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] PreparationReport report)
    {
        try
        {
            if (report == null)
            {
                return BadRequest(new { Error = "Le compte-rendu ne peut pas être null." });
            }

            _context.PreparationReports.Add(report);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la sauvegarde en base de données : {ex.Message}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Une erreur est survenue : {ex.Message}" });
        }
    }

    // Récupère un compte-rendu spécifique par son identifiant.
    // GET: api/preparationreports/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReport(int id)
    {
        try
        {
            var report = await _context.PreparationReports
                .Include(pr => pr.Preparation)
                .FirstOrDefaultAsync(pr => pr.Id == id);

            if (report == null)
            {
                return NotFound(new { Error = $"Aucun compte-rendu trouvé avec l'identifiant {id}." });
            }
            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Une erreur est survenue : {ex.Message}" });
        }
    }

    // Récupère la liste de tous les comptes-rendus.
    // GET: api/preparationreports
    [HttpGet]
    public async Task<IActionResult> GetAllReports()
    {
        try
        {
            var reports = await _context.PreparationReports
                .Include(pr => pr.Preparation)
                .ToListAsync();
            return Ok(reports);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Une erreur est survenue : {ex.Message}" });
        }
    }

    // Récupère les comptes-rendus associés à une préparation spécifique.
    // GET: api/preparationreports/by-preparation/5
    [HttpGet("by-preparation/{preparationId}")]
    public async Task<IActionResult> GetReportsByPreparation(int preparationId)
    {
        try
        {
            var reports = await _context.PreparationReports
                .Where(pr => pr.PreparationId == preparationId)
                .ToListAsync();
            return Ok(reports);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Une erreur est survenue : {ex.Message}" });
        }
    }
}
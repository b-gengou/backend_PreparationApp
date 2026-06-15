//Contrôleur pour gérer les requêtes liées aux préparations.
// Gestion des préparations (récupération depuis Google Calendar, liste des préparations).

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreparationApp.Backend.ModelsBD;
using PreparationApp.Backend.Services;

namespace PreparationApp.Backend.Controllers;

// Contrôleur pour gérer les requêtes HTTP liées aux préparations.
[ApiController]
[Route("api/[controller]")]
public class PreparationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly GoogleCalendarService _calendarService;

    // Constructeur du contrôleur.
    // L'injection de dépendance fournit le contexte de la BD et le service Google Calendar.
    public PreparationsController(AppDbContext context, GoogleCalendarService calendarService)
    {
        _context = context;
        _calendarService = calendarService;
    }

    // Synchronise les préparations avec Google Calendar.
    // GET: api/preparations/sync
    [HttpGet("sync")]
    public async Task<IActionResult> SyncPreparationsFromCalendar()
    {
        try
        {
            var calendarEvents = await _calendarService.GetPreparationEventsAsync();
            foreach (var calendarEvent in calendarEvents)
            {
                // Vérifie si la préparation existe déjà en BD
                var existingPreparation = await _context.Preparations
                    .FirstOrDefaultAsync(p => p.GoogleEventId == calendarEvent.Id);

                if (existingPreparation == null)
                {
                    // Si elle n'existe pas, on la crée
                    var preparation = new Preparation
                    {
                        GoogleEventId = calendarEvent.Id,
                        Subject = calendarEvent.Summary,
                        StartDate = calendarEvent.Start,
                        EndDate = calendarEvent.End,
                        Status = "À venir"
                    };
                    _context.Preparations.Add(preparation);
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Préparations synchronisées avec Google Calendar." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la synchronisation : {ex.Message}" });
        }
    }

    // Récupère la liste de toutes les préparations.
    // GET: api/preparations
    [HttpGet]
    public async Task<IActionResult> GetPreparations()
    {
        try
        {
            // Include charge aussi le formateur lié à chaque préparation
            var preparations = await _context.Preparations
                .Include(p => p.Formateur)
                .ToListAsync();
            return Ok(preparations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Une erreur est survenue : {ex.Message}" });
        }
    }

    // Récupère une préparation spécifique par son identifiant.
    // GET: api/preparations/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPreparation(int id)
    {
        try
        {
            var preparation = await _context.Preparations
                .Include(p => p.Formateur)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (preparation == null)
            {
                return NotFound(new { Error = $"Aucune préparation trouvée avec l'identifiant {id}." });
            }
            return Ok(preparation);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Une erreur est survenue : {ex.Message}" });
        }
    }
}
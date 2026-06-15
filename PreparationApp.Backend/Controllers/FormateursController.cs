//Contrôleur pour gérer les requêtes liées aux formateurs.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreparationApp.Backend.ModelsBD;

namespace PreparationApp.Backend.Controllers;

// Contrôleur pour gérer les requêtes HTTP liées aux formateurs.
[ApiController]
[Route("api/[controller]")]
public class FormateursController : ControllerBase
{
    private readonly AppDbContext _context;

    // Constructeur du contrôleur.
    // L'injection de dépendance fournit automatiquement le contexte de la base de données.
    public FormateursController(AppDbContext context)
    {
        _context = context;
    }

    // Récupère la liste de tous les formateurs.
    // GET: api/formateurs
    [HttpGet]
    public async Task<IActionResult> GetFormateurs()
    {
        try
        {
            var formateurs = await _context.Formateurs.ToListAsync();
            return Ok(formateurs);
        }
        catch (Exception ex)
        {
            // Log l'erreur 
            return StatusCode(500, new { Error = $"Une erreur est survenue : {ex.Message}" });
        }
    }

    // Récupère un formateur spécifique par son identifiant.
    // GET: api/formateurs/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFormateur(int id)
    {
        try
        {
            var formateur = await _context.Formateurs.FindAsync(id);
            if (formateur == null)
            {
                return NotFound(new { Error = $"Aucun formateur trouvé avec l'identifiant {id}." });
            }
            return Ok(formateur);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Une erreur est survenue : {ex.Message}" });
        }
    }

    // Crée un nouveau formateur.
    // POST: api/formateurs
    [HttpPost]
    public async Task<IActionResult> CreateFormateur([FromBody] Formateur formateur)
    {
        try
        {
            if (formateur == null)
            {
                return BadRequest(new { Error = "Le formateur ne peut pas être null." });
            }

            _context.Formateurs.Add(formateur);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetFormateur), new { id = formateur.Id }, formateur);
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
}
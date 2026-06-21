// Ce contrôleur gère le "catalogue des supports" exigé par le cahier des charges
// (section 3.1 - MVP : "Catalogue des supports" ; US015 et US016).
//
// Règles --> rôle "1" = admin, rôle "2" = formateur
// a) Consulter/rechercher (GET) : tout utilisateur connecté.
// b) Créer (POST) : tout utilisateur connecté (formateur ou admin).
// c) Modifier/Supprimer : uniquement le créateur de la ressource OU un admin.

using Microsoft.AspNetCore.Authorization; // Fournit [Authorize] pour protéger les routes.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreparationApp.Backend.ModelsBD;
using System.Security.Claims;             // Fournit ClaimTypes pour lire les infos du token JWT.

namespace PreparationApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResourcesController(AppDbContext context)
    {
        _context = context;
    }


    // GET: api/resources
    // GET: api/resources?search=sql&type=PDF&formateurId=1&date=2026-06-15
    // Récupère la liste des ressources, avec recherche par mot-clé et filtres
    // optionnels par type, formateur et date (US015 et US016).
    // Règle : tout utilisateur connecté peut consulter toutes les ressources,
    // peu importe qui les a créées --> catalogue partagé entre tous les formateurs.

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetResources(
        [FromQuery] string? search,
        [FromQuery] int? formateurId,
        [FromQuery] DateTime? date,
        [FromQuery] string? type)
    {
        try
        {
            var query = _context.Resources
                .Include(r => r.CreatedBy)
                .Include(r => r.PreparationResources)
                    .ThenInclude(pr => pr.Preparation)
                        .ThenInclude(p => p.Formateur)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r => r.Name.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(r => r.Type == type);
            }

            if (formateurId.HasValue)
            {
                query = query.Where(r => r.PreparationResources
                    .Any(pr => pr.Preparation.FormateurId == formateurId.Value));
            }

            if (date.HasValue)
            {
                query = query.Where(r => r.PreparationResources
                    .Any(pr => pr.Preparation.StartDate.Date == date.Value.Date));
            }

            var resources = await query.ToListAsync();
            return Ok(resources);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la recherche des ressources : {ex.Message}" });
        }
    }

    // GET: api/resources/5
    // Récupère une ressource précise par son identifiant.
    // Règle : tout utilisateur connecté peut consulter.

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetResource(int id)
    {
        try
        {
            var resource = await _context.Resources
                .Include(r => r.CreatedBy)
                .Include(r => r.PreparationResources)
                    .ThenInclude(pr => pr.Preparation)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resource == null)
            {
                return NotFound(new { Error = $"Aucune ressource trouvée avec l'ID {id}." });
            }

            return Ok(resource);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Une erreur est survenue : {ex.Message}" });
        }
    }

    // POST: api/resources
    // Crée une nouvelle ressource (ex.: un nouveau PDF ou lien vidéo ajouté au catalogue).
    // Règle : tout utilisateur connecté (formateur ou admin) peut créer une ressource.
    // Le créateur est automatiquement l'utilisateur actuellement connecté.

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateResource([FromBody] Resource resource)
    {
        try
        {
            // On récupère l'id. de l'utilisateur connecté depuis son token JWT.
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { Error = "Utilisateur non connecté." });
            }
            var currentUserId = int.Parse(currentUserIdClaim);

            // Force le créateur à être l'utilisateur connecté, pour éviter
            // qu'un utilisateur qui a été bercé trop près du mur attribue la ressource à quelqu'un d'autre.

            resource.CreatedById = currentUserId;
            resource.CreatedAt = DateTime.UtcNow;

            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResource), new { id = resource.Id }, resource);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la création de la ressource : {ex.Message}" });
        }
    }
    // PUT: api/resources/5
    // Met à jour une ressource existante.
    // Règle : seul le créateur de la ressource OU un admin (rôle "1") peut modifier.

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateResource(int id, [FromBody] Resource resource)
    {
        try
        {
            if (id != resource.Id)
            {
                return BadRequest(new { Error = "L'ID de la ressource ne correspond pas." });
            }

            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { Error = "Utilisateur non connecté." });
            }
            var currentUserId = int.Parse(currentUserIdClaim);

            // Le rôle est stocké dans le token : "1" = admin, "2" = formateur simple.
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(currentUserRole))
            {
                return Unauthorized(new { Error = "Rôle utilisateur manquant." });
            }

            var existingResource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == id);
            if (existingResource == null)
            {
                return NotFound(new { Error = $"Aucune ressource trouvée avec l'ID {id}." });
            }

            // Vérif. des droits : seul le créateur ou un admin peut modifier.
            if (existingResource.CreatedById != currentUserId && currentUserRole != "1")
            {
                return Forbid(); // 403 Interdit
            }

            // Met à jour les champs modifiables : ne pas toucher pas à CreatedById/CreatedAt,
            // qui doivent rester ceux d'origine
            
            existingResource.Name = resource.Name;
            existingResource.Url = resource.Url;
            existingResource.Type = resource.Type;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Resources.AnyAsync(r => r.Id == id))
            {
                return NotFound(new { Error = $"La ressource avec l'ID {id} n'existe plus." });
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la mise à jour : {ex.Message}" });
        }
    }

    // DELETE: api/resources/5
    // Supprime une ressource existante.
    // Règle : seul le créateur de la ressource ou un admin (rôle "1") peut supprimer.


    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResource(int id)
    {
        try
        {
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

            var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == id);
            if (resource == null)
            {
                return NotFound(new { Error = $"Aucune ressource trouvée avec l'ID {id}." });
            }

            // Vérif. des droits : seul le créateur ou un admin peut supprimer.
            if (resource.CreatedById != currentUserId && currentUserRole != "1")
            {
                return Forbid();
            }

            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la suppression : {ex.Message}" });
        }
    }


    // POST: api/resources/5/link/3
    // Lie une ressource existante (id=5) à une préparation existante (id=3).
    // Règle : tout utilisateur connecté peut lier une ressource à une préparation.

    [Authorize]
    [HttpPost("{resourceId}/link/{preparationId}")]
    public async Task<IActionResult> LinkToPreparation(int resourceId, int preparationId)
    {
        try
        {
            var resourceExists = await _context.Resources.AnyAsync(r => r.Id == resourceId);
            var preparationExists = await _context.Preparations.AnyAsync(p => p.Id == preparationId);

            if (!resourceExists || !preparationExists)
            {
                return NotFound(new { Error = "Ressource ou préparation introuvable." });
            }

            var alreadyLinked = await _context.PreparationResources
                .AnyAsync(pr => pr.ResourceId == resourceId && pr.PreparationId == preparationId);

            if (alreadyLinked)
            {
                return BadRequest(new { Error = "Cette ressource est déjà liée à cette préparation." });
            }

            _context.PreparationResources.Add(new PreparationResource
            {
                ResourceId = resourceId,
                PreparationId = preparationId
            });
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Ressource liée à la préparation avec succès." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la liaison : {ex.Message}" });
        }
    }
}
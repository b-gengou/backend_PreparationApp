// Ce contrôleur gère les requêtes liées aux préparations de formation.
// a) La synchronisation avec Google Calendar (récupérer les événements
// "préparation" et les enregistrer en base) - cf. cahier des charges
//  section 3.1 (MVP) et US/objectif 05.
// b) CRUD des préparations (lister, lire, créer, modifier, supprimer),
//  avec vérif. des droits d'accès (créateur ou admin uniquement).


using Microsoft.AspNetCore.Authorization; // [Authorize] pour protéger les routes.
using Microsoft.AspNetCore.Mvc;           // [ApiController], [HttpGet], [HttpPost], etc.
using Microsoft.EntityFrameworkCore;      // Include, FirstOrDefaultAsync, ToListAsync...
using PreparationApp.Backend.ModelsBD;    // Modèles Preparation, AppDbContext.
using PreparationApp.Backend.ModelsBD.Dtos;
using PreparationApp.Backend.Services;    // Service GoogleCalendarService.
using System.Security.Claims;             // ClaimTypes pour lire les infos du token JWT.

namespace PreparationApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PreparationsController : ControllerBase
{
    // Contexte de la BD.
    private readonly AppDbContext _context;

    // Service qui s'adresse à l'API Google Calendar (cf. Services/GoogleCalendarService.cs).
    private readonly GoogleCalendarService _calendarService;

    // Constructeur : ASP.NET Core injecte automatiquement le contexte BD
    // et le service Calendar (configurés dans Program.cs).
    public PreparationsController(AppDbContext context, GoogleCalendarService calendarService)
    {
        _context = context;
        _calendarService = calendarService;
    }

    // GET: api/preparations/sync
    // Synchronise les préparations avec Google Calendar : récupère les
    // événements contenant le mot "préparation" et les enregistre en base
    // s'ils n'existent pas encore (identifiés par leur GoogleEventId).
    // on utilise l'identifiant de l'utilisateur réellement connecté,
    // récupéré depuis son token JWT.
    // [Authorize] est donc ajouté : il faut être connecté pour synchroniser.

    [Authorize]
    [HttpGet("sync")]
    public async Task<IActionResult> SyncPreparationsFromCalendar()
    {
        try
        {
            // On récupère l'id de l'utilisateur connecté à partir du token JWT.
            // ClaimTypes.NameIdentifier correspond au claim défini dans AuthController.cs
            // lors de la génération du token (GenerateJwtToken).
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Si ce claim est absent, c'est que l'on n'est pas authentifié.
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { Error = "Utilisateur non connecté." });
            }

            // Conversion de la chaîne de caractères du claim en Int.
            var currentUserId = int.Parse(currentUserIdClaim);

            // Appel au service qui contacte réellement l'API Google Calendar
            // et renvoie la liste des événements "préparation" trouvés.
            var calendarEvents = await _calendarService.GetPreparationEventsAsync();

            // Parcourt chaque événement récupéré depuis Google Calendar.
            foreach (var calendarEvent in calendarEvents)
            {
                // Vérif. si une préparation existe déjà en base pour cet événement
                // utilisation de GoogleEventId comme identifiant unique de correspondance,
                // pour éviter de créer des doublons à chaque synchronisation).
                var existingPreparation = await _context.Preparations
                    .FirstOrDefaultAsync(p => p.GoogleEventId == calendarEvent.Id);

                // Si la préparation n'existe pas, on la crée.
                if (existingPreparation == null)
                {
                    var preparation = new Preparation
                    {
                        GoogleEventId = calendarEvent.Id,
                        Subject = calendarEvent.Summary,
                        StartDate = calendarEvent.Start,
                        EndDate = calendarEvent.End,
                        Status = "Upcoming",
                        // Le formateur & le créateur sont l'utilisateur
                        // réellement connecté.
                        FormateurId = currentUserId,
                        CreatedById = currentUserId
                    };
                    _context.Preparations.Add(preparation);
                }
                // Si elle existe déjà, on ne fait rien pour éviter de la dupliquer
                // ou d'écraser des données déjà saisies par le formateur.
            }

            // Enregistre tous les ajouts en une seule fois en base de données.
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Préparations synchronisées avec Google Calendar." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la synchronisation : {ex.Message}" });
        }
    }

    // GET: api/preparations
    // Récupère la liste de toutes les préparations, avec le formateur
    // et le créateur associés (Include = charge aussi ces objets liés).

    [HttpGet]
    public async Task<IActionResult> GetPreparations()
    {
        try
        {
            var preparations = await _context.Preparations
                .Include(p => p.Formateur)
                .Include(p => p.CreatedBy)
                .ToListAsync();

            return Ok(preparations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Une erreur est survenue : {ex.Message}" });
        }
    }

    // GET: api/preparations/5
    // Récupère une préparation précise par son identifiant.

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPreparation(int id)
    {
        try
        {
            var preparation = await _context.Preparations
                .Include(p => p.Formateur)
                .Include(p => p.CreatedBy)
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

    // POST: api/preparations
    // Crée une nouvelle préparation manuellement (sans passer par Google Calendar).
    // [Authorize] : !!! il faut être connecté pour créer une préparation.


    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostPreparation(PreparationDto dto)
    {
        try
        {
            // Récupère l'utilisateur connecté pour l'enregistrer comme créateur.
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Unauthorized(new { Error = "Utilisateur non connecté." });
            }
            var currentUserId = int.Parse(currentUserIdClaim);

            // Vérifie que le formateur assigné existe réellement, pour
            // éviter une erreur de contrainte de clef étrangère 
            // si le frontend envoie un FormateurId invalide.
            var assignedFormateur = await _context.Formateurs.FindAsync(dto.FormateurId);
            if (assignedFormateur == null)
            {
                return BadRequest(new { Error = $"Aucun formateur trouvé avec l'ID {dto.FormateurId}." });
            }

            // On ne peut pas assigner une nouvelle préparation à un compte
            // désactivé (ex. : formateur qui a quitté l'entreprise). Les
            // préparations déjà existantes pour ce formateur, elles,
            // restent visibles normalement (voir Formateur.IsActive).
            if (!assignedFormateur.IsActive)
            {
                return BadRequest(new { Error = $"{assignedFormateur.Name} a un compte désactivé : impossible de lui assigner une nouvelle préparation." });
            }

            // Mappe le DTO (champs saisis dans le formulaire) vers
            // l'entité EF Core, puis il faut compléter nous-mêmes CreatedById et
            // CreatedAt : le créateur doit toujours être l'utilisateur
            // connecté, jamais une valeur envoyée par le frontend.
            var preparation = new Preparation
            {
                Subject = dto.Subject,
                Description = dto.Description,
                FormateurId = dto.FormateurId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                GoogleEventId = dto.GoogleEventId,
                CreatedById = currentUserId,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Preparations.Add(preparation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPreparation), new { id = preparation.Id }, preparation);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la création : {ex.Message}" });
        }
    }

    // PUT: api/preparations/5
    // Met à jour une préparation existante.
    // Seuls le créateur de la préparation ou un administrateur (rôle "1")
    // peuvent la modifier.


    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPreparation(int id, PreparationDto dto)
    {
        try
        {
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

            var existingPreparation = await _context.Preparations
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingPreparation == null)
            {
                return NotFound(new { Error = $"Aucune préparation trouvée avec l'identifiant {id}." });
            }

            // Vérif. des droits : seul le créateur ou un admin peut modifier.
            if (existingPreparation.CreatedById != currentUserId && currentUserRole != "1")
            {
                return Forbid(); // Renvoie un code 403 (Interdit).
            }

            // Vérifie que le formateur assigné existe réellement.
            var assignedFormateur = await _context.Formateurs.FindAsync(dto.FormateurId);
            if (assignedFormateur == null)
            {
                return BadRequest(new { Error = $"Aucun formateur trouvé avec l'ID {dto.FormateurId}." });
            }

            // On bloque seulement le cas où on tente de réassigner la
            // préparation à un formateur désactivé. Si le formateur assigné
            // ne change pas (dto.FormateurId == FormateurId d'origine), on
            // laisse passer : sinon on ne pourrait plus du tout modifier les
            // anciennes préparations d'un formateur qui a quitté l'entreprise
            // (ex. : corriger une faute de frappe dans le sujet).
            if (!assignedFormateur.IsActive && dto.FormateurId != existingPreparation.FormateurId)
            {
                return BadRequest(new { Error = $"{assignedFormateur.Name} a un compte désactivé : impossible de lui réassigner cette préparation." });
            }

            // Met à jour uniquement les champs modifiables par le
            // formateur/admin. CreatedById et CreatedAt restent ceux de
            // l'entité existante en base, jamais ceux du DTO.
            existingPreparation.Subject = dto.Subject;
            existingPreparation.Description = dto.Description;
            existingPreparation.FormateurId = dto.FormateurId;
            existingPreparation.StartDate = dto.StartDate;
            existingPreparation.EndDate = dto.EndDate;
            existingPreparation.Status = dto.Status;
            existingPreparation.GoogleEventId = dto.GoogleEventId;
            existingPreparation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 204 No Content = la mise à jour a réussi, il n'y a rien à renvoyer.
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Cette exception arrive si la préparation a été supprimée
            // entre le moment où on l'a lue et le moment où on a essayé de la sauvegarder.

            if (!PreparationExists(id))
            {
                return NotFound(new { Error = $"La préparation avec l'ID {id} n'existe plus." });
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

    // DELETE: api/preparations/5
    // Supprime une préparation. Mêmes règles de droits que pour la modification.

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePreparation(int id)
    {
        try
        {
            var preparation = await _context.Preparations
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (preparation == null)
            {
                return NotFound(new { Error = $"Aucune préparation trouvée avec l'identifiant {id}." });
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

            if (preparation.CreatedById != currentUserId && currentUserRole != "1")
            {
                return Forbid();
            }

            _context.Preparations.Remove(preparation);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la suppression : {ex.Message}" });
        }
    }

    // Méthode privée utilitaire : vérifie si une préparation avec cet id
    // existe encore en base (utilisée en cas de conflit de concurrence).

    private bool PreparationExists(int id)
    {
        return _context.Preparations.Any(e => e.Id == id);
    }
}
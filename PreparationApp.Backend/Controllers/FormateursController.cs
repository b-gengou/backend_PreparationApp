// Contrôleur permettant de gérer les requêtes liées aux formateurs.
// Règles : rôle "1" = admin, rôle "2" = formateur :
// a) Consulter la liste / un formateur (GET) : tout utilisateur connecté
// (formateur ou admin), car il est normal pour un formateur de voir
// ses collègues (ex. : pour filtrer le catalogue par formateur).
// b) Créer un formateur via ce contrôleur (POST) : réservé à l'admin,
// car l'inscription "normale" d'un nouveau formateur passe déjà par
// POST /api/auth/register (AuthController.cs).
// c) Modifier le rôle d'un formateur (PUT .../role) : réservé à l'admin,
// seule façon de promouvoir un formateur en administrateur
// une fois qu'un premier admin existe déjà (voir le tout premier admin,
// créé manuellement en BD via une requête SQL).


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PreparationApp.Backend.ModelsBD;

namespace PreparationApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormateursController : ControllerBase
{
    private readonly AppDbContext _context;

    public FormateursController(AppDbContext context)
    {
        _context = context;
    }

    
    // GET: api/formateurs
    // Récupère la liste de tous les formateurs.
    // Règles : tout utilisateur connecté (formateur ou admin).
    
    [Authorize]
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
            return StatusCode(500, new { Error = $"Une erreur est survenue : {ex.Message}" });
        }
    }


    // GET: api/formateurs/5
    // Récupère un formateur spécifique par son identifiant.
    // Règles : tout utilisateur connecté (formateur ou admin).

    [Authorize]
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

    
    // POST: api/formateurs
    // Crée un nouveau formateur directement (sans passer par le formulaire
    // d'inscription classique).
    // Règles : réservé à l'admin (policy "AdminOnly" définie dans Program.cs),
    // car c'est une création "manuelle" utilisée pour qu'un
    // admin ajoute un compte à un collègue, en dehors du flux d'inscription
    // public POST /api/auth/register.
    
    [Authorize(Policy = "AdminOnly")]
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

    
    // PUT: api/formateurs/5/role
    // Modifie le rôle d'un formateur existant : le promouvoir administrateur
    // ("1"), ou le repasser formateur simple ("2").
    // Règles : réservé à l'admin (policy "AdminOnly"), car changer le rôle
    // de quelqu'un est une action sensible qui ne doit jamais être
    // accessible à un formateur simple, même sur son propre compte
    // (sinon n'importe quel formateur pourrait s'auto-promouvoir admin).
    
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleModel model)
    {
        try
        {
            // On accepte uniquement les valeurs de rôle connues ("1" ou "2"),
            // pour éviter qu'une valeur invalide (ex: "abc" ou "99") ne soit
            // enregistrée en base et ne casse les vérifications de droits ailleurs.
            if (model.Role != "1" && model.Role != "2")
            {
                return BadRequest(new { Error = "Le rôle doit être '1' (admin) ou '2' (formateur)." });
            }

            var formateur = await _context.Formateurs.FindAsync(id);
            if (formateur == null)
            {
                return NotFound(new { Error = $"Aucun formateur trouvé avec l'identifiant {id}." });
            }

            // Super protection : on ne peut jms retirer le statut admin au
            // au dernier administrateur actif. 
            // On ne bloque que la rétrogradation ("1" -> "2") d'un admin
            // qui est actuellement le dernier admin actif restant ; promouvoir
            // un formateur en admin ("2" -> "1") n'est, lui, jamais bloqué.
            if (formateur.Role == "1" && model.Role == "2")
            {
                var activeAdminCount = await _context.Formateurs
                    .CountAsync(f => f.Role == "1" && f.IsActive);

                // Si ce formateur est lui-même actif et qu'il est le seul
                // admin actif compté, le rétrograder le ferait disparaître :
                // on bloque ce cas précis.
                if (formateur.IsActive && activeAdminCount <= 1)
                {
                    return BadRequest(new { Error = $"Impossible de retirer le statut administrateur à {formateur.Name} : il s'agit du dernier administrateur actif. Promouvez d'abord un autre formateur en administrateur." });
                }
            }

            // Mise à jour du rôle, puis sauvegarde en base.
            formateur.Role = model.Role;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Le rôle du formateur {formateur.Name} est maintenant '{model.Role}'." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la mise à jour du rôle : {ex.Message}" });
        }
    }

    
    // PUT: api/formateurs/5/deactivate
    // Désactive un compte formateur ou admin (ex. : départ de l'entreprise).
    // Le compte ne peut alors plus se connecter (vérifié dans
    // AuthController.Login).
    // Toutes ses données (préparations
    // créées/assignées, comptes-rendus, ressources) restent intactes en
    // base.
    // Résultat, on ne supprime jms le formateur, on le marque seulement
    // inactif. Son nom continue donc d'apparaître partout dans l'app,
    // suffixé par "(compte désactivé)" via Formateur.DisplayName, pour
    // que les autres formateurs et admins sachent toujours qui a fait quoi.
    // Règles : réservé à l'admin (policy "AdminOnly"), même règle que pour
    // UpdateRole — une action sensible sur le compte de quelqu'un d'autre.
    
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateFormateur(int id)
    {
        try
        {
            var formateur = await _context.Formateurs.FindAsync(id);
            if (formateur == null)
            {
                return NotFound(new { Error = $"Aucun formateur trouvé avec l'identifiant {id}." });
            }

            if (!formateur.IsActive)
            {
                return BadRequest(new { Error = $"Le compte de {formateur.Name} est déjà désactivé." });
            }

            // !!! Même règle que pour UpdateRole (voir le
            // commentaire détaillé dans cette méthode). Désactiver le
            // dernier administrateur actif aurait exactement le même effet
            // de fou que le rétrograder : plus personne ne pourrait plus
            // se connecter en tant qu'admin pour réactiver quelqu'un (sauf en BD).
            if (formateur.Role == "1")
            {
                var activeAdminCount = await _context.Formateurs
                    .CountAsync(f => f.Role == "1" && f.IsActive);

                if (activeAdminCount <= 1)
                {
                    return BadRequest(new { Error = $"Impossible de désactiver {formateur.Name} : il s'agit du dernier administrateur actif. Promouvez d'abord un autre formateur en administrateur." });
                }
            }

            formateur.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Le compte de {formateur.Name} a été désactivé. Ses préparations, comptes-rendus et ressources restent visibles." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la désactivation : {ex.Message}" });
        }
    }

    
    // PUT: api/formateurs/5/reactivate
    // Réactive un compte précédemment désactivé (ex. : la personne revient,
    // ou la désactivation était une erreur). Le formateur peut alors se
    // reconnecter normalement.
    // Règles/droits : réservé à l'admin (policy "AdminOnly").
    
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id}/reactivate")]
    public async Task<IActionResult> ReactivateFormateur(int id)
    {
        try
        {
            var formateur = await _context.Formateurs.FindAsync(id);
            if (formateur == null)
            {
                return NotFound(new { Error = $"Aucun formateur trouvé avec l'identifiant {id}." });
            }

            if (formateur.IsActive)
            {
                return BadRequest(new { Error = $"Le compte de {formateur.Name} est déjà actif." });
            }

            formateur.IsActive = true;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Le compte de {formateur.Name} a été réactivé." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la réactivation : {ex.Message}" });
        }
    }
        
    // Modèle représentant le corps JSON attendu pour la route UpdateRole.
    // Exemple de requête envoyée par le frontend : { "role": "1" }
    
    public class UpdateRoleModel
    {
        public string Role { get; set; } = string.Empty;
    }
}
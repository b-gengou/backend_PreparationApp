// Modèle représentant un formateur dans PreparationApp.
// Ce modèle inclut les propriétés nécessaires pour l'authentification et la
// gestion des rôles.
// "Role" (string), permet d'ajouter d'autres rôles à l'avenir
// (ex.: "3" pour un futur rôle "responsable pédagogique").
// Convention actuelle : "1" = administrateur, "2" = formateur simple.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace PreparationApp.Backend.ModelsBD;

public class Formateur
{
    // Id. unique du formateur (clef primaire).
    [Key]
    public int Id { get; set; }

    // Nom complet du formateur.
    [Required(ErrorMessage = "Le nom du formateur est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
    public string Name { get; set; } = string.Empty;

    // Adresse courriel du formateur, utilisée pour la connexion.
    [Required(ErrorMessage = "L'email du formateur est obligatoire.")]
    [StringLength(255, ErrorMessage = "L'email ne peut pas dépasser 255 caractères.")]
    [EmailAddress(ErrorMessage = "L'email doit être une adresse valide.")]
    public string Email { get; set; } = string.Empty;

    // Mot de passe du formateur (haché en production).
    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir entre 6 et 255 caractères.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    // Id. du calendrier Google associé au formateur.
    [StringLength(255, ErrorMessage = "L'ID du calendrier Google ne peut pas dépasser 255 caractères.")]
    public string GoogleCalendarId { get; set; } = string.Empty;

    // Rôle du formateur dans PreparationApp, sous forme de texte.
    // Convention : "1" = administrateur (droits étendus, peut tout
    // modifier/supprimer), "2" = formateur simple (peut seulement
    // modifier/supprimer ce qu'il a lui-même créé).
    // C'est cette valeur qui sera inscrite dans le token JWT au moment
    // du login, puis relue par le backend à chaque requête protégée.


    [Required(ErrorMessage = "Le rôle du formateur est obligatoire.")]
    [StringLength(10, ErrorMessage = "Le rôle ne peut pas dépasser 10 caractères.")]
    public string Role { get; set; } = "2"; // Par défaut : formateur simple.

    // Indique si le compte est actif. Un compte désactivé (ex: formateur
    // ou admin qui a quitté l'entreprise) ne peut plus se connecter, mais
    // toutes ses données (préparations créées, comptes-rendus, ressources)
    // restent intactes en base — voir DeleteBehavior.Restrict dans
    // AppDbContext.cs. On NE supprime jamais le formateur lui-même : ça
    // casserait la traçabilité de qui a réellement fait quoi.
    // Par défaut à true : tout nouveau compte est actif dès sa création.
    public bool IsActive { get; set; } = true;

    // Liste des préparations associées à ce formateur (relation 1:N).
    public ICollection<Preparation> Preparations { get; set; } = new List<Preparation>();

    // Liste des préparations créées par ce formateur (relation 1:N).
    [InverseProperty("CreatedBy")]
    public ICollection<Preparation> CreatedPreparations { get; set; } = new List<Preparation>();

    // Ignore les boucles de référence lors de la sérialisation JSON.
    [JsonIgnore]
    [NotMapped]
    public Formateur FormateurReference => this;

    // Nom à afficher dans toute l'interface (listes de préparations,
    // ressources, comptes-rendus...). Calculé une seule fois ici plutôt
    // que de réimplémenter la même logique dans chaque composant React —
    // un seul endroit à maintenir.
    //
    // a) Si le compte est actif : nom complet (ex. : "Charles Martel").
    // b) Si le compte est désactivé : seulement les initiales ("C. M."),
    // pour que les autres formateurs/admins sachent qu'une préparation,
    // un compte-rendu ou une ressource existe bien et a un auteur,
    // sans afficher son identité complète après son départ.
    // Si le compte est réactivé, IsActive repasse à true et le nom
    // complet réapparaît automatiquement, sans rien à faire de plus :
    // DisplayName n'est jamais stocké, il est recalculé à chaque lecture.
    [NotMapped]
    public string DisplayName => IsActive ? Name : GetInitials(Name);

    // Transforme "Charles Martel" en "C. M.", ou "Jean-Pierre Dubois" en
    // "J-P. D." : un mot composé avec un tiret garde une initiale par
    // segment (séparées par un tiret, sans point entre elles), puis un
    // point final est ajouté au dernier segment du mot. Les mots séparés
    // par un espace (typiquement prénom / nom) sont eux séparés par
    // un espace dans le résultat. Repose sur les espaces et les tirets
    // saisis dans FormateursForm.tsx.
    private static string GetInitials(string fullName)
    {
        var words = fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (words.Length == 0)
        {
            return string.Empty;
        }

        // Pour chaque mot (ex. : "Jean-Pierre"), découpe d'abord sur le
        // tiret (-> "Jean", "Pierre"), prend l'initiale de chaque
        // segment, puis les rejoint avec un tiret : "J-P". Le point
        // final n'est ajouté qu'une fois, à la fin du mot complet.
        var initialsPerWord = words.Select(word =>
        {
            var segments = word.Split('-', StringSplitOptions.RemoveEmptyEntries);
            var segmentInitials = segments.Select(s => char.ToUpper(s[0]).ToString());
            return string.Join("-", segmentInitials) + ".";
        });

        // string.Join place les mots ("C.", "M-A.") côte à côte avec un
        // espace entre eux : "C. M-A.".
        return string.Join(" ", initialsPerWord);
    }
}
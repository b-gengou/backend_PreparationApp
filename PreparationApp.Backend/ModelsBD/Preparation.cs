using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant une préparation (session de formation) dans l'application.
// Ce modèle inclut les propriétés nécessaires pour la gestion des dates, des formateurs, et des relations.
public class Preparation
{
    // Id. unique de la préparation (clef primaire).
    // Utilisé comme référence dans les autres tables (ex: PreparationReport, PreparationResource).
    [Key]
    public int Id { get; set; }

    // Titre ou sujet de la préparation.
    // Obligatoire et limité à 255 caractères pour éviter les entrées trop longues.
    [Required(ErrorMessage = "Le titre de la préparation est obligatoire.")]
    [StringLength(255, ErrorMessage = "Le titre ne peut pas dépasser 255 caractères.")]
    public string Subject { get; set; } = string.Empty;

    // Description détaillée de la préparation.
    // Optionnelle, utile pour fournir plus de contexte.
    [StringLength(1000, ErrorMessage = "La description ne peut pas dépasser 1000 caractères.")]
    public string Description { get; set; } = string.Empty;

    // Id. du formateur associé à cette préparation (clef étrangère).
    // !!! Obligatoire : chaque préparation doit être associée à un formateur.
    [Required(ErrorMessage = "Le formateur est obligatoire pour une préparation.")]
    public int FormateurId { get; set; }

    // Propriété de navigation rendue explicitement nullable
    // (Formateur? au lieu de Formateur). Sans ce nullable "?", ASP.NET Core exigeait
    // que le frontend fournisse un objet "formateur" complet dans le corps
    // de la requête POST/PUT, en plus de "formateurId" — ce qui provoquait
    // une erreur 400 ("The Formateur field is required") même lorsque
    // formateurId était correctement indiqué. EF Core continue de
    // charger cet objet automatiquement depuis la base via FormateurId
    // quand on utilise .Include(p => p.Formateur) dans un contrôleur GET ;
    // cette correction ne change que la validation du modèle en entrée.

    [ForeignKey("FormateurId")]
    public Formateur? Formateur { get; set; }

    // Id. du formateur qui a créé cette préparation (clef étrangère).
    // Utilisé pour vérifier les droits d'accès (seul le créateur ou un admin peut modifier/supprimer).
    [Required(ErrorMessage = "Le créateur de la préparation est obligatoire.")]
    public int CreatedById { get; set; }

    // idem que pour Formateur ci-dessus (nullable).
    [ForeignKey("CreatedById")]
    public Formateur? CreatedBy { get; set; }

    // Date et heure de début de la préparation.
    // Obligatoire pour planifier la session.
    [Required(ErrorMessage = "La date de début est obligatoire.")]
    public DateTime StartDate { get; set; }

    // Date et heure de fin de la préparation.
    // Obligatoire pour connaître la durée de la session.
    [Required(ErrorMessage = "La date de fin est obligatoire.")]
    public DateTime EndDate { get; set; }

    // Statut de la préparation (ex: "Upcoming", "In Progress", "Completed", "Cancelled").
    // Optionnel, utile pour filtrer ou trier les préparations.
    [StringLength(50, ErrorMessage = "Le statut ne peut pas dépasser 50 caractères.")]
    public string Status { get; set; } = "Upcoming";

    // Id. de l'événement dans Google Calendar.
    // Optionnel : certaines préparations peuvent ne pas être liées à un événement Google.
    [StringLength(255, ErrorMessage = "L'ID de l'événement Google ne peut pas dépasser 255 caractères.")]
    public string GoogleEventId { get; set; } = string.Empty;

    // Date de création de la préparation dans la base de données.
    // Utile pour le tri ou l'audit.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Date de la dernière mise à jour de la préparation.
    // Utile pour suivre les modifications.
    public DateTime? UpdatedAt { get; set; }

    // Liste des comptes-rendus associés à cette préparation (relation 1:N).
    // Un compte-rendu est lié à une seule préparation, mais une préparation peut avoir plusieurs comptes-rendus.
    public ICollection<PreparationReport> Reports { get; set; } = new List<PreparationReport>();

    // Liste des ressources associées à cette préparation (relation N:N via PreparationResource).
    // Une préparation peut avoir plusieurs ressources, et une ressource peut être associée à plusieurs préparations.
    public ICollection<PreparationResource> PreparationResources { get; set; } = new List<PreparationResource>();

    // Ignore les boucles de référence lors de la sérialisation JSON.
    // Évite les erreurs de sérialisation circulaire (ex: Preparation -> Formateur -> Preparation).
    // Ici : nullable également, car ces propriétés dérivent directement
    // de Formateur/CreatedBy ci-dessus, qui sont désormais nullables.
    [JsonIgnore]
    [NotMapped]
    public Formateur? FormateurReference => Formateur;

    // Ignore les boucles de référence pour le créateur lors de la sérialisation JSON.
    [JsonIgnore]
    [NotMapped]
    public Formateur? CreatedByReference => CreatedBy;
}
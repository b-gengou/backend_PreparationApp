// DTO utilisé pour les requêtes
// POST/PUT venant du frontend. Contrairement au modèle PreparationReport,
// il ne contient que les champs que le formateur doit réellement saisir.
// !!! DTO séparé du modèle EF Core PreparationReport 
// a) Email et CreatedAt sont remplis par le serveur (jamais par le frontend).
// b) Preparation (la propriété de navigation) ne doit jamais être envoyée
//   par le client : seul PreparationId (un simple int) doit l'être.
// c) Sans ce DTO, ASP.NET Core valide le JSON reçu directement contre le
//   modèle PreparationReport (méchanceté de [ApiController]), et rejette la
//   requête à cause des [Required] sur Email et Preparation avant même
//   que le code du contrôleur ait la chance de les remplir lui-même.

using System.ComponentModel.DataAnnotations;

namespace PreparationApp.Backend.ModelsBD.Dtos;

public class PreparationReportDto
{
    [Required(ErrorMessage = "Le sujet abordé est obligatoire.")]
    [StringLength(1000, ErrorMessage = "Le sujet ne peut pas dépasser 1000 caractères.")]
    public string SubjectsCovered { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'objectif de la journée est obligatoire.")]
    [StringLength(1000, ErrorMessage = "L'objectif ne peut pas dépasser 1000 caractères.")]
    public string DailyObjectives { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Les supports de référence ne peuvent pas dépasser 1000 caractères.")]
    public string ReferenceSupports { get; set; } = string.Empty;

    // [Url] sur une chaîne vide pose problème selon la version de .NET :
    // il faut relâcher la contrainte ici, et il faut revalider manuellement seulement
    // si le champ n'est pas vide (voir contrôleur).
    [StringLength(500, ErrorMessage = "Le lien ne peut pas dépasser 500 caractères.")]
    public string DirectoryLink { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La liste des fichiers modifiés ne peut pas dépasser 1000 caractères.")]
    public string ModifiedFiles { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La liste des nouveaux exercices ne peut pas dépasser 1000 caractères.")]
    public string NewExercises { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Le lien ne peut pas dépasser 500 caractères.")]
    public string WorkDirectoryLink { get; set; } = string.Empty;

    [Required(ErrorMessage = "La date planifiée de la matière est obligatoire.")]
    public DateTime PlannedDate { get; set; }

    [Required(ErrorMessage = "La durée du cours (en journées) est obligatoire.")]
    [Range(1, 365, ErrorMessage = "La durée doit être comprise entre 1 et 365 jours.")]
    public int CourseDurationDays { get; set; }

    [StringLength(1000, ErrorMessage = "Les points d'achoppement ne peuvent pas dépasser 1000 caractères.")]
    public string TechnicalIssues { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Cette réponse ne peut pas dépasser 100 caractères.")]
    public string SecondOpinionNeed { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "L'action à entreprendre ne peut pas dépasser 500 caractères.")]
    public string SecondOpinionAction { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Le temps consacré ne peut pas dépasser 100 caractères.")]
    public string TimeSpent { get; set; } = string.Empty;

    // Seul un entier est nécessaire ici, jamais l'objet Preparation complet.
    [Required(ErrorMessage = "La préparation est obligatoire pour un compte-rendu.")]
    public int PreparationId { get; set; }
}
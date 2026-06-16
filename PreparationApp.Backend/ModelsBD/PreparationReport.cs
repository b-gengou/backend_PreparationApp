using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant un compte-rendu de préparation dans l'application.
public class PreparationReport
{
    // Identifiant unique du compte-rendu (clef primaire).
    [Key]
    public int Id { get; set; }

    // Identifiant de la préparation associée à ce compte-rendu (clef étrangère).
    [Required(ErrorMessage = "La préparation associée est obligatoire.")]
    public int PreparationId { get; set; }

    // Objet Preparation associé à ce compte-rendu (propriété de navigation).
    [ForeignKey("PreparationId")]
    public Preparation Preparation { get; set; } = null!;

    // Sujets couverts pendant la préparation.
    [StringLength(1000, ErrorMessage = "Les sujets couverts ne peuvent pas dépasser 1000 caractères.")]
    public string SubjectsCovered { get; set; } = string.Empty;

    // Objectifs du jour pour la préparation.
    [StringLength(1000, ErrorMessage = "Les objectifs du jour ne peuvent pas dépasser 1000 caractères.")]
    public string DailyObjectives { get; set; } = string.Empty;

    // Supports de référence utilisés pendant la préparation.
    [StringLength(1000, ErrorMessage = "Les supports de référence ne peuvent pas dépasser 1000 caractères.")]
    public string ReferenceSupports { get; set; } = string.Empty;

    // Lien vers le dossier de travail.
    [StringLength(500, ErrorMessage = "Le lien vers le dossier de travail ne peut pas dépasser 500 caractères.")]
    [RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", ErrorMessage = "Le lien doit être une URL valide.")]
    public string DirectoryLink { get; set; } = string.Empty;

    // Liste des fichiers modifiés pendant la préparation.
    [StringLength(1000, ErrorMessage = "La liste des fichiers modifiés ne peut pas dépasser 1000 caractères.")]
    public string ModifiedFiles { get; set; } = string.Empty;

    // Liste des nouveaux exercices créés pendant la préparation.
    [StringLength(1000, ErrorMessage = "La liste des nouveaux exercices ne peut pas dépasser 1000 caractères.")]
    public string NewExercises { get; set; } = string.Empty;

    // Lien vers le dossier de travail principal.
    [StringLength(500, ErrorMessage = "Le lien vers le dossier principal ne peut pas dépasser 500 caractères.")]
    [RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", ErrorMessage = "Le lien doit être une URL valide.")]
    public string WorkDirectoryLink { get; set; } = string.Empty;

    // Date prévue pour la préparation.
    [Required(ErrorMessage = "La date prévue est obligatoire.")]
    public DateTime PlannedDate { get; set; }

    // Durée du cours en jours.
    [Range(1, 365, ErrorMessage = "La durée du cours doit être comprise entre 1 et 365 jours.")]
    public int CourseDurationDays { get; set; }

    // Problèmes techniques rencontrés pendant la préparation.
    [StringLength(1000, ErrorMessage = "Les problèmes techniques ne peuvent pas dépasser 1000 caractères.")]
    public string TechnicalIssues { get; set; } = string.Empty;

    // Indique si un deuxième avis est nécessaire.
    [StringLength(100, ErrorMessage = "Le champ 'Deuxième avis nécessaire' ne peut pas dépasser 100 caractères.")]
    public string SecondOpinionNeed { get; set; } = string.Empty;

    // Action à entreprendre pour le deuxième avis.
    [StringLength(500, ErrorMessage = "L'action pour le deuxième avis ne peut pas dépasser 500 caractères.")]
    public string SecondOpinionAction { get; set; } = string.Empty;

    // Temps passé sur la préparation.
    [StringLength(100, ErrorMessage = "Le temps passé ne peut pas dépasser 100 caractères.")]
    public string TimeSpent { get; set; } = string.Empty;

    // Email associé au compte-rendu.
    [StringLength(255, ErrorMessage = "L'email ne peut pas dépasser 255 caractères.")]
    [EmailAddress(ErrorMessage = "L'email doit être une adresse valide.")]
    public string Email { get; set; } = string.Empty;

    // Date de création du compte-rendu.
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
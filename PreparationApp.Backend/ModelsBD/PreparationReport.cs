using PreparationApp.Backend.ModelsBD;
// Modèle représentant un compte-rendu de préparation.
namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant un compte-rendu de préparation dans l'application.
public class PreparationReport
{
    // Identifiant unique du compte-rendu (clef primaire).
    public int Id { get; set; }

    // Identifiant de la préparation associée à ce compte-rendu (clef étrangère).
    public int PreparationId { get; set; }

    // Objet Preparation associé à ce compte-rendu (propriété de navigation).
    // Initialisé avec null! pour indiquer qu'il sera toujours initialisé par Entity Framework.
    public Preparation Preparation { get; set; } = null!;

    // Sujets couverts pendant la préparation.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string SubjectsCovered { get; set; } = string.Empty;

    // Objectifs du jour pour la préparation.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string DailyObjectives { get; set; } = string.Empty;

    // Supports de référence utilisés pendant la préparation.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string ReferenceSupports { get; set; } = string.Empty;

    // Lien vers le dossier de travail.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string DirectoryLink { get; set; } = string.Empty;

    // Liste des fichiers modifiés pendant la préparation.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string ModifiedFiles { get; set; } = string.Empty;

    // Liste des nouveaux exercices créés pendant la préparation.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string NewExercises { get; set; } = string.Empty;

    // Lien vers le dossier de travail principal.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string WorkDirectoryLink { get; set; } = string.Empty;

    // Date prévue pour la préparation.
    public DateTime PlannedDate { get; set; }

    // Durée du cours en jours.
    public int CourseDurationDays { get; set; }

    // Problèmes techniques rencontrés pendant la préparation.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string TechnicalIssues { get; set; } = string.Empty;

    // Indique si un deuxième avis est nécessaire.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string SecondOpinionNeed { get; set; } = string.Empty;

    // Action à entreprendre pour le deuxième avis.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string SecondOpinionAction { get; set; } = string.Empty;

    // Temps passé sur la préparation.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string TimeSpent { get; set; } = string.Empty;

    // Email associé au compte-rendu.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string Email { get; set; } = string.Empty;

    // Date de création du compte-rendu.
    // Initialisé avec la date actuelle par défaut.
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
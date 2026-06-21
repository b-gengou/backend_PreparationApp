// Modèle "PreparationReport" (compte-rendu de préparation).
// Un compte-rendu est rempli par un formateur !!! après !!! sa journée de préparation.
// Il reprend les champs du Google Form actuel (cf. cahier des charges,
// section 1.2 et section 6.2 - dictionnaire des données), pour que rien ne soit perdu
// lors du passage de Google Forms vers l'application.

using System.ComponentModel.DataAnnotations;       // Permet d'utiliser [Required], [StringLength], [Url], etc.
using System.ComponentModel.DataAnnotations.Schema; // Permet d'utiliser [ForeignKey] pour les clefs étrangères.

namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant un compte-rendu pour une préparation.
// Une préparation peut avoir plusieurs comptes-rendus (relation 1:N, voir AppDbContext.cs).
public class PreparationReport
{
    // EF Core va générer un nombre auto-incrémenté (1, 2, 3...).


    [Key]
    public int Id { get; set; }


    // Sujet(s) abordé(s) pendant la préparation.
    // Correspond à US01 du cahier des charges : champ obligatoire.
    // [Required] = le champ ne peut pas être vide, sinon une erreur 400 est renvoyée.
    // [StringLength(1000)] = limite la taille du texte à 1000 caractères en base.

    [Required(ErrorMessage = "Le sujet abordé est obligatoire.")]
    [StringLength(1000, ErrorMessage = "Le sujet ne peut pas dépasser 1000 caractères.")]
    public string SubjectsCovered { get; set; } = string.Empty;

  
    // Objectif(s) de la journée de préparation.
    // Correspond à US02 : champ obligatoire.
  
    [Required(ErrorMessage = "L'objectif de la journée est obligatoire.")]
    [StringLength(1000, ErrorMessage = "L'objectif ne peut pas dépasser 1000 caractères.")]
    public string DailyObjectives { get; set; } = string.Empty;

  
    // Support(s) de référence utilisés (texte libre : titres de livres, PDF, etc.).
    // Correspond à US03 : champ optionnel (texte libre).
  
    [StringLength(1000, ErrorMessage = "Les supports de référence ne peuvent pas dépasser 1000 caractères.")]
    public string ReferenceSupports { get; set; } = string.Empty;

  
    // Lien vers le répertoire utilisé (Drive, GitHub, etc.).
    // Correspond à US04 : doit être une URL valide.
    // [Url] vérifie que le texte ressemble bien à une adresse web (http:// ou https://).
  
    [StringLength(500, ErrorMessage = "Le lien ne peut pas dépasser 500 caractères.")]
    [Url(ErrorMessage = "Le lien vers le répertoire doit être une URL valide.")]
    public string DirectoryLink { get; set; } = string.Empty;

  
    // Fichiers modifiés ou ajoutés dans le répertoire (ex: "exercice1.sql, exercice2.sql").
    // Correspond à US05 : champ optionnel (texte libre).
  
    [StringLength(1000, ErrorMessage = "La liste des fichiers modifiés ne peut pas dépasser 1000 caractères.")]
    public string ModifiedFiles { get; set; } = string.Empty;

  
    // Nouveaux exercices créés ou modifiés.
    // Correspond à US06 : champ optionnel (texte libre).
  
    [StringLength(1000, ErrorMessage = "La liste des nouveaux exercices ne peut pas dépasser 1000 caractères.")]
    public string NewExercises { get; set; } = string.Empty;

  
    // Lien vers le répertoire de travail dans Google Drive.
    // Correspond à US07 : doit être une URL valide.
  
    [StringLength(500, ErrorMessage = "Le lien ne peut pas dépasser 500 caractères.")]
    [Url(ErrorMessage = "Le lien vers le répertoire de travail doit être une URL valide.")]
    public string WorkDirectoryLink { get; set; } = string.Empty;

  
    // Date à laquelle la matière préparée sera enseignée en formation.
    // Correspond à US08 : doit être une date valide.
  
    [Required(ErrorMessage = "La date planifiée de la matière est obligatoire.")]
    public DateTime PlannedDate { get; set; }

  
    // Durée du cours en nombre de journées de formation.
    // Correspond à US09 : doit être un nombre entier.
  
    [Required(ErrorMessage = "La durée du cours (en journées) est obligatoire.")]
    [Range(1, 365, ErrorMessage = "La durée doit être comprise entre 1 et 365 jours.")]
    public int CourseDurationDays { get; set; }

  
    // Points d'achoppement techniques ou pédagogiques rencontrés.
    // Correspond à US010 : champ optionnel (texte libre).
  
    [StringLength(1000, ErrorMessage = "Les points d'achoppement ne peuvent pas dépasser 1000 caractères.")]
    public string TechnicalIssues { get; set; } = string.Empty;

  
    // Indique si le formateur a besoin d'un second avis ou d'un complément.
    // Correspond à US011 : stocké comme texte ("Oui"/"Non" ou une des 4 options du form).
  
    [StringLength(100, ErrorMessage = "Cette réponse ne peut pas dépasser 100 caractères.")]
    public string SecondOpinionNeed { get; set; } = string.Empty;

  
    // Action à entreprendre si un second avis est nécessaire.
    // Correspond à US012 : champ optionnel (texte libre).
  
    [StringLength(500, ErrorMessage = "L'action à entreprendre ne peut pas dépasser 500 caractères.")]
    public string SecondOpinionAction { get; set; } = string.Empty;

  
    // Temps réellement consacré à la préparation (format libre, ex: "03h30").
    // Correspond à US013 : champ optionnel (texte libre, pas un nombre,
    // car le formateur peut écrire "3h30" ou "environ 4h").
  
    [StringLength(100, ErrorMessage = "Le temps consacré ne peut pas dépasser 100 caractères.")]
    public string TimeSpent { get; set; } = string.Empty;

  
    // Adresse courriel du formateur qui a rempli ce compte-rendu (traçabilité).
    // Ce champ duplique volontairement Formateur.Email pour garder une trace
    // même si le formateur change d'adresse plus tard.
  
    [Required(ErrorMessage = "L'email du formateur est obligatoire pour la traçabilité.")]
    [StringLength(255)]
    [EmailAddress(ErrorMessage = "L'email doit être une adresse valide.")]
    public string Email { get; set; } = string.Empty;

  
    // Date de création du compte-rendu dans la base de données.
    // Remplie automatiquement par le serveur (DateTime.UtcNow = heure universelle),
    // jamais par le formateur lui-même.
  
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  
    // Clef étrangère : identifiant de la préparation à laquelle ce compte-rendu
    // est rattaché. Obligatoire car un compte-rendu n'existe jamais seul.
  
    [Required(ErrorMessage = "La préparation est obligatoire pour un compte-rendu.")]
    public int PreparationId { get; set; }

  
    // Propriété de navigation : permet d'accéder directement à l'objet
    // Preparation complet depuis un PreparationReport (ex. : report.Preparation.Subject).
    // [ForeignKey("PreparationId")] indique à EF Core que cette propriété
    // correspond à la colonne PreparationId définie juste au-dessus.
  
    [ForeignKey("PreparationId")]
    public Preparation Preparation { get; set; } = null!;
}
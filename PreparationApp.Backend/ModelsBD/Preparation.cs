using PreparationApp.Backend.ModelsBD;

//Modèle représentant une préparation (session de formation)
namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant une préparation (session de formation) dans l'application.
public class Preparation
{
    // Identifiant unique de la préparation (clef primaire).
    public int Id { get; set; }

    // Identifiant du formateur associé à cette préparation (clef étrangère).
    public int FormateurId { get; set; }

    // Objet Formateur associé à cette préparation (propriété de navigation).
    // Initialisé avec null! pour indiquer qu'il sera toujours initialisé par Entity Framework.
    public Formateur Formateur { get; set; } = null!;

    // Sujet ou titre de la préparation.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string Subject { get; set; } = string.Empty;

    // Date et heure de début de la préparation.
    public DateTime StartDate { get; set; }

    // Date et heure de fin de la préparation.
    public DateTime EndDate { get; set; }

    // Identifiant de l'événement dans Google Calendar.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string GoogleEventId { get; set; } = string.Empty;

    // Statut de la préparation (ex: "À venir", "En cours", "Terminé").
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string Status { get; set; } = string.Empty;

    // Liste des comptes-rendus associés à cette préparation (relation 1:N).
    // Initialisé avec une liste vide pour éviter les problèmes de nullabilité.
    public ICollection<PreparationReport> Reports { get; set; } = new List<PreparationReport>();

    // Liste des ressources associées à cette préparation (relation N:N).
    // Initialisé avec une liste vide pour éviter les problèmes de nullabilité.
    public ICollection<PreparationResource> PreparationResources { get; set; } = new List<PreparationResource>();
}
//using PreparationApp.Backend.ModelsBD
namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant un formateur dans l'application.
public class Formateur
{
    // Identifiant unique du formateur (clef primaire).
    public int Id { get; set; }

    // Nom du formateur.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string Name { get; set; } = string.Empty;

    // Adresse email du formateur.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string Email { get; set; } = string.Empty;

    // Identifiant du calendrier Google associé au formateur.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string GoogleCalendarId { get; set; } = string.Empty;

    // Liste des préparations associées à ce formateur (relation 1:N).
    // Initialisé avec une liste vide pour éviter les problèmes de nullabilité.
    public ICollection<Preparation> Preparations { get; set; } = new List<Preparation>();
}
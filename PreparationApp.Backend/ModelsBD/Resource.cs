//using PreparationApp.Backend.ModelsBD
//Modèle représentant une ressource (document, lien, etc.).
namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant une ressource (document, lien, etc.) dans l'application.
public class Resource
{
    // Identifiant unique de la ressource (clef primaire).
    public int Id { get; set; }

    // Titre de la ressource.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string Title { get; set; } = string.Empty;

    // Type de la ressource (ex: "PDF", "Lien", "Vidéo").
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string Type { get; set; } = string.Empty;

    // Lien vers la ressource.
    // Initialisé avec string.Empty pour éviter les problèmes de nullabilité.
    public string Link { get; set; } = string.Empty;

    // Liste des préparations associées à cette ressource (relation N:N).
    // Initialisé avec une liste vide pour éviter les problèmes de nullabilité.
    public ICollection<PreparationResource> PreparationResources { get; set; } = new List<PreparationResource>();
}
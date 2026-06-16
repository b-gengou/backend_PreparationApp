using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant une ressource (document, lien, etc.) dans l'application.
public class Resource
{
    // Identifiant unique de la ressource (clef primaire).
    [Key]
    public int Id { get; set; }

    // Titre de la ressource.
    [Required(ErrorMessage = "Le titre de la ressource est obligatoire.")]
    [StringLength(255, ErrorMessage = "Le titre ne peut pas dépasser 255 caractères.")]
    public string Title { get; set; } = string.Empty;

    // Type de la ressource (ex: "PDF", "Lien", "Vidéo").
    [Required(ErrorMessage = "Le type de la ressource est obligatoire.")]
    [StringLength(50, ErrorMessage = "Le type ne peut pas dépasser 50 caractères.")]
    public string Type { get; set; } = string.Empty;

    // Lien vers la ressource.
    [StringLength(500, ErrorMessage = "Le lien ne peut pas dépasser 500 caractères.")]
    [RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", ErrorMessage = "Le lien doit être une URL valide.")]
    public string Link { get; set; } = string.Empty;

    // Liste des préparations associées à cette ressource (relation N:N).
    public ICollection<PreparationResource> PreparationResources { get; set; } = new List<PreparationResource>();
}
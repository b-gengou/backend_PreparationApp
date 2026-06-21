// Modèle "Resource" (support de cours : PDF, vidéo, lien...).
// Une ressource peut être associée à plusieurs préparations (relation N:N,
// cf. PreparationResource.cs et AppDbContext.cs).
// CreatedById permet de savoir qui a ajouté cette ressource au catalogue,
// pour appliquer la règle de droits : seul le créateur ou un admin peut
// modifier/supprimer une ressource (voir ResourcesController.cs).


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant une ressource pour une préparation.
// Une ressource peut être associée à plusieurs préparations (relation N:N).
public class Resource
{
    // Identifiant unique de la ressource (clef primaire).
    [Key]
    public int Id { get; set; }

    // Nom de la ressource (ex. : "PDF du cours", "Lien vers la vidéo").
    [Required(ErrorMessage = "Le nom de la ressource est obligatoire.")]
    [StringLength(255, ErrorMessage = "Le nom ne peut pas dépasser 255 caractères.")]
    public string Name { get; set; } = string.Empty;

    // URL ou chemin vers la ressource.
    [Required(ErrorMessage = "L'URL de la ressource est obligatoire.")]
    [StringLength(1000, ErrorMessage = "L'URL ne peut pas dépasser 1000 caractères.")]
    public string Url { get; set; } = string.Empty;

    // Type de la ressource (ex. : "PDF", "Vidéo", "Lien").
    [StringLength(50, ErrorMessage = "Le type ne peut pas dépasser 50 caractères.")]
    public string Type { get; set; } = string.Empty;

    
    // Id. du formateur qui a créé cette ressource (clef étrangère).
    // Obligatoire : on doit toujours savoir qui a ajouté une ressource,
    // pour pouvoir appliquer la règle "seul le créateur ou l'admin peut modifier".
    
    [Required(ErrorMessage = "Le créateur de la ressource est obligatoire.")]
    public int CreatedById { get; set; }

    // Propriété de navigation : permet d'accéder directement à l'objet
    // Formateur complet depuis une Resource (ex. : resource.CreatedBy.Name).
    
    [ForeignKey("CreatedById")]
    public Formateur CreatedBy { get; set; } = null!;

    // Date de création de la ressource, remplie automatiquement par le serveur.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Liste des préparations associées à cette ressource (relation N:N via PreparationResource).
    public ICollection<PreparationResource> PreparationResources { get; set; } = new List<PreparationResource>();
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant la relation entre une préparation et une ressource (table de jointure pour la relation N:N).
public class PreparationResource
{
    // Identifiant de la préparation (clef étrangère).
    [Required(ErrorMessage = "L'ID de la préparation est obligatoire.")]
    public int PreparationId { get; set; }

    // Objet Preparation associé (propriété de navigation).
    [ForeignKey("PreparationId")]
    public Preparation Preparation { get; set; } = null!;

    // Identifiant de la ressource (clef étrangère).
    [Required(ErrorMessage = "L'ID de la ressource est obligatoire.")]
    public int ResourceId { get; set; }

    // Objet Resource associé (propriété de navigation).
    [ForeignKey("ResourceId")]
    public Resource Resource { get; set; } = null!;
}
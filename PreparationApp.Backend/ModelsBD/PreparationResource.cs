using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant la table de jointure entre Preparation et Resource.
// Permet de gérer la relation N:N entre préparations et ressources.
public class PreparationResource
{
    // Identifiant de la préparation (clef étrangère).
    [Required(ErrorMessage = "La préparation est obligatoire pour une ressource.")]
    public int PreparationId { get; set; }

    // Préparation associée (propriété de navigation).
    [ForeignKey("PreparationId")]
    public Preparation Preparation { get; set; } = null!;

    // Identifiant de la ressource (clef étrangère).
    [Required(ErrorMessage = "La ressource est obligatoire.")]
    public int ResourceId { get; set; }

    // Ressource associée (propriété de navigation).
    [ForeignKey("ResourceId")]
    public Resource Resource { get; set; } = null!;
}
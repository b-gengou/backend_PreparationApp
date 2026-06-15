using PreparationApp.Backend.ModelsBD;
//Modèle représentant la relation entre une préparation et une ressource (table de jointure pour la relation N:N).
namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant la relation entre une préparation et une ressource (table de jointure pour la relation N:N).
public class PreparationResource
{
    // Identifiant de la préparation (clef étrangère).
    public int PreparationId { get; set; }

    // Objet Preparation associé (propriété de navigation).
    // Initialisé avec null! pour indiquer qu'il sera toujours initialisé par Entity Framework.
    public Preparation Preparation { get; set; } = null!;

    // Identifiant de la ressource (clef étrangère).
    public int ResourceId { get; set; }

    // Objet Resource associé (propriété de navigation).
    // Initialisé avec null! pour indiquer qu'il sera toujours initialisé par Entity Framework.
    public Resource Resource { get; set; } = null!;
}
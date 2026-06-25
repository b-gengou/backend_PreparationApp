// DTO pour les requêtes  POST/PUT venant du frontend.
// Contrairement au modèle EF Core Resource,
// il ne contient QUE les champs que l'utilisateur saisit réellement dans
// le formulaire (ResourcesForm.tsx) : Name, Url, Type.
//
// !!! DTO est séparé du modèle EF Core Resource 
//
// Même raison que pour PreparationReportDto (cf. ce fichier pour détails complets) :
// avec [ApiController], ASP.NET Core valide le JSON
// reçu contre le type du paramètre de l'action avant d'exécuter le code
// du contrôleur.
//
// Si on utilise directement Resource comme paramètre :
// CreatedBy est une propriété de navigation non-nullable
// ([ForeignKey("CreatedById")] public Formateur CreatedBy { get; set; } = null!;)
// jamais envoyée par le frontend (seul CreatedById, un simple int,
// devrait l'être, et même celui-là est en fait déterminé par le
// serveur depuis le token JWT) -> 400 "CreatedBy" avant même que
// ResourcesController.CreateResource() ait pu exécuter
// resource.CreatedById = currentUserId;
//
// Le DTO résout ce souci : il ne contient que Name/Url/Type, le contrôleur se
// charge ensuite de mapper le DTO vers l'entité EF Core et de compléter
// CreatedById/CreatedAt lui-même.

using System.ComponentModel.DataAnnotations;

namespace PreparationApp.Backend.ModelsBD.Dtos;

public class ResourceDto
{
    [Required(ErrorMessage = "Le nom de la ressource est obligatoire.")]
    [StringLength(255, ErrorMessage = "Le nom ne peut pas dépasser 255 caractères.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'URL de la ressource est obligatoire.")]
    [StringLength(1000, ErrorMessage = "L'URL ne peut pas dépasser 1000 caractères.")]
    public string Url { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Le type ne peut pas dépasser 50 caractères.")]
    public string Type { get; set; } = string.Empty;
}
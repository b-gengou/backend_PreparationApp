// DTO utilisé pour les requêtes  POST/PUT venant du frontend (PreparationsForm.tsx).
// Contrairement au modèle EF Core Preparation, il ne contient que les champs que
// l'utilisateur saisit réellement dans le formulaire : Subject, StartDate,
// EndDate, Status, FormateurId.
//
// !!! DTO séparé du modèle EF Core Preparation 

// Même cause que pour PreparationReportDto et ResourceDto  : avec [ApiController] ET
// <Nullable>enable</Nullable> dans le .csproj, ASP.NET Core déduit
// automatiquement un [Required] implicite sur tout type référence
// non-nullable, puis valide le JSON reçu contre le type du paramètre de
// l'action avant d'exécuter le code du contrôleur.
//
// Si on utilise directement Preparation comme paramètre :
// a) Formateur est une propriété de navigation non-nullable
// ([ForeignKey("FormateurId")] public Formateur Formateur { get; set; } = null!;)
//  jamais envoyée par le frontend (seul FormateurId, un simple int,
// l'est) -> 400 "Formateur".
// b) CreatedBy, même souci : propriété de navigation non-nullable,
//  jamais envoyée par le frontend (le créateur est déterminé par le
// serveur depuis le token JWT, voir PreparationsController.cs) -> 400
// "CreatedBy".
//
// Le DTO résout ce souci : il ne contient que les champs du formulaire, le
// contrôleur se charge ensuite de mapper le DTO vers l'entité EF Core et
// de compléter CreatedById/CreatedAt lui-même.

using System.ComponentModel.DataAnnotations;

namespace PreparationApp.Backend.ModelsBD.Dtos;

public class PreparationDto
{
    [Required(ErrorMessage = "Le titre de la préparation est obligatoire.")]
    [StringLength(255, ErrorMessage = "Le titre ne peut pas dépasser 255 caractères.")]
    public string Subject { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La description ne peut pas dépasser 1000 caractères.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le formateur est obligatoire pour une préparation.")]
    public int FormateurId { get; set; }

    [Required(ErrorMessage = "La date de début est obligatoire.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "La date de fin est obligatoire.")]
    public DateTime EndDate { get; set; }

    [StringLength(50, ErrorMessage = "Le statut ne peut pas dépasser 50 caractères.")]
    public string Status { get; set; } = "Upcoming";

    [StringLength(255, ErrorMessage = "L'ID de l'événement Google ne peut pas dépasser 255 caractères.")]
    public string GoogleEventId { get; set; } = string.Empty;
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant un formateur dans l'application.
public class Formateur
{
    // Identifiant unique du formateur (clef primaire).
    [Key]
    public int Id { get; set; }

    // Nom du formateur.
    [Required(ErrorMessage = "Le nom du formateur est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
    public string Name { get; set; } = string.Empty;

    // Adresse email du formateur.
    [Required(ErrorMessage = "L'email du formateur est obligatoire.")]
    [StringLength(255, ErrorMessage = "L'email ne peut pas dépasser 255 caractères.")]
    [EmailAddress(ErrorMessage = "L'email doit être une adresse valide.")]
    public string Email { get; set; } = string.Empty;

    // Identifiant du calendrier Google associé au formateur.
    [StringLength(255, ErrorMessage = "L'ID du calendrier Google ne peut pas dépasser 255 caractères.")]
    public string GoogleCalendarId { get; set; } = string.Empty;

    // Liste des préparations associées à ce formateur (relation 1:N).
    public ICollection<Preparation> Preparations { get; set; } = new List<Preparation>();
}
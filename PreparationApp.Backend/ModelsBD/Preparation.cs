using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PreparationApp.Backend.ModelsBD;

// Modèle représentant une préparation (session de formation) dans l'application.
public class Preparation
{
    // Identifiant unique de la préparation (clef primaire).
    [Key]
    public int Id { get; set; }

    // Identifiant du formateur associé à cette préparation (clef étrangère).
    [Required(ErrorMessage = "Le formateur est obligatoire pour une préparation.")]
    public int FormateurId { get; set; }

    // Objet Formateur associé à cette préparation (propriété de navigation).
    [ForeignKey("FormateurId")]
    public Formateur Formateur { get; set; } = null!;

    // Sujet ou titre de la préparation.
    [Required(ErrorMessage = "Le sujet de la préparation est obligatoire.")]
    [StringLength(255, ErrorMessage = "Le sujet ne peut pas dépasser 255 caractères.")]
    public string Subject { get; set; } = string.Empty;

    // Date et heure de début de la préparation.
    [Required(ErrorMessage = "La date de début est obligatoire.")]
    public DateTime StartDate { get; set; }

    // Date et heure de fin de la préparation.
    [Required(ErrorMessage = "La date de fin est obligatoire.")]
    public DateTime EndDate { get; set; }

    // Identifiant de l'événement dans Google Calendar.
    [StringLength(255, ErrorMessage = "L'ID de l'événement Google ne peut pas dépasser 255 caractères.")]
    public string GoogleEventId { get; set; } = string.Empty;

    // Statut de la préparation (ex: "À venir", "En cours", "Terminé").
    [StringLength(50, ErrorMessage = "Le statut ne peut pas dépasser 50 caractères.")]
    public string Status { get; set; } = string.Empty;

    // Liste des comptes-rendus associés à cette préparation (relation 1:N).
    public ICollection<PreparationReport> Reports { get; set; } = new List<PreparationReport>();

    // Liste des ressources associées à cette préparation (relation N:N).
    public ICollection<PreparationResource> PreparationResources { get; set; } = new List<PreparationResource>();
}
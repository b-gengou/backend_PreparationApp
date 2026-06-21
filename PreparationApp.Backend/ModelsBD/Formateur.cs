// Modèle représentant un formateur dans PreparationApp.
// Ce modèle inclut les propriétés nécessaires pour l'authentification et la
// gestion des rôles.
// "Role" (string), permet d'ajouter d'autres rôles à l'avenir
// (ex.: "3" pour un futur rôle "responsable pédagogique").
// Convention actuelle : "1" = administrateur, "2" = formateur simple.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PreparationApp.Backend.ModelsBD;

public class Formateur
{
    // Id. unique du formateur (clef primaire).
    [Key]
    public int Id { get; set; }

    // Nom complet du formateur.
    [Required(ErrorMessage = "Le nom du formateur est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
    public string Name { get; set; } = string.Empty;

    // Adresse courriel du formateur, utilisée pour la connexion.
    [Required(ErrorMessage = "L'email du formateur est obligatoire.")]
    [StringLength(255, ErrorMessage = "L'email ne peut pas dépasser 255 caractères.")]
    [EmailAddress(ErrorMessage = "L'email doit être une adresse valide.")]
    public string Email { get; set; } = string.Empty;

    // Mot de passe du formateur (haché en production).
    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir entre 6 et 255 caractères.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    // Id. du calendrier Google associé au formateur.
    [StringLength(255, ErrorMessage = "L'ID du calendrier Google ne peut pas dépasser 255 caractères.")]
    public string GoogleCalendarId { get; set; } = string.Empty;

    // Rôle du formateur dans PreparationApp, sous forme de texte.
    // Convention : "1" = administrateur (droits étendus, peut tout
    // modifier/supprimer), "2" = formateur simple (peut seulement
    // modifier/supprimer ce qu'il a lui-même créé).
    // C'est cette valeur qui sera inscrite dans le token JWT au moment
    // du login, puis relue par le backend à chaque requête protégée.


    [Required(ErrorMessage = "Le rôle du formateur est obligatoire.")]
    [StringLength(10, ErrorMessage = "Le rôle ne peut pas dépasser 10 caractères.")]
    public string Role { get; set; } = "2"; // Par défaut : formateur simple.

    // Liste des préparations associées à ce formateur (relation 1:N).
    public ICollection<Preparation> Preparations { get; set; } = new List<Preparation>();

    // Liste des préparations créées par ce formateur (relation 1:N).
    [InverseProperty("CreatedBy")]
    public ICollection<Preparation> CreatedPreparations { get; set; } = new List<Preparation>();

    // Ignore les boucles de référence lors de la sérialisation JSON.
    [JsonIgnore]
    [NotMapped]
    public Formateur FormateurReference => this;
}
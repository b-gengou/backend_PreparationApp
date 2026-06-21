// Contexte de la BD pour PreparationApp.
// Définit les tables (DbSet) et les relations entre les entités (OnModelCreating).

using Microsoft.EntityFrameworkCore;
using PreparationApp.Backend.ModelsBD;

namespace PreparationApp.Backend.ModelsBD;

public class AppDbContext : DbContext
{
    // Constructeur du contexte de la BD.
    // Reçoit les options de config. (ex. : chaîne de connexion).
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet pour les formateurs.
    // Permet d'accéder à la table Formateurs en BD.
    public DbSet<Formateur> Formateurs { get; set; }

    // DbSet pour les préparations.
    // Permet d'accéder à la table Preparations en BD.
    public DbSet<Preparation> Preparations { get; set; }

    // DbSet pour les comptes-rendus des préparations.
    // Permet d'accéder à la table Reports en BD.
    public DbSet<PreparationReport> Reports { get; set; }

    // DbSet pour les ressources.
    // Permet d'accéder à la table Resources en BD.
    public DbSet<Resource> Resources { get; set; }

    // DbSet pour la table de jointure entre Preparation et Resource.
    // Permet de gérer la relation N:N entre préparations et ressources.
    public DbSet<PreparationResource> PreparationResources { get; set; }

    // Config. des relations et contraintes de la BD.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relation 1:N entre Formateur et Preparation.
        // Un formateur peut avoir plusieurs préparations (celles où il est le formateur principal).
        modelBuilder.Entity<Preparation>()
            .HasOne(p => p.Formateur)
            .WithMany(f => f.Preparations)
            .HasForeignKey(p => p.FormateurId)
            .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression en cascade

        // Relation 1:N entre Formateur et Preparation (pour CreatedBy).
        // Un formateur peut créer plusieurs préparations.
        modelBuilder.Entity<Preparation>()
            .HasOne(p => p.CreatedBy)
            .WithMany(f => f.CreatedPreparations)
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression en cascade

        // Relation 1:N entre Preparation et PreparationReport.
        // Une préparation peut avoir plusieurs comptes-rendus.
        modelBuilder.Entity<PreparationReport>()
            .HasOne(pr => pr.Preparation)
            .WithMany(p => p.Reports)
            .HasForeignKey(pr => pr.PreparationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relation 1:N entre Formateur et Resource (CreatedBy).
        // Un formateur peut créer plusieurs ressources (PDF, liens, vidéos...).
        // Utilisé pour appliquer la règle de droits : seul le créateur d'une
        // ressource ou un admin peut la modifier/supprimer.

        modelBuilder.Entity<Resource>()
            .HasOne(r => r.CreatedBy)
            .WithMany() // Un Formateur n'a pas besoin d'une liste "MyResources" dédiée ici.
            .HasForeignKey(r => r.CreatedById)
            .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression en cascade

        // Relation N:N entre Preparation et Resource (via PreparationResource).
        // Une préparation peut avoir plusieurs ressources, et une ressource peut être liée à plusieurs préparations.
        modelBuilder.Entity<PreparationResource>()
            .HasKey(pr => new { pr.PreparationId, pr.ResourceId });

        // Contrainte de validation : EndDate doit être après StartDate pour une préparation.
        modelBuilder.Entity<Preparation>()
            .ToTable(t => t.HasCheckConstraint("CK_Preparation_EndDate_After_StartDate", @"
                [EndDate] > [StartDate]
            "));

        // Index pour améliorer les performances des jointures.
        modelBuilder.Entity<Preparation>()
            .HasIndex(p => p.FormateurId);

        // Index pour la relation CreatedBy.
        modelBuilder.Entity<Preparation>()
            .HasIndex(p => p.CreatedById);

        modelBuilder.Entity<PreparationReport>()
            .HasIndex(pr => pr.PreparationId);

        modelBuilder.Entity<PreparationResource>()
            .HasIndex(pr => new { pr.PreparationId, pr.ResourceId });

        // Index pour accélérer les recherches de ressources par créateur.
        modelBuilder.Entity<Resource>()
            .HasIndex(r => r.CreatedById);
    }
}
//Contexte de la BD. définit les tables et les relations entre elles
using Microsoft.EntityFrameworkCore;
using PreparationApp.Backend.ModelsBD;


namespace PreparationApp.Backend.ModelsBD;

// Contexte de la BD  pour l'application PreparationApp.
// Cette classe hérite de DbContext et définit les tables et relations de la BD.
public class AppDbContext : DbContext
{
    // Constructeur du contexte de la base de données.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet pour chaque table de la base de données
    public DbSet<Formateur> Formateurs { get; set; }
    public DbSet<Preparation> Preparations { get; set; }
    public DbSet<PreparationReport> PreparationReports { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<PreparationResource> PreparationResources { get; set; }

    // --> Config. des relations entre les tables
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relation 1:N entre Formateur et Preparation
        // Un formateur peut avoir plusieurs préparations
        modelBuilder.Entity<Preparation>()
            .HasOne(p => p.Formateur)
            .WithMany(f => f.Preparations)
            .HasForeignKey(p => p.FormateurId)
            .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression en cascade

        // Relation 1:N entre Preparation et PreparationReport
        // Une préparation peut avoir plusieurs comptes-rendus
        modelBuilder.Entity<PreparationReport>()
            .HasOne(pr => pr.Preparation)
            .WithMany(p => p.Reports)
            .HasForeignKey(pr => pr.PreparationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relation N:N entre Preparation et Resource (via PreparationResource)
        // Une préparation peut avoir plusieurs ressources, et une ressource peut être liée à plusieurs préparations
        modelBuilder.Entity<PreparationResource>()
            .HasKey(pr => new { pr.PreparationId, pr.ResourceId });
    }
}
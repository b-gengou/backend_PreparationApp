// Tests automatisés du contrôleur PreparationsController.
// Exigé par le planning du cahier des charges (phase "Tests").
// Chaque test suit le schéma "Arrange / Act / Assert" :
// a) Arrange : prépare les données nécessaires au test.
// b) Act     : exécute l'action qu'on veut tester.
// c) Assert  : vérifie que le résultat correspond à ce qui est attendu.

using Microsoft.EntityFrameworkCore;
using PreparationApp.Backend.Controllers;
using PreparationApp.Backend.ModelsBD;
using Xunit;

namespace PreparationApp.Backend.Tests;

public class PreparationsControllerTests
{
    // Méthode utilitaire privée : crée un contexte de BD
    // "en mémoire", complètement vide et indépendant à chaque appel
    // (grâce à Guid.NewGuid() qui génère un nom unique à chaque fois).
    // Cela évite que les tests interfèrent entre eux ou avec la vraie base.

    private AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    // [Fact] indique à xUnit "ceci est un test à exécuter automatiquement".
    // Ce test vérifie que GetPreparations() renvoie bien une liste vide
    // (et non une erreur) quand la BD ne contient aucune préparation.

    [Fact]
    public async Task GetPreparations_ReturnsEmptyList_WhenNoData()
    {
        // Arrange : on crée une base vide et le contrôleur à tester.
        // Le deuxième paramètre (GoogleCalendarService) est mis à "null!"
        // car cette méthode précise n'en a pas besoin pour ce test.
        var context = GetInMemoryContext();
        var controller = new PreparationsController(context, null!);

        // Act : appelle la méthode que l'on veut tester.
        var result = await controller.GetPreparations();

        // Assert : vérifie que le résultat est bien un "200 OK"...
        var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);

        // et que la liste à l'intérieur est bien vide.
        var preparations = Assert.IsAssignableFrom<IEnumerable<Preparation>>(okResult.Value);
        Assert.Empty(preparations);
    }

    // Ce test vérifie que GetPreparations() renvoie bien une préparation
    // quand on en a ajouté une à la base au préalable.

    [Fact]
    public async Task GetPreparations_ReturnsOnePreparation_WhenOneExists()
    {
        // Arrange : prépare une base contenant un formateur et une préparation liée.
        var context = GetInMemoryContext();

        var formateur = new Formateur
        {
            Name = "Charles Martel",
            Email = "charles@cognitic.be",
            Password = "motdepasse123",
            Role = "2",
            GoogleCalendarId = "primary"
        };
        context.Formateurs.Add(formateur);
        await context.SaveChangesAsync();

        var preparation = new Preparation
        {
            Subject = "Formation SQL Avancé",
            StartDate = new DateTime(2026, 6, 10, 9, 0, 0),
            EndDate = new DateTime(2026, 6, 10, 12, 0, 0),
            FormateurId = formateur.Id,
            CreatedById = formateur.Id,
            Status = "Upcoming"
        };
        context.Preparations.Add(preparation);
        await context.SaveChangesAsync();

        var controller = new PreparationsController(context, null!);

        // Act : appelle la méthode à tester.
        var result = await controller.GetPreparations();

        // Assert : vérifie qu'on récupère bien exactement une préparation,
        // et que son sujet correspond à celui que l'on a créé.
        var okResult = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
        var preparations = Assert.IsAssignableFrom<IEnumerable<Preparation>>(okResult.Value);
        Assert.Single(preparations);
        Assert.Equal("Formation SQL Avancé", preparations.First().Subject);
    }
}

// Tests automatisés du contrôleur ResourcesController, en particulier
// la règle de droits : seul le créateur d'une ressource ou un admin
// peut la modifier ou la supprimer.

using Microsoft.EntityFrameworkCore;
using PreparationApp.Backend.Controllers;
using PreparationApp.Backend.ModelsBD;
using System.Security.Claims;
using Xunit;

namespace PreparationApp.Backend.Tests;

public class ResourcesControllerTests
{
    private AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    
    // Méthode utilitaire : simule un utilisateur connecté, en construisant
    // un faux "User" avec les claims (identifiant + rôle) qu'on veut tester,
    // exactement comme le ferait ASP.NET Core après une vraie authentification JWT.
    
    private void SetCurrentUser(ResourcesController controller, int userId, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claimsPrincipal }
        };
    }

    
    // Ce test vérifie qu'un formateur simple (rôle "2") qui n'a pas créé
    // une ressource reçoit bien une erreur 403 (Forbidden) s'il essaie
    // de la supprimer.
    
    [Fact]
    public async Task DeleteResource_Returns403_WhenNotCreatorAndNotAdmin()
    {
        // Arrange : création de deux formateurs, et une ressource créée par le premier.
        var context = GetInMemoryContext();

        var createur = new Formateur { Name = "Créateur", Email = "createur@test.be", Password = "x", Role = "2" };
        var autreFormateur = new Formateur { Name = "Autre", Email = "autre@test.be", Password = "x", Role = "2" };
        context.Formateurs.AddRange(createur, autreFormateur);
        await context.SaveChangesAsync();

        var resource = new Resource
        {
            Name = "PDF de test",
            Url = "https://exemple.com/test.pdf",
            Type = "PDF",
            CreatedById = createur.Id
        };
        context.Resources.Add(resource);
        await context.SaveChangesAsync();

        var controller = new ResourcesController(context);

        // Simulation que c'est "autreFormateur" (pas le créateur, et pas admin) qui est connecté.
        SetCurrentUser(controller, autreFormateur.Id, "2");

        // Act : essaie de supprimer la ressource créée par quelqu'un d'autre.
        var result = await controller.DeleteResource(resource.Id);

        // Assert : doit recevoir une interdiction (403), pas une suppression réussie.
        Assert.IsType<Microsoft.AspNetCore.Mvc.ForbidResult>(result);
    }

    
    // Ce test vérifie qu'un admin (rôle "1") peut supprimer une ressource
    // même s'il n'en est pas le créateur.
    
    [Fact]
    public async Task DeleteResource_Succeeds_WhenUserIsAdmin()
    {
        // Arrange
        var context = GetInMemoryContext();

        var createur = new Formateur { Name = "Créateur", Email = "createur2@test.be", Password = "x", Role = "2" };
        var admin = new Formateur { Name = "Admin", Email = "admin2@test.be", Password = "x", Role = "1" };
        context.Formateurs.AddRange(createur, admin);
        await context.SaveChangesAsync();

        var resource = new Resource
        {
            Name = "PDF de test 2",
            Url = "https://exemple.com/test2.pdf",
            Type = "PDF",
            CreatedById = createur.Id
        };
        context.Resources.Add(resource);
        await context.SaveChangesAsync();

        var controller = new ResourcesController(context);

        // Simule que c'est l'admin qui est connecté.
        SetCurrentUser(controller, admin.Id, "1");

        // Act
        var result = await controller.DeleteResource(resource.Id);

        // Assert : la suppression doit réussir (204 No Content), pas être bloquée.
        Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
    }
}
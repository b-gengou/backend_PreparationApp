//Service pour interagir avec l'API Google Calendar.

using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

namespace PreparationApp.Backend.Services;

// Service pour interagir avec l'API Google Calendar.
// Ce service permet de récupérer les événements liés aux préparations depuis un calendrier Google.
public class GoogleCalendarService
{
    private readonly IConfiguration _configuration;

    // Constructeur du service.
    // L'injection de dépendance fournit automatiquement la configuration.
    public GoogleCalendarService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Récupère les événements de préparation depuis Google Calendar.
    public async Task<List<GoogleCalendarEvent>> GetPreparationEventsAsync()
    {
        // --> Récupération et validation des clefs Google
        var credentialsJson = _configuration["Google:Credentials"];
        if (string.IsNullOrEmpty(credentialsJson))
        {
            throw new InvalidOperationException(
                "La configuration est manquante ou invalide. " +
                "Vérifiez que la clef 'Google:Credentials' existe dans appsettings.json " +
                "et contient un JSON de credentials valide.");
        }

        // --> Création des credentials Google
        // Désactive l'avertissement d'obsolescence pour FromJson
        #pragma warning disable CS0618
        GoogleCredential credential;
        try
        {
            credential = GoogleCredential.FromJson(credentialsJson)
                .CreateScoped(CalendarService.Scope.CalendarReadonly);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Erreur lors de la création des credentials Google : {ex.Message}. " +
                "Vérifiez que le JSON de credentials est valide.");
        }
        finally
        {
            // Réactive les warnings d'obsolescence
            #pragma warning restore CS0618
        }

        // --> Initialisation du service Google Calendar
        var service = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "PreparationApp"
        });

        // --> Configuration de la requête
        var request = service.Events.List("primary");
        request.Q = "préparation"; // Filtre les événements contenant "préparation"
        request.TimeMinDateTimeOffset = DateTimeOffset.Now; // À partir de maintenant
        request.MaxResults = 50; // Limite à 50 résultats
        request.SingleEvents = true; // Événements uniques (pas les récurrences)
        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime; // Tri par date

        // --> Exécution de la requête et traitement de la réponse
        try
        {
            var response = await request.ExecuteAsync();
            return response.Items.Select(e => new GoogleCalendarEvent
            {
                Id = e.Id,
                Summary = e.Summary,
                // Gère les deux formats de date (DateTime et Date)
                Start = e.Start.DateTimeDateTimeOffset?.DateTime ??
                       (e.Start.Date != null ? DateTime.Parse(e.Start.Date) : DateTime.MinValue),
                End = e.End.DateTimeDateTimeOffset?.DateTime ??
                      (e.End.Date != null ? DateTime.Parse(e.End.Date) : DateTime.MinValue),
                Description = e.Description ?? string.Empty
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Erreur lors de la récupération des événements Google Calendar : {ex.Message}");
        }
    }
}

// Classe pour stocker les informations d'un événement Google Calendar.
public class GoogleCalendarEvent
{
    // ID unique de l'événement dans Google Calendar.
    public string Id { get; set; } = string.Empty;

    // Titre ou résumé de l'événement.
    public string Summary { get; set; } = string.Empty;

    // Date et heure de début de l'événement.
    public DateTime Start { get; set; }

    // Date et heure de fin de l'événement.
    public DateTime End { get; set; }

    // Description détaillée de l'événement.
    public string Description { get; set; } = string.Empty;
}
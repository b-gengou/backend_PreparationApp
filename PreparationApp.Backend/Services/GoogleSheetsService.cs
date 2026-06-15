using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using System.Collections.Generic;

namespace PreparationApp.Backend.Services;

// Service pour interagir avec l'API Google Sheets.
// Ce service permet de lire et écrire des données dans des feuilles Google Sheets.
public class GoogleSheetsService
{
    private readonly IConfiguration _configuration;

    // Constructeur du service.
    // L'injection de dépendance fournit automatiquement la configuration.
    public GoogleSheetsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Récupère les données d'une feuille Google Sheets.
    public async Task<List<IList<object>>> GetSheetDataAsync(string spreadsheetId, string range)
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
        #pragma warning disable CS0618
        GoogleCredential credential;
        try
        {
            credential = GoogleCredential.FromJson(credentialsJson)
                .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Erreur lors de la création des credentials Google : {ex.Message}. " +
                "Vérifiez que le JSON de credentials est valide.");
        }
        finally
        {
            #pragma warning restore CS0618
        }

        // --> Initialisation du service Google Sheets
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "PreparationApp"
        });

        // --> Exécution de la requête
        try
        {
            var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = await request.ExecuteAsync();
            return response.Values?.ToList() ?? new List<IList<object>>();
        }
        catch (Google.GoogleApiException ex)
        {
            throw new InvalidOperationException(
                $"Erreur Google Sheets : {ex.Message}. " +
                "Vérifiez que l'ID de la feuille et la plage sont valides.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Erreur lors de la récupération des données : {ex.Message}");
        }
    }

    // Ajoute des données à une feuille Google Sheets.
    public async Task AppendSheetDataAsync(string spreadsheetId, string range, List<IList<object>> values)
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
        #pragma warning disable CS0618
        GoogleCredential credential;
        try
        {
            credential = GoogleCredential.FromJson(credentialsJson)
                .CreateScoped(SheetsService.Scope.Spreadsheets);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Erreur lors de la création des credentials Google : {ex.Message}. " +
                "Vérifiez que le JSON de credentials est valide.");
        }
        finally
        {
            #pragma warning restore CS0618
        }

        // --> Initialisation du service Google Sheets
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "PreparationApp"
        });

        // --> Exécution de la requête
        try
        {
            var valueRange = new ValueRange { Values = values };
            var appendRequest = service.Spreadsheets.Values.Append(
                valueRange,
                spreadsheetId,
                range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            await appendRequest.ExecuteAsync();
        }
        catch (Google.GoogleApiException ex)
        {
            throw new InvalidOperationException(
                $"Erreur Google Sheets : {ex.Message}. " +
                "Vérifiez que l'ID de la feuille et la plage sont valides.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Erreur lors de l'ajout des données : {ex.Message}");
        }
    }
}
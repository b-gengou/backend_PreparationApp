
// Contrôleur pour gérer les requêtes (interactions) liées à Google Sheets (lecture/écriture).
// Gestion des interactions avec Google Sheets .

using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Mvc;
using PreparationApp.Backend.Services;
//using PreparationApp.Backend.ModelsBD;

namespace PreparationApp.Backend.Controllers;

// Contrôleur pour gérer les requêtes HTTP liées à Google Sheets.
[ApiController]
[Route("api/[controller]")]
public class SheetsController : ControllerBase
{
    private readonly GoogleSheetsService _sheetsService;

    // Constructeur du contrôleur.
    // L'injection de dépendance fournit automatiquement le service Google Sheets.
    public SheetsController(GoogleSheetsService sheetsService)
    {
        _sheetsService = sheetsService;
    }

    // Récupère les données d'une feuille Google Sheets.
    // GET: api/sheets/{spreadsheetId}/{range}
    [HttpGet("{spreadsheetId}/{range}")]
    public async Task<IActionResult> GetSheetData(string spreadsheetId, string range)
    {
        try
        {
            // Vérifie que les paramètres ne sont pas null ou vides
            if (string.IsNullOrEmpty(spreadsheetId) || string.IsNullOrEmpty(range))
            {
                return BadRequest(new { Error = "Les paramètres spreadsheetId et range sont requis." });
            }

            var data = await _sheetsService.GetSheetDataAsync(spreadsheetId, range);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de la récupération des données : {ex.Message}" });
        }
    }

    // Ajoute des données à une feuille Google Sheets.
    // POST: api/sheets/{spreadsheetId}/{range}
    [HttpPost("{spreadsheetId}/{range}")]
    public async Task<IActionResult> AppendSheetData(
        string spreadsheetId,
        string range,
        [FromBody] List<IList<object>> values)
    {
        try
        {
            // Vérifie que les paramètres ne sont pas null ou vides
            if (string.IsNullOrEmpty(spreadsheetId) || string.IsNullOrEmpty(range))
            {
                return BadRequest(new { Error = "Les paramètres spreadsheetId et range sont requis." });
            }

            // Vérifie que les données à ajouter ne sont pas null ou vides
            if (values == null || values.Count == 0)
            {
                return BadRequest(new { Error = "Les données à ajouter ne peuvent pas être null ou vides." });
            }

            await _sheetsService.AppendSheetDataAsync(spreadsheetId, range, values);
            return Ok(new { Message = "Données ajoutées à Google Sheets avec succès." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Erreur lors de l'ajout des données : {ex.Message}" });
        }
    }
}
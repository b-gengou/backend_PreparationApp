# Backend - PreparationApp

---

## Description
Ce dossier contient le **backend** de l'application **PreparationApp**, développé en **ASP.NET Core 10.0**.
Il gère :
- Les **formateurs** et leurs disponibilités.
- La **synchronisation avec Google Calendar**.
- L’**export/import depuis Google Sheets**.
- La **BD de SQL Server**.

---

---

## Technologies
   Composant          | Technologie       | Version  |
 |--------------------|-------------------|----------|
 | Framework          | ASP.NET Core      | 10.0     |
 | Base de données    | SQL Server        | 2022     |
 | ORM                | Entity Framework  | Core 10.0|
 | Authentification   | Google OAuth2 + JWT| -        |
 | API Externes       | Google Calendar API, Google Sheets API | v3, v4 |

---

---

## **Structure du Dossier `backend/`**

backend/
│
├── 📁 Controllers/
│   ├── FormateursController.cs       # Gère les requêtes HTTP pour les formateurs
│   ├── PreparationsController.cs     # Gère les requêtes HTTP pour les préparations
│   └── PreparationReportsController.cs # Gère les rapports de préparation
│   └── SheetsController
│
├── 📁 ModelsBD/
│   ├── Formateur.cs                   # Modèle EF Core pour les formateurs
│   ├── Preparation.cs                 # Modèle EF Core pour les préparations
│   ├── PreparationReport.cs           # Modèle EF Core pour les rapports
│   ├── PreparationResource.cs         # Modèle pour les ressources liées aux préparations
│   ├── Resource.cs                    # Modèle pour les ressources
│   └── AppDbContext.cs                # Contexte EF Core pour la BD
│
├── 📁 Services/
│   ├── GoogleCalendarService.cs       # Service pour interagir avec Google Calendar
│   └── GoogleSheetsService.cs         # Service pour interagir avec Google Sheets
│
├── 📁 Properties/
│   ├── launchSettings.json            # Configuration des profils de lancement (ports, environnement)
│   └── appsettings.Development.json    # Configuration spécifique au développement
│
├── Program.cs                   # Point d'entrée de l'application (configuration des services, middleware)
├── appsettings.json             # Configuration générale (chaînes de connexion, clés API, etc.)
└── BACKEND.md                    # Ce fichier (documentation)
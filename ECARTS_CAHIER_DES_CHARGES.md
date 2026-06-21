# Écarts connus par rapport au cahier des charges

_Dernière mise à jour : 20/06/2026_


## 1. Authentification (cahier des charges, sections 4.2 et 10)

**Exigence du cahier des charges** : "Authentification : OAuth 2.0 obligatoire
(pas de mot de passe local)".

**État actuel** : `AuthController.cs` implémente un système classique
email + mot de passe, avec génération d'un token JWT applicatif. Le schéma
d'authentification Google (`AddGoogle(...)`) est configuré dans `Program.cs`
mais n'est pas encore branché à un flux d'authentification réel.

**Raison de l'écart** : priorité donnée, dans le temps disponible, à la correction du modèle de données
(`PreparationReport`) et à l'ajout du catalogue des supports (`ResourcesController`).

**Plan de correction prévu** :
1. Côté frontend (React) : ajouter un bouton "Se connecter avec Google" qui
   utilise la librairie `@react-oauth/google` pour récupérer un `id_token`
   Google après connexion de l'utilisateur.
2. Côté backend : créer un nouvel endpoint `POST /api/auth/google` qui reçoit
   ce `id_token`, le vérifie avec
   `Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(...)`, retrouve ou
   crée le `Formateur` correspondant à l'email Google, puis génère le JWT
   applicatif comme c'est déjà fait actuellement.
3. Retirer progressivement les endpoints `login`/`register` par mot de passe.

**Échéance proposée** : à discuter lors du rendez-vous du 23/06/2026.

## 2. Google Sheets API (cahier des charges, section 4.1)

**Exigence du cahier des charges** : "Le catalogue des supports va remplacer
Google Sheets. L'API Sheets ne sera plus nécessaire."

**État actuel** : `GoogleSheetsService.cs` et `SheetsController.cs` sont
toujours présents et fonctionnels dans le code, bien que `ResourcesController.cs`
remplisse maintenant ce rôle de catalogue des supports.

**Raison de l'écart** : code conservé par prudence en cas de besoin de
migration de données existantes depuis l'ancien Google Sheet.

**Plan de correction prévu** : à valider — soit suppression
complète, soit conservation assumée comme outil de migration ponctuel
(non utilisé par le frontend final).
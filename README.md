# 📋 Architecture du Projet Monetrixa.Challenge.Api

## Vue d'ensemble

**Type d'architecture** : Architecture en Couches (Layered Architecture)  
**Framework** : ASP.NET Core 10.0  
**Base de données** : SQL Server  
**Authentification** : JWT Bearer  
**ORM** : Entity Framework Core 10.0.6

---

## 🏗️ Structure en Couches

```
┌──────────────────────────────┐
│  Couche Présentation         │  ← API Controllers
│ (Monetrixa.Challenge.Api)    │
└──────────────┬───────────────┘
               │
┌──────────────┴───────────────┐
│  Couche Application          │  ← DTOs (simples conteneurs)
│ (Monetrixa.Challenge.        │    + Interfaces de services
│  Application)                │
└──────────────┬───────────────┘
               │
┌──────────────┴───────────────────────────────┐
│  Couche Métier/Implémentation                │
├───────────────────────────────────────────────┤
│ Monetrixa.Challenge.Infrastructure           │
│ - Services (AuthService, ChallengeService)   │
│ - EF Core DbContext                          │
│ - Configurations                             │
└──────────────┬───────────────────────────────┘
               │
┌──────────────┴───────────────┐
│  Couche Domaine              │  ← Entities (anémiques)
│ (Monetrixa.Challenge.        │  + Énumérations
│  Domain)                     │
└──────────────────────────────┘
```

---

## 📁 Structure des Projets

### 1️⃣ **Monetrixa.Challenge.Api**
**Rôle** : Couche Présentation (Point d'entrée)

**Contient** :
- `AuthController` : Gestion de l'authentification (Register, Login)
- `ChallengesController` : Gestion des défis (Join, GetCurrent, ValidateDay, etc.)
- `TestAuthController` : Contrôleur de test
- `Program.cs` : Configuration et injection de dépendances

**Dépendances** :
- Monetrixa.Challenge.Application
- Monetrixa.Challenge.Infrastructure

**Packages NuGet** :
- Microsoft.AspNetCore.Authentication.JwtBearer (10.0.6)
- Microsoft.AspNetCore.OpenApi (10.0.6)
- System.IdentityModel.Tokens.Jwt (8.17.0)
- Scalar.AspNetCore (2.14.1)
- Microsoft.EntityFrameworkCore.Design (10.0.6)
- Microsoft.EntityFrameworkCore.Tools (10.0.6)

---

### 2️⃣ **Monetrixa.Challenge.Application**
**Rôle** : Couche Application (Contrats et modèles)

**Contient** :

#### DTOs (Data Transfer Objects)
**Auth** :
- `RegisterRequest` : Requête d'inscription
- `LoginRequest` : Requête de connexion
- `AuthResponse` : Réponse d'authentification avec token
- `JwtSettings` : Configuration JWT

**Challenge** :
- `JoinChallengeRequest` : Rejoindre un défi
- `ChallengeSummaryResponse` : Résumé d'un défi
- `ValidateChallengeDayRequest` : Valider un jour du défi
- `CurrentChallengeDaysResponse` : Jours actuels du défi
- `DailyValidationResponse` : Réponse de validation quotidienne
- `ChallengeDayResponse` : Réponse pour un jour du défi
- `PublishedContentResponse` : Contenu publié
- `CreatePublishedContentRequest` : Créer un contenu publié
- `CommentResponse` : Commentaire
- `CreateCommentRequest` : Créer un commentaire

#### Interfaces de Services
- `IAuthService` : Contrat d'authentification
- `IJwtTokenService` : Contrat pour tokens JWT
- `IChallengeService` : Contrat de gestion des défis
- `ICurrentUserService` : Contrat pour récupérer l'utilisateur courant

**Dépendances** :
- Monetrixa.Challenge.Domain

---

### 3️⃣ **Monetrixa.Challenge.Domain**
**Rôle** : Couche Domaine (Cœur métier - Entities anémiques)

**Contient** :

#### Entities
- `User` : Utilisateur (Email, PasswordHash, Role, CreatedAtUtc)
- `Challenge` : Défi (Title, AccessCode, StartDateUtc, EndDateUtc)
- `ChallengeDay` : Jour du défi
- `UserChallenge` : Relation Many-to-Many (User ↔ Challenge)
- `DailyValidation` : Validation quotidienne
- `PublishedContent` : Contenu publié par les utilisateurs
- `Comment` : Commentaires sur les contenus
- `IdeaGeneration` : Génération d'idées
- `GeneratedIdea` : Idée générée
- `Notification` : Notifications
- `Resource` : Ressources associées aux défis

#### Enums
- `UserRole` : Rôles utilisateur (Participant, Moderator, Admin)
- `ValidationStatus` : État de validation (Pending, Validated, Rejected)
- `MoodType` : Types de mood
- `ResourceType` : Types de ressources
- `PlatformType` : Types de plateforme
- `NotificationType` : Types de notifications

**Dépendances** : Aucune (couche la plus indépendante)

---

### 4️⃣ **Monetrixa.Challenge.Infrastructure**
**Rôle** : Couche Implémentation (Logique métier et accès données)

**Contient** :

#### Services (Implémentations)
- `AuthService` : Logique d'authentification
- `JwtTokenService` : Génération et validation des tokens JWT
- `PasswordService` : Hachage (bcrypt) et vérification de mots de passe
- `ChallengeService` : Logique métier des défis
- `CurrentUserService` : Récupération de l'utilisateur depuis le contexte HTTP

#### Persistence
- `ChallengeDbContext` : DbContext EF Core principal
  - DbSet<User>
  - DbSet<Challenge>
  - DbSet<UserChallenge>
  - DbSet<ChallengeDay>
  - DbSet<DailyValidation>
  - DbSet<PublishedContent>
  - DbSet<Comment>
  - DbSet<Resource>
  - DbSet<IdeaGeneration>
  - DbSet<GeneratedIdea>
  - DbSet<Notification>

#### Configurations EF Core (Fluent API)
- `UserConfiguration`
- `ChallengeConfiguration`
- `ChallengeDayConfiguration`
- `UserChallengeConfiguration`
- `DailyValidationConfiguration`
- `PublishedContentConfiguration`
- `CommentConfiguration`
- `NotificationConfiguration`
- `ResourceConfiguration`

#### Migrations
- `20260415230402_InitialCreate` : Migration initiale

#### Seed
- `ChallengeSeeder` : Initialisation des données de test

**Dépendances** :
- Monetrixa.Challenge.Domain
- Monetrixa.Challenge.Application

**Packages NuGet** :
- Microsoft.EntityFrameworkCore.SqlServer (10.0.6)
- Microsoft.EntityFrameworkCore.Design (10.0.6)
- Microsoft.AspNetCore.App (FrameworkReference)

---

### 5️⃣ **Monetrixa.Challenge.Tests**
**Rôle** : Tests Unitaires

**Contient** :
- `AuthServiceTests` : Tests du service d'authentification
- `PasswordServiceTests` : Tests du service de mot de passe
- `ChallengeServiceTests` : Tests du service de défis
- `SqliteTestDbContextFactory` : Factory pour DbContext SQLite (tests)

**Dépendances** :
- Monetrixa.Challenge.Domain
- Monetrixa.Challenge.Application
- Monetrixa.Challenge.Infrastructure

---

## 🔄 Flux Principaux

### Flux d'Authentification

```
Register/Login Request (HTTP)
            ↓
    AuthController
            ↓
    IAuthService (Interface)
            ↓
    AuthService (Implementation)
            ↓
    PasswordService (Hash/Verify)
            ↓
    ChallengeDbContext (EF Core)
            ↓
    SQL Server Database
            ↓
    JwtTokenService (Generate Token)
            ↓
    AuthResponse (JWT Token)
```

### Flux de Gestion des Défis

```
API Request (avec JWT Bearer Token)
            ↓
    ChallengesController [Authorize]
            ↓
    ICurrentUserService (Extract User from JWT)
            ↓
    IChallengeService (Interface)
            ↓
    ChallengeService (Implementation)
            ↓
    ChallengeDbContext (EF Core)
            ↓
    SQL Server Database
            ↓
    Response DTO
```

---

## 🔧 Configuration DI (Program.cs)

```csharp
// Services d'authentification
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Services métier
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();

// Configuration authentification
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Database Context
builder.Services.AddDbContext<ChallengeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

---

## 📦 Dépendances NuGet

| Package | Version | Couche | Rôle |
|---------|---------|--------|------|
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.6 | Infrastructure | ORM pour SQL Server |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.6 | API | Authentification JWT |
| System.IdentityModel.Tokens.Jwt | 8.17.0 | Infrastructure | Génération/Validation JWT |
| Scalar.AspNetCore | 2.14.1 | API | Documentation API interactive |
| Microsoft.AspNetCore.OpenApi | 10.0.6 | API | Endpoints OpenAPI |
| Microsoft.EntityFrameworkCore.Design | 10.0.6 | Build | Outils EF Core |
| Microsoft.EntityFrameworkCore.Tools | 10.0.6 | Build | Migrations CLI |

---

## ⚙️ Patterns Utilisés

✅ **Layered Architecture** : Séparation en couches (Présentation, Application, Infrastructure, Domaine)  
✅ **Dependency Injection** : Via ASP.NET Core DI  
✅ **Service Layer** : Services pour la logique métier  
✅ **DTO Pattern** : Séparation entre modèles de données et DTOs  
✅ **Repository Pattern (implicite)** : Via EF Core DbContext  
✅ **JWT Authentication** : Tokens JWT pour l'authentification  
✅ **Entity Framework Core** : ORM pour l'accès aux données  

---

## ⚠️ Points Importants

### Ce qu'il N'y a PAS :
- ❌ **Clean Architecture** : Pas de Use Cases/Application Handlers
- ❌ **Domain-Driven Design** : Pas d'Aggregates, Value Objects, Domain Services
- ❌ **Domain Events** : Pas d'événements métier
- ❌ **Rich Entities** : Les entities sont anémiques (données uniquement)

### Caractéristiques réelles :
- ✅ Services procédurales (pas orientées objet métier)
- ✅ Entities anémiques (données + relations seulement)
- ✅ DTOs = simples conteneurs sans validation métier
- ✅ Logique métier dans les Services
- ✅ Architecture fonctionnelle classique

---

## 📊 Diagramme des Dépendances

```
Api (présentation)
    ↓
    ├─→ Application (contrats)
    └─→ Infrastructure (implémentation)
            ↓
            ├─→ Application
            └─→ Domain (entities)

Tests
    ↓
    ├─→ Domain
    ├─→ Application
    └─→ Infrastructure
```

---

## 🚀 Technologies Stack

| Aspect | Technologie |
|--------|-------------|
| **Framework** | ASP.NET Core 10.0 |
| **Langage** | C# 13+ (Nullable Reference Types, Implicit Usings) |
| **Base de données** | SQL Server |
| **ORM** | Entity Framework Core 10.0.6 |
| **Authentification** | JWT Bearer |
| **Documentation API** | OpenAPI + Scalar |
| **Tests** | NUnit / xUnit avec SQLite (in-memory) |

---

## 📝 Notes

- Cible **.NET 10**
- Nullable Reference Types **activés** (`<Nullable>enable</Nullable>`)
- Implicit Usings **activés** (`<ImplicitUsings>enable</ImplicitUsings>`)
- Seed des données en développement lors du démarrage
- JWT validation strict (Issuer, Audience, Lifetime, Signature)

---

**Dernière mise à jour** : 2025-04-15  
**Architecture** : Layered Architecture (Non-DDD, Non-Clean Architecture)

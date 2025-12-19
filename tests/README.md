# Tests Mo5 RAG Server

Ce document explique comment exécuter les tests du projet Mo5 RAG Server.

## Types de Tests

Le projet contient deux catégories de tests :

### 1. Tests Unitaires (48 tests)

Tests qui ne nécessitent pas de dépendances externes. Ils testent la logique métier, les services, et les utilitaires de manière isolée.

**Exécution :**
```bash
dotnet test --filter "Category!=RequiresPostgreSQL"
```

**Catégories de tests unitaires :**
- `SimpleTfIdfEmbeddingServiceTests` (13 tests) - Service d'embeddings TF-IDF
- `LocalEmbeddingServiceTests` (5 tests) - Service d'embeddings local
- `MarkdownTextChunkerTests` (6 tests) - Découpage de texte Markdown
- `TagDetectionServiceTests` (8 tests) - Détection automatique de tags
- `IngestionStatsServiceTests` (8 tests) - Statistiques d'ingestion
- `FileWatcherServiceTests` (8 tests) - Surveillance de fichiers

### 2. Tests d'Intégration (45 tests) - Nécessitent PostgreSQL

Tests qui nécessitent une base de données PostgreSQL avec l'extension pgvector. Ces tests sont **désactivés par défaut** car ils nécessitent un environnement complet.

**Tests marqués comme `RequiresPostgreSQL` :**
- `FullWorkflowIntegrationTests` (6 tests) - Workflow complet RAG
- `SemanticSearchIntegrationTests` (11 tests) - Recherche sémantique
- `SearchControllerTests` (8 tests) - API de recherche
- `SearchPerformanceTests` (8 tests) - Performance de recherche
- `RagDbContextTests` (3 tests) - Contexte de base de données
- `DocumentServiceSearchTests` (9 tests) - Service de documents

**Exécution (nécessite PostgreSQL avec pgvector) :**
```bash
# Démarrer PostgreSQL avec Docker Compose
cd deployment/local-dev
docker compose up -d postgres
cd ../..

# Exécuter tous les tests
dotnet test

# Ou exécuter uniquement les tests d'intégration
dotnet test --filter "Category=RequiresPostgreSQL"
```

## Résultats Actuels

✅ **48/48 tests unitaires passent** (100%)  
⚠️ **45 tests d'intégration nécessitent PostgreSQL** (désactivés par défaut)

## Commandes Utiles

```bash
# Exécuter tous les tests unitaires
dotnet test --filter "Category!=RequiresPostgreSQL"

# Exécuter tous les tests (unitaires + intégration)
dotnet test

# Exécuter un test spécifique
dotnet test --filter "FullyQualifiedName~SimpleTfIdfEmbeddingServiceTests"

# Exécuter avec verbosité détaillée
dotnet test --logger "console;verbosity=detailed"

# Exécuter avec rapport de couverture
dotnet test --collect:"XPlat Code Coverage"
```

## Corrections Apportées

### 1. LocalEmbeddingServiceTests
**Problème :** Moq ne peut pas mocker les méthodes d'extension comme `IConfiguration.GetValue<T>()`  
**Solution :** Utilisation de `ConfigurationBuilder` avec `AddInMemoryCollection()` au lieu de Mock

### 2. MarkdownTextChunkerTests
**Problème :** Test attendait un chunk "Introduction" qui n'existait pas dans le markdown  
**Solution :** Correction des assertions pour correspondre aux chunks réels générés

### 3. FileWatcherServiceTests
**Problème :** Le système de fichiers peut déclencher `Modified` au lieu de `Created` pour un nouveau fichier  
**Solution :** Accepter les deux types d'événements (`Created` ou `Modified`)

### 4. Tests d'Intégration
**Problème :** EF Core InMemory ne supporte pas le type `Vector` de pgvector  
**Solution :** Marquage des tests comme `RequiresPostgreSQL` pour les désactiver par défaut

## Architecture des Tests

```
tests/Mo5.RagServer.Tests/
├── Api/                    # Tests API (nécessitent PostgreSQL)
├── Infrastructure/
│   ├── Data/              # Tests DbContext (nécessitent PostgreSQL)
│   └── Services/          # Tests de services (unitaires)
├── Integration/           # Tests d'intégration (nécessitent PostgreSQL)
└── Performance/           # Tests de performance (nécessitent PostgreSQL)
```

## Notes Importantes

1. **Tests Unitaires** : Peuvent être exécutés partout (Raspberry Pi, CI/CD, développement local)
2. **Tests d'Intégration** : Nécessitent PostgreSQL 16+ avec extension pgvector
3. **Trait Category** : Utilisé pour filtrer les tests selon leurs dépendances
4. **CustomWebApplicationFactory** : Factory de test pour les tests d'intégration (non utilisée pour tests unitaires)

## CI/CD

Pour l'intégration continue, il est recommandé de :
1. Exécuter les tests unitaires sur chaque commit
2. Exécuter les tests d'intégration uniquement sur les branches principales avec un environnement PostgreSQL disponible

```yaml
# Exemple GitHub Actions
- name: Run Unit Tests
  run: dotnet test --filter "Category!=RequiresPostgreSQL"

- name: Run Integration Tests (with PostgreSQL)
  run: dotnet test --filter "Category=RequiresPostgreSQL"
  if: github.ref == 'refs/heads/main'
```

## 🐳 Tests avec Docker (ARM64)

Pour exécuter les tests dans un conteneur Docker ARM64 (Raspberry Pi) :

```bash
# Depuis la racine du projet
cd deployment/pi-nas

# Build et exécution des tests
docker build -f Dockerfile.arm64.tests -t mo5-rag-tests:arm64 ../..
docker run --rm mo5-rag-tests:arm64

# Ou avec docker compose (si configuré)
docker compose -f docker-compose.tests.yml up --build
```

**Note** : Le fichier `Dockerfile.arm64.tests` est optimisé pour l'architecture ARM64 (Raspberry Pi 4/5).


# Mo5 RAG Server - Référence API

## Vue d'ensemble

L'API Mo5 RAG Server fournit des endpoints pour la recherche sémantique dans la documentation Thomson MO5. Elle utilise des embeddings OpenAI et une base de données vectorielle PostgreSQL avec pgvector.

**Base URL**: `http://localhost:8080` (développement)

## Authentification

Actuellement, l'API ne nécessite pas d'authentification. En production, considérez l'ajout d'une authentification par clé API.

## Endpoints

### Health Check

#### `GET /health`

Vérifie l'état de santé de l'API.

**Réponse**:
```
200 OK
Content-Type: text/plain

Healthy
```

---

### Recherche Sémantique

#### `POST /api/search`

Effectue une recherche sémantique avancée dans la base de connaissances.

**Corps de la requête**:
```json
{
  "query": "C programming on MO5",
  "maxResults": 10,
  "minSimilarityScore": 0.7,
  "tags": ["C", "examples"],
  "includeMetadata": true,
  "includeContext": false
}
```

**Paramètres**:
- `query` (string, requis): Texte de recherche
- `maxResults` (int, optionnel): Nombre maximum de résultats (défaut: 10, max: 50)
- `minSimilarityScore` (float, optionnel): Score de similarité minimum (0.0-1.0, défaut: 0.7)
- `tags` (string[], optionnel): Filtrer par tags
- `includeMetadata` (bool, optionnel): Inclure les métadonnées des documents
- `includeContext` (bool, optionnel): Inclure le contexte autour des chunks

**Réponse**:
```json
{
  "query": "C programming on MO5",
  "results": [
    {
      "chunkId": "uuid",
      "content": "Contenu du chunk...",
      "similarityScore": 0.85,
      "document": {
        "documentId": "uuid",
        "fileName": "c-programming.md",
        "title": "Guide de programmation C",
        "filePath": "c-programming.md",
        "lastModified": "2024-01-15T10:30:00Z",
        "tags": ["C", "examples"]
      },
      "position": {
        "chunkIndex": 0,
        "startPosition": 0,
        "endPosition": 500,
        "sectionHeading": "Introduction"
      }
    }
  ],
  "totalResults": 1,
  "executionTimeMs": 150,
  "filters": {
    "tags": ["C", "examples"],
    "minSimilarityScore": 0.7,
    "maxResults": 10
  }
}
```

#### `GET /api/search`

Recherche simple via query string.

**Paramètres**:
- `q` (string, requis): Texte de recherche
- `maxResults` (int, optionnel): Nombre maximum de résultats
- `minScore` (float, optionnel): Score de similarité minimum
- `tags` (string, optionnel): Tags séparés par des virgules

**Exemple**:
```
GET /api/search?q=6809%20assembly&maxResults=5&tags=Assembly,hardware
```

#### `GET /api/search/suggestions`

Obtient des suggestions de recherche basées sur un texte partiel.

**Paramètres**:
- `partial` (string, requis): Texte partiel (minimum 3 caractères)
- `limit` (int, optionnel): Nombre maximum de suggestions (défaut: 10, max: 20)

**Réponse**:
```json
[
  "6809 processor",
  "6809 assembly language",
  "6809 interrupts"
]
```

---

### Gestion des Documents

#### `GET /api/documents`

Liste tous les documents indexés.

**Paramètres**:
- `tags` (string, optionnel): Filtrer par tags (séparés par des virgules)

**Réponse**:
```json
[
  {
    "id": "uuid",
    "fileName": "c-programming.md",
    "title": "Guide de programmation C",
    "filePath": "c-programming.md",
    "fileSize": 15420,
    "lastModified": "2024-01-15T10:30:00Z",
    "createdAt": "2024-01-10T08:00:00Z",
    "tags": ["C", "examples"],
    "chunkCount": 12
  }
]
```

#### `GET /api/documents/{id}`

Obtient les détails d'un document spécifique.

**Réponse**:
```json
{
  "id": "uuid",
  "fileName": "c-programming.md",
  "title": "Guide de programmation C",
  "content": "Contenu complet du document...",
  "filePath": "c-programming.md",
  "fileSize": 15420,
  "lastModified": "2024-01-15T10:30:00Z",
  "createdAt": "2024-01-10T08:00:00Z",
  "chunks": [
    {
      "id": "uuid",
      "chunkIndex": 0,
      "content": "Contenu du chunk...",
      "startPosition": 0,
      "endPosition": 500,
      "tokenCount": 125
    }
  ],
  "documentTags": [
    {
      "tag": {
        "name": "C",
        "category": "language",
        "color": "#00599C"
      },
      "confidence": 0.95,
      "assignmentSource": "auto"
    }
  ]
}
```

#### `DELETE /api/documents/{id}`

Supprime (désactive) un document.

**Réponse**:
```json
{
  "message": "Document supprimé avec succès"
}
```

#### `GET /api/documents/tags`

Liste tous les tags disponibles.

**Réponse**:
```json
[
  {
    "id": "uuid",
    "name": "C",
    "category": "language",
    "description": "Langage de programmation C",
    "color": "#00599C",
    "documentCount": 5
  }
]
```

---

### Indexation

#### `POST /api/index/all`

Lance la réindexation complète de tous les documents.

**Réponse**:
```json
{
  "message": "Indexation démarrée",
  "estimatedDuration": "2-5 minutes"
}
```

#### `POST /api/index/document`

Indexe un document spécifique.

**Corps de la requête**:
```json
{
  "filePath": "assembly-graphics-mode.md"
}
```

#### `GET /api/index/status`

Obtient le statut de l'indexation.

**Réponse**:
```json
{
  "totalDocuments": 25,
  "totalChunks": 340,
  "lastIndexed": "2024-01-15T14:30:00Z",
  "isIndexing": false,
  "tagDistribution": {
    "C": 8,
    "Assembly": 12,
    "graphics-mode": 6,
    "text-mode": 4,
    "hardware": 10
  }
}
```

---

### Monitoring de l'Ingestion

#### `GET /api/ingestion/stats`

Obtient les statistiques d'ingestion automatique.

**Réponse**:
```json
{
  "totalFilesProcessed": 25,
  "successfulIndexings": 24,
  "failedIndexings": 1,
  "successRate": 96.0,
  "totalProcessingTimeMs": 45000,
  "averageProcessingTimeMs": 1875,
  "totalChunksCreated": 340,
  "lastProcessedAt": "2024-01-15T14:30:00Z",
  "lastFailureAt": "2024-01-15T12:15:00Z",
  "lastError": "File not accessible",
  "isWatching": true,
  "recentActivities": [
    {
      "timestamp": "2024-01-15T14:30:00Z",
      "filePath": "new-document.md",
      "action": "Indexed",
      "success": true,
      "processingTimeMs": 1200
    }
  ]
}
```

#### `GET /api/ingestion/activities`

Obtient les activités récentes d'ingestion.

**Paramètres**:
- `limit` (int, optionnel): Nombre maximum d'activités (défaut: 50, max: 200)

#### `POST /api/ingestion/stats/reset`

Remet à zéro les statistiques d'ingestion.

#### `GET /api/ingestion/watcher/status`

Obtient le statut du surveillant de fichiers.

**Réponse**:
```json
{
  "isWatching": true,
  "status": "Active"
}
```

---

## Codes d'Erreur

| Code | Description |
|------|-------------|
| 200  | Succès |
| 400  | Requête invalide (paramètres manquants ou incorrects) |
| 404  | Ressource non trouvée |
| 500  | Erreur interne du serveur |

## Exemples d'Utilisation

### Recherche simple
```bash
curl "http://localhost:8080/api/search?q=6809%20assembly"
```

### Recherche avancée
```bash
curl -X POST http://localhost:8080/api/search \
  -H "Content-Type: application/json" \
  -d '{
    "query": "How to program graphics on MO5",
    "maxResults": 5,
    "tags": ["Assembly", "graphics-mode"],
    "includeMetadata": true
  }'
```

### Indexation
```bash
curl -X POST http://localhost:8080/api/index/all
```

### Monitoring
```bash
curl http://localhost:8080/api/ingestion/stats
```

## Limites

- **Taille de requête**: Maximum 1MB
- **Résultats de recherche**: Maximum 50 par requête
- **Rate limiting**: 10 requêtes/seconde par IP (en production avec Nginx)
- **Timeout**: 30 secondes pour les requêtes de recherche

## Support

Pour des questions ou des problèmes avec l'API, consultez les logs de l'application ou créez une issue sur le repository GitHub.

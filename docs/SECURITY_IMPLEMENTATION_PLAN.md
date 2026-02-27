# Plan d'implémentation - Sécurité API MO5 RAG Server

**Date :** 2026-02-27
**Statut :** ✅ Implémenté (Memory cache - pattern Strategy prêt pour Redis)

---

## 🎯 Objectifs

1. **Rate Limiting** : Limiter le nombre de requêtes par IP et par API Key
2. **Anti-Brute-Force** : Bloquer les IPs après N tentatives échouées pendant 30 minutes
3. **Monitoring** : Dashboard pour visualiser les logs et les IPs bloquées
4. **Configuration** : Tous les paramètres configurables via variables d'environnement

---

## 🏗️ Architecture retenue

```
┌─────────────────────────────────────────────────────────────┐
│  Docker Compose Stack sur VPS Hostinger                     │
│                                                              │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │ Postgres │  │ Embedding│  │  Redis   │  │   Seq    │   │
│  │ +pgvector│  │  Service │  │  (cache) │  │  (logs)  │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
│                                     ▲            ▲          │
│  ┌──────────────────────────────────┼────────────┼───────┐ │
│  │  .NET API                        │            │       │ │
│  │                                  │            │       │ │
│  │  ┌────────────────────────────────────────────────┐  │ │
│  │  │  1. Rate Limiting Middleware                   │  │ │
│  │  │     - Par IP: 100 req/min (configurable)       │  │ │
│  │  │     - Par API Key: 1000 req/heure (config.)    │  │ │
│  │  └────────────────────────────────────────────────┘  │ │
│  │                         ▼                             │ │
│  │  ┌────────────────────────────────────────────────┐  │ │
│  │  │  2. Anti-Brute-Force Middleware                │  │ │
│  │  │     - Track échecs par IP (Redis)              │  │ │
│  │  │     - Blocage 30min après 5 échecs (config.)   │  │ │
│  │  └────────────────────────────────────────────────┘  │ │
│  │                         ▼                             │ │
│  │  ┌────────────────────────────────────────────────┐  │ │
│  │  │  3. ApiKeyAuthenticationHandler (existant)     │  │ │
│  │  └────────────────────────────────────────────────┘  │ │
│  │                         ▼                             │ │
│  │  ┌────────────────────────────────────────────────┐  │ │
│  │  │  4. Endpoints métier                           │  │ │
│  │  └────────────────────────────────────────────────┘  │ │
│  └───────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## 📦 Technologies & Packages

### Packages NuGet à ajouter
```xml
<!-- Rate Limiting (ASP.NET 7+) -->
<PackageReference Include="System.Threading.RateLimiting" Version="8.0.0" />

<!-- Redis pour cache distribué -->
<PackageReference Include="StackExchange.Redis" Version="2.7.10" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.0" />

<!-- Serilog Seq Sink (pour logs) -->
<PackageReference Include="Serilog.Sinks.Seq" Version="7.0.0" />
```

### Containers Docker
- **Redis** : Cache distribué pour tracking des IPs bloquées
- **Seq** : Dashboard de logs (accès via tunnel SSH uniquement)

---

## 🔧 Variables d'environnement

### Fichier `.env` (à ajouter)
```bash
# --- RATE LIMITING ---
RATE_LIMIT_ENABLED=true
RATE_LIMIT_REQUESTS_PER_MINUTE=100
RATE_LIMIT_REQUESTS_PER_HOUR=1000

# --- ANTI-BRUTE-FORCE ---
BRUTE_FORCE_ENABLED=true
BRUTE_FORCE_MAX_ATTEMPTS=5
BRUTE_FORCE_BLOCK_DURATION_MINUTES=30

# --- REDIS ---
REDIS_ENABLED=true
REDIS_CONNECTION_STRING=redis:6379

# --- SEQ (Logs Dashboard) ---
SEQ_ENABLED=true
SEQ_URL=http://seq:5341

# --- IP WHITELIST (optionnel) ---
WHITELISTED_IPS=127.0.0.1,::1
```

---

## 📁 Structure des fichiers à créer

```
api-sources/src/Mo5.RagServer.Api/
├── Security/
│   ├── ApiKeyAuthenticationHandler.cs          (✅ existant)
│   ├── Middleware/
│   │   ├── RateLimitingMiddleware.cs           (🆕 à créer)
│   │   └── AntiBruteForceMiddleware.cs         (🆕 à créer)
│   └── Models/
│       ├── RateLimitSettings.cs                (🆕 à créer)
│       ├── AntiBruteForceSettings.cs           (🆕 à créer)
│       └── RedisSettings.cs                    (🆕 à créer)
│
├── Services/
│   ├── IBlockedIpService.cs                    (🆕 à créer)
│   ├── RedisBlockedIpService.cs                (🆕 à créer)
│   └── MemoryBlockedIpService.cs               (🆕 à créer - fallback)
│
└── Controllers/
    └── AdminSecurityController.cs              (🆕 à créer - optionnel)
```

---

## 🐳 Docker Compose - Modifications

### Ajout de Redis
```yaml
services:
  redis:
    image: redis:7-alpine
    container_name: mo5-rag-redis
    command: redis-server --appendonly yes
    volumes:
      - ./data/redis:/data
    networks:
      - mo5-rag-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 3s
      retries: 3
```

### Ajout de Seq (Dashboard logs)
```yaml
  seq:
    image: datalust/seq:latest
    container_name: mo5-rag-seq
    environment:
      - ACCEPT_EULA=Y
    ports:
      - "127.0.0.1:5341:80"  # ⚠️ Localhost uniquement (sécurité)
    volumes:
      - ./data/seq:/data
    networks:
      - mo5-rag-network
    restart: unless-stopped
```

### Mise à jour du service API
```yaml
  api:
    # ... configuration existante ...
    environment:
      # ... variables existantes ...
      
      # Rate Limiting
      - RateLimitSettings__Enabled=${RATE_LIMIT_ENABLED:-true}
      - RateLimitSettings__RequestsPerMinute=${RATE_LIMIT_REQUESTS_PER_MINUTE:-100}
      - RateLimitSettings__RequestsPerHour=${RATE_LIMIT_REQUESTS_PER_HOUR:-1000}
      
      # Anti-Brute-Force
      - AntiBruteForceSettings__Enabled=${BRUTE_FORCE_ENABLED:-true}
      - AntiBruteForceSettings__MaxFailedAttempts=${BRUTE_FORCE_MAX_ATTEMPTS:-5}
      - AntiBruteForceSettings__BlockDurationMinutes=${BRUTE_FORCE_BLOCK_DURATION_MINUTES:-30}
      
      # Redis
      - RedisSettings__Enabled=${REDIS_ENABLED:-true}
      - RedisSettings__ConnectionString=${REDIS_CONNECTION_STRING:-redis:6379}
      
      # Seq
      - Serilog__WriteTo__1__Name=Seq
      - Serilog__WriteTo__1__Args__serverUrl=${SEQ_URL:-http://seq:5341}

    depends_on:
      postgres:
        condition: service_healthy
      embedding-api:
        condition: service_healthy
      redis:
        condition: service_healthy
```

---

## ⚙️ Configuration appsettings.json

### Ajout des sections de configuration
```json
{
  "RateLimitSettings": {
    "Enabled": true,
    "RequestsPerMinute": 100,
    "RequestsPerHour": 1000,
    "EnableIpRateLimiting": true,
    "EnableApiKeyRateLimiting": true
  },
  "AntiBruteForceSettings": {
    "Enabled": true,
    "MaxFailedAttempts": 5,
    "BlockDurationMinutes": 30,
    "TrackByIp": true
  },
  "RedisSettings": {
    "Enabled": false,
    "ConnectionString": "localhost:6379",
    "InstanceName": "mo5-rag:"
  },
  "SecuritySettings": {
    "WhitelistedIps": ["127.0.0.1", "::1"]
  },
  "Serilog": {
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq:5341"
        }
      }
    ]
  }
}
```

---

## 🔐 Accès au Dashboard Seq

### Méthode retenue : Tunnel SSH (sécurité maximale)

**Depuis ta machine locale :**
```bash
# Créer le tunnel SSH
ssh -L 5341:localhost:5341 user@ton-vps-hostinger.com

# Puis ouvrir dans le navigateur :
# http://localhost:5341
```

**Avantages :**
- ✅ Sécurité maximale (Seq non exposé sur internet)
- ✅ Pas de configuration supplémentaire
- ✅ Pas de risque d'accès non autorisé
- ✅ Gratuit

**Inconvénients :**
- ❌ Nécessite un tunnel SSH à chaque fois
- ❌ Pas d'accès mobile facile

### Requêtes Seq utiles

```sql
-- Voir toutes les IPs bloquées
event = "IpBlocked"

-- Rate limits dépassés aujourd'hui
event = "RateLimitExceeded" and @Timestamp > Now() - 1d

-- Top 10 IPs les plus actives
select count(*) as requests, ip
group by ip
order by requests desc

-- Tentatives échouées par IP
event = "ApiKeyFailed"
| group by ip
| where count() > 3
```

---

## 📊 Logs structurés (exemples)

### Event : Rate Limit dépassé
```json
{
  "timestamp": "2026-02-19T10:30:00Z",
  "level": "Warning",
  "event": "RateLimitExceeded",
  "ip": "203.0.113.42",
  "endpoint": "/api/search",
  "limit": 100,
  "window": "1 minute",
  "userAgent": "Mozilla/5.0..."
}
```

### Event : IP bloquée (brute-force)
```json
{
  "timestamp": "2026-02-19T10:31:00Z",
  "level": "Error",
  "event": "IpBlocked",
  "ip": "203.0.113.42",
  "reason": "BruteForce",
  "failedAttempts": 5,
  "blockDuration": "30 minutes",
  "expiresAt": "2026-02-19T11:01:00Z"
}
```

### Event : Tentative API Key invalide
```json
{
  "timestamp": "2026-02-19T10:25:00Z",
  "level": "Warning",
  "event": "ApiKeyFailed",
  "ip": "203.0.113.42",
  "endpoint": "/api/search",
  "attemptNumber": 3
}
```

---

## 🔄 Ordre d'exécution dans le pipeline ASP.NET

```csharp
// Program.cs
var app = builder.Build();

// Pipeline de sécurité (ordre important !)
app.UseMiddleware<RateLimitingMiddleware>();      // 1. Rate limiting
app.UseMiddleware<AntiBruteForceMiddleware>();    // 2. Anti brute-force
app.UseAuthentication();                          // 3. API Key validation (existant)
app.UseAuthorization();                           // 4. Authorization
app.MapControllers();                             // 5. Endpoints métier
```

---

## 🎯 Endpoints Admin (optionnels)

### À créer dans `AdminSecurityController.cs`

```csharp
// Voir les IPs bloquées
GET /admin/security/blocked-ips

// Débloquer une IP manuellement
POST /admin/security/unblock/{ip}

// Ajouter une IP à la whitelist
POST /admin/security/whitelist/{ip}

// Statistiques de sécurité
GET /admin/security/stats

// Voir les rate limits actuels
GET /admin/security/rate-limits
```

**Protection :** Ces endpoints doivent être protégés par un rôle admin ou une API key spéciale.

---

## 📝 Headers HTTP standards

### Réponses Rate Limiting
```http
HTTP/1.1 429 Too Many Requests
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1708340400
Retry-After: 60
```

### Réponses IP bloquée
```http
HTTP/1.1 403 Forbidden
X-Blocked-Reason: BruteForce
X-Blocked-Until: 2026-02-19T11:01:00Z
Retry-After: 1800
```

---

## ✅ Checklist d'implémentation

### Phase 1 : Infrastructure
- [ ] Ajouter Redis au `docker-compose.yml` (optionnel - pas encore nécessaire)
- [ ] Ajouter Seq au `docker-compose.yml` (optionnel pour monitoring)
- [x] Mettre à jour `.env.example` avec nouvelles variables ✅
- [x] Ajouter packages NuGet au projet (System.Threading.RateLimiting) ✅

### Phase 2 : Configuration
- [x] Créer `RateLimitSettings.cs` ✅
- [x] Créer `AntiBruteForceSettings.cs` ✅
- [ ] Créer `RedisSettings.cs` (optionnel - plus tard)
- [x] Mettre à jour `appsettings.json` ✅

### Phase 3 : Services
- [x] Créer `IBlockedIpService.cs` (interface Strategy pattern) ✅
- [ ] Créer `RedisBlockedIpService.cs` (optionnel - plus tard)
- [x] Créer `InMemoryBlockedIpService.cs` ✅
- [x] Enregistrer les services dans `Program.cs` ✅

### Phase 4 : Middlewares
- [x] Créer `RateLimitingMiddleware.cs` (30 req/min par défaut) ✅
- [x] Créer `AntiBruteForceMiddleware.cs` ✅
- [x] Enregistrer les middlewares dans `Program.cs` ✅

### Phase 5 : Logs
- [x] Logs structurés dans les middlewares ✅
- [ ] Configurer Serilog pour Seq (optionnel - plus tard)
- [ ] Tester les événements dans Seq (optionnel - plus tard)

### Phase 6 : Admin (optionnel)
- [ ] Créer `AdminSecurityController.cs`
- [ ] Implémenter endpoints de gestion
- [ ] Protéger les endpoints admin

### Phase 7 : Tests
- [ ] Tester rate limiting par IP
- [ ] Tester blocage après N échecs
- [ ] Tester déblocage automatique après expiration
- [ ] Tester whitelist IPs

### Phase 8 : Déploiement
- [ ] Mettre à jour `.env` sur le VPS
- [ ] Déployer avec `docker-compose up -d`
- [ ] Vérifier les health checks
- [ ] Monitorer les premiers jours

---

## 🚀 Commandes utiles

### Déploiement
```bash
# Sur le VPS
cd /path/to/mo5-rag-server
docker-compose down
docker-compose pull
docker-compose up -d --build

# Vérifier les logs
docker-compose logs -f api
docker-compose logs -f redis
docker-compose logs -f seq
```

### Accès Seq
```bash
# Depuis ta machine locale
ssh -L 5341:localhost:5341 user@ton-vps.com

# Navigateur : http://localhost:5341
```

### Redis CLI (debug)
```bash
# Voir les IPs bloquées
docker exec -it mo5-rag-redis redis-cli
> KEYS mo5-rag:blocked:*
> GET mo5-rag:blocked:203.0.113.42
> TTL mo5-rag:blocked:203.0.113.42
```

---

## 📚 Ressources

- [ASP.NET Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
- [Seq Documentation](https://docs.datalust.co/docs)
- [Serilog Structured Logging](https://github.com/serilog/serilog/wiki/Structured-Data)

---

## 📌 Notes importantes

1. **Redis est optionnel** : Le système peut fonctionner avec cache mémoire si Redis est désactivé (via `REDIS_ENABLED=false`)
2. **Seq est en localhost** : Accessible uniquement via tunnel SSH pour sécurité maximale
3. **Tout est configurable** : Aucun rebuild nécessaire, juste modifier `.env` et redémarrer
4. **Whitelist IPs** : Permet d'exclure certaines IPs (ton infra, partenaires) du rate limiting
5. **Tests** : En environnement `Testing`, les middlewares peuvent être désactivés automatiquement

---

**Dernière mise à jour :** 2026-02-19
**Auteur :** Plan établi avec Augment Agent


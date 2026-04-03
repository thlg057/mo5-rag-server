# Accès aux services internes via SSH Tunnel

Ce document explique comment accéder aux services non exposés publiquement (Swagger, Seq) via un tunnel SSH sécurisé.

---

## 🔒 Pourquoi utiliser un tunnel SSH ?

Les services suivants ne sont **pas exposés sur Internet** pour des raisons de sécurité :

| Service | Port local | Description |
|---------|------------|-------------|
| **Swagger UI** | 5000 | Documentation interactive de l'API |
| **Seq Dashboard** | 5341 | Logs et monitoring (optionnel) |

Le tunnel SSH permet d'y accéder **comme s'ils étaient en localhost**, tout en gardant une connexion chiffrée.

---

## 📋 Prérequis

- Accès SSH au VPS (clé SSH ou mot de passe)
- Client SSH (Terminal Linux/Mac, PowerShell/Git Bash sur Windows)

---

## 🚀 Connexion au Swagger

### 1. Ouvrir le tunnel SSH

```bash
# Remplacer par tes informations de connexion
ssh -L 5000:localhost:5000 user@ton-vps-hostinger.com
```

**Explication :**
- `-L 5000:localhost:5000` : Redirige le port 5000 local vers le port 5000 du serveur
- La session SSH reste ouverte tant que tu as besoin du tunnel

### 2. Accéder au Swagger

Ouvre ton navigateur à l'adresse :

```
http://localhost:5000/swagger
```

### 3. Fermer le tunnel

Simplement fermer le terminal SSH ou taper `exit`.

---

## 📊 Connexion au Dashboard Seq (Logs)

> ⚠️ **Seq doit être déployé** - Voir `SECURITY_IMPLEMENTATION_PLAN.md` pour l'ajout dans docker-compose.

### 1. Ouvrir le tunnel SSH

```bash
ssh -L 5341:localhost:5341 user@ton-vps-hostinger.com
```

### 2. Accéder au Dashboard Seq

Ouvre ton navigateur à l'adresse :

```
http://localhost:5341
```

### 3. Requêtes Seq utiles

Une fois connecté, tu peux utiliser ces requêtes :

```sql
-- Voir toutes les IPs bloquées
@Message like '%blocked%'

-- Rate limits dépassés aujourd'hui
@Message like '%rate limit%' and @Timestamp > Now() - 1d

-- Échecs d'authentification
@Message like '%Invalid API key%'

-- Erreurs uniquement
@Level = 'Error'
```

---

## 🔗 Tunnel multiple (Swagger + Seq)

Tu peux ouvrir plusieurs tunnels en une seule commande :

```bash
ssh -L 5000:localhost:5000 -L 5341:localhost:5341 user@ton-vps-hostinger.com
```

Puis accéder à :
- Swagger : `http://localhost:5000/swagger`
- Seq : `http://localhost:5341`

---

## 💡 Astuces

### Alias SSH (Linux/Mac)

Ajoute dans `~/.ssh/config` :

```
Host mo5-vps
    HostName ton-vps-hostinger.com
    User ton-user
    LocalForward 5000 localhost:5000
    LocalForward 5341 localhost:5341
```

Puis simplement :
```bash
ssh mo5-vps
```

### Script Windows (PowerShell)

Crée un fichier `connect-mo5.ps1` :

```powershell
# Connexion avec tunnels
ssh -L 5000:localhost:5000 -L 5341:localhost:5341 user@ton-vps-hostinger.com
```

---

## ⚠️ Dépannage

| Problème | Solution |
|----------|----------|
| `Connection refused` | Vérifier que le service tourne sur le VPS |
| `Address already in use` | Un autre processus utilise le port local |
| `Permission denied` | Vérifier les credentials SSH |
| Page blanche Swagger | L'API n'est peut-être pas en mode Development |

### Vérifier les services sur le VPS

```bash
# Vérifier que les containers tournent
docker ps

# Vérifier les logs de l'API
docker logs mo5-rag-api

# Vérifier les ports écoutés
netstat -tlnp | grep -E "5000|5341"
```

---

**Dernière mise à jour :** 2026-02-27


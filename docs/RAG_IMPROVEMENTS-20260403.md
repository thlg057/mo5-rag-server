# 📚 RAG Improvements – Technical Deep Dive

Ce document détaille les évolutions du système RAG, les problèmes rencontrés, ainsi que les solutions techniques mises en place.

---

# 🧠 1. Problème : dépendance à la formulation utilisateur

## Symptômes

Une même intention utilisateur produisait des résultats différents :

- "Mon jeu rame"
- "jeu lent"
- "fps faible"

→ résultats incohérents

## Cause

Le système utilisait **un seul embedding de la query brute**  
→ forte dépendance au wording

---

# ✅ Solution : Multi-query sans LLM

## Principe

Au lieu d'une seule query, on génère plusieurs variantes :

- synonymes
- termes métier
- reformulations simples

## Implémentation

Ajout d’un fichier config :

`query-expansion.json`

```json
{
  "expansions": {
    "rame": ["lent", "slow", "fps", "performance"],
    "collision": ["hitbox", "overlap"]
  }
}
```

## Intégration

- Chargé via `IRagConfigService`
- Fonction `ExpandToMultiQueries()`
- Génère plusieurs embeddings
- On garde le **meilleur score par chunk**

## Bénéfices

- énorme gain en recall
- pas de dépendance LLM
- comportement déterministe

---

# 🧠 2. Problème : résultats manquants (recall faible)

## Cause

Un chunk pertinent pouvait être ignoré si :

- wording différent
- embedding légèrement différent

---

# ✅ Solution : scoring multi-embedding

## Implémentation

```csharp
foreach (var embedding in queryEmbeddings)
{
    score = max(score, cosine(chunk, embedding));
}
```

## Résultat

- un document peut matcher sur **n'importe quelle variation**
- améliore fortement la robustesse

---

# 🧠 3. Problème : bruit et redondance

## Symptômes

- plusieurs chunks du même document
- résultats très similaires

---

# ✅ Solution 1 : Dedup document

```csharp
GroupBy(DocumentId)
→ garder le meilleur chunk
```

---

# ✅ Solution 2 : Diversification

```csharp
GroupBy(category)
→ limiter à N résultats par catégorie
```

## Résultat

- résultats plus variés
- meilleure UX

---

# 🧠 4. Problème : scoring trop brut

## Symptômes

- documents mal classés
- pas de logique métier

---

# ✅ Solution : Reranking configurable

## Fichier

`reranking.json`

```json
{
  "boostRules": [
    { "keywords": ["problem", "problème"], "boost": 0.03 },
    { "keywords": ["solution", "fix"], "boost": 0.05 },
    { "keywords": ["optimize", "performance"], "boost": 0.04 }
  ]
}
```

## Fonctionnement

- après calcul vectoriel
- on booste certains contenus

## Avantages

- pilotable sans code
- adaptable métier

---

# 🧠 5. Problème : perte des mots exacts

## Symptômes

- "rame" ≠ toujours match avec "performance"
- perte de précision

---

# ✅ Solution : Hybrid Search

## Principe

Combiner :

- vector search (sens)
- keyword search (exact)

## Implémentation

```csharp
keywordScore = content.Contains(word)
finalScore = vectorScore * 0.8 + keywordScore * 0.2
```

## Résultat

- précision + compréhension
- meilleur ranking global

---

# 🧠 6. Problème : embeddings peu contextualisés

## Cause

chunk seul sans contexte

---

# ✅ Solution : Enrichissement sémantique

Avant embedding :

```text
Document: Title > Section: X
Contenu: ...
```

## Résultat

- meilleure compréhension du contexte
- embeddings plus cohérents

---

# 🧠 7. Problème : logique codée en dur

## Symptômes

- difficile à maintenir
- difficile à adapter

---

# ✅ Solution : externalisation config

## Fichiers

- `tag-mapping.json`
- `reranking.json`
- `query-expansion.json`

## Avantages

- tuning sans code
- évolutif
- adaptable à d'autres domaines

---

# 🧠 8. Contrainte : coût (pas de LLM)

## Choix

- aucune utilisation LLM
- uniquement règles + embeddings

## Résultat

- coût = 0
- latence faible
- infra simple

---

# 🚀 Architecture finale

Pipeline :

1. Query utilisateur
2. Expansion multi-query
3. Embeddings multiples
4. Retrieval (DB)
5. Scoring vectoriel (max)
6. Keyword scoring
7. Dedup
8. Diversification
9. Reranking
10. Résultat final

---

# 🏁 Conclusion

Le RAG est passé de :

- simple vector search

à :

- multi-query
- hybrid search
- reranking configurable
- tagging dynamique

## Résultat

- robuste
- explicable
- scalable
- sans LLM

Base solide pour production.

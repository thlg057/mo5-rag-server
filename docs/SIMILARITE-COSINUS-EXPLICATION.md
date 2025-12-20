# 📐 Similarité Cosinus - Comment on compare les embeddings ?

> Une explication simple de la similarité cosinus et comment elle permet de trouver les documents pertinents

## Le problème

On a généré des embeddings (vecteurs de 384 nombres) pour tous les chunks de documentation.

On a aussi généré un embedding pour la question de l'utilisateur.

**Maintenant, comment savoir quels chunks sont les plus pertinents ?**

Il faut **comparer** l'embedding de la question avec tous les embeddings des chunks.

Mais comment comparer deux listes de 384 nombres ? 🤔

---

## La solution : Similarité cosinus

La **similarité cosinus** est une formule mathématique qui mesure à quel point deux vecteurs sont "proches".

### L'idée de base

Imaginez deux vecteurs comme des **flèches** dans l'espace.

```
        ↗ Vecteur A (question)
       /
      /
     /____→ Vecteur B (chunk)
```

**Similarité cosinus** = mesure de l'**angle** entre les deux flèches.

- Si les flèches pointent dans la **même direction** → angle petit → similarité élevée
- Si les flèches pointent dans des **directions opposées** → angle grand → similarité faible

### Le score

La similarité cosinus donne un **score entre -1 et 1** :

- **1** = Vecteurs identiques (même direction)
- **0** = Vecteurs perpendiculaires (aucun lien)
- **-1** = Vecteurs opposés (sens contraire)

Dans notre cas, les vecteurs sont normalisés (longueur = 1), donc le score est généralement entre **0 et 1**.

**Interprétation** :

- **0.9 - 1.0** : Très similaire ⭐⭐⭐
- **0.7 - 0.9** : Similaire ⭐⭐
- **0.5 - 0.7** : Moyennement similaire ⭐
- **0.0 - 0.5** : Peu similaire

---

## La formule mathématique

### Formule complète

```
similarité_cosinus = (A · B) / (||A|| × ||B||)
```

**Où** :
- **A · B** = produit scalaire (dot product)
- **||A||** = magnitude (longueur) du vecteur A
- **||B||** = magnitude (longueur) du vecteur B

### Étape par étape

#### 1. Produit scalaire (dot product)

On multiplie chaque dimension des deux vecteurs et on additionne :

```
A = [0.5, 0.3, 0.1]
B = [0.6, 0.2, 0.4]

A · B = (0.5 × 0.6) + (0.3 × 0.2) + (0.1 × 0.4)
      = 0.30 + 0.06 + 0.04
      = 0.40
```

#### 2. Magnitude (longueur) des vecteurs

On calcule la "longueur" de chaque vecteur avec le théorème de Pythagore :

```
||A|| = √(0.5² + 0.3² + 0.1²)
      = √(0.25 + 0.09 + 0.01)
      = √0.35
      = 0.59

||B|| = √(0.6² + 0.2² + 0.4²)
      = √(0.36 + 0.04 + 0.16)
      = √0.56
      = 0.75
```

#### 3. Division

On divise le produit scalaire par le produit des magnitudes :

```
similarité_cosinus = 0.40 / (0.59 × 0.75)
                   = 0.40 / 0.44
                   = 0.91
```

**Résultat** : Score de **0.91** → Très similaire ! ⭐⭐⭐

---

## Implémentation en C#

Voici le code utilisé dans le projet :

```csharp
private static float CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
{
    if (vectorA.Length != vectorB.Length)
        return 0f;

    double dotProduct = 0;
    double magnitudeA = 0;
    double magnitudeB = 0;

    // Calculer le produit scalaire et les magnitudes en une seule passe
    for (int i = 0; i < vectorA.Length; i++)
    {
        dotProduct += vectorA[i] * vectorB[i];      // A · B
        magnitudeA += vectorA[i] * vectorA[i];      // ||A||²
        magnitudeB += vectorB[i] * vectorB[i];      // ||B||²
    }

    // Calculer les racines carrées des magnitudes
    magnitudeA = Math.Sqrt(magnitudeA);
    magnitudeB = Math.Sqrt(magnitudeB);

    // Éviter la division par zéro
    if (magnitudeA == 0 || magnitudeB == 0)
        return 0f;

    // Calculer la similarité cosinus
    return (float)(dotProduct / (magnitudeA * magnitudeB));
}
```


#### 2. Calcul des similarités

Le système calcule la similarité cosinus entre la question et chaque chunk :

```
Similarité(Q, C1) = CalculateCosineSimilarity([0.5, 0.3, 0.8, ...], [0.52, 0.28, 0.75, ...])
                  = 0.94  ⭐⭐⭐ (très similaire)

Similarité(Q, C2) = CalculateCosineSimilarity([0.5, 0.3, 0.8, ...], [-0.1, 0.6, -0.3, ...])
                  = 0.23  (peu similaire)

Similarité(Q, C3) = CalculateCosineSimilarity([0.5, 0.3, 0.8, ...], [0.48, 0.32, 0.77, ...])
                  = 0.92  ⭐⭐⭐ (très similaire)
```

#### 3. Tri par score

Les chunks sont triés par score décroissant :

```
1. Chunk 1 : 0.94 ⭐⭐⭐
2. Chunk 3 : 0.92 ⭐⭐⭐
3. Chunk 2 : 0.23
```

#### 4. Filtrage par seuil

On ne garde que les chunks avec un score supérieur au seuil (par exemple 0.5) :

```
Résultats finaux :
1. Chunk 1 : 0.94 ⭐⭐⭐
2. Chunk 3 : 0.92 ⭐⭐⭐
```

**Résultat** : L'utilisateur reçoit les 2 chunks les plus pertinents sur le registre X !

---

## Pourquoi la similarité cosinus ?

### Avantages

**✅ Indépendante de la longueur**

La similarité cosinus mesure l'**angle**, pas la longueur.

Deux vecteurs peuvent avoir des longueurs différentes mais pointer dans la même direction → score élevé.

**Exemple** :

```
A = [1, 2, 3]
B = [2, 4, 6]  (2 fois plus long que A)

Similarité cosinus = 1.0  (même direction)
```

**✅ Normalisée entre -1 et 1**

Le score est toujours dans la même plage, facile à interpréter.

**✅ Rapide à calculer**

Une seule boucle sur les 384 dimensions, très efficace.

**✅ Fonctionne bien avec les embeddings**

Les embeddings représentent le "sens" du texte, et la similarité cosinus mesure la "proximité sémantique".

### Alternatives

Il existe d'autres méthodes pour comparer des vecteurs :

**Distance euclidienne** :
- Mesure la distance "en ligne droite" entre deux points
- Sensible à la longueur des vecteurs
- Moins adaptée aux embeddings

**Distance de Manhattan** :
- Somme des différences absolues sur chaque dimension
- Moins utilisée pour les embeddings

**Produit scalaire (dot product)** :
- Plus simple mais sensible à la longueur
- Nécessite des vecteurs normalisés

**Similarité cosinus** est le meilleur choix pour les embeddings ! 🎯

---

## Optimisation avec pgvector

### Le problème de performance

Si on a 10 000 chunks dans la base, il faut calculer 10 000 similarités pour chaque recherche.

Avec 384 dimensions par vecteur, ça fait beaucoup de calculs ! 😅

### La solution : Index IVFFlat

**pgvector** utilise un index spécial appelé **IVFFlat** (Inverted File with Flat compression).

**Principe** :
1. Les vecteurs sont regroupés en **clusters** (groupes)
2. Lors d'une recherche, on cherche d'abord les clusters les plus proches
3. Puis on cherche les vecteurs les plus proches **dans ces clusters**

**Résultat** : Au lieu de comparer avec 10 000 vecteurs, on compare avec ~100 vecteurs.

**Gain de performance** : 10x à 100x plus rapide ! 🚀

### Configuration de l'index

Dans le projet, l'index est créé avec cette commande SQL :

```sql
CREATE INDEX "IX_DocumentChunks_Embedding"
ON "DocumentChunks"
USING ivfflat ("Embedding" vector_cosine_ops)
WITH (lists = 100);
```

**Paramètres** :
- **ivfflat** : Type d'index (Inverted File with Flat compression)
- **vector_cosine_ops** : Opérateur de similarité cosinus
- **lists = 100** : Nombre de clusters (100 groupes)

**Recommandation** : `lists = nombre_de_lignes / 1000` (pour 10 000 chunks → 10 clusters)

---

## Utilisation dans le code

### Recherche avec similarité

Voici comment le système utilise la similarité cosinus pour la recherche :

```csharp
public async Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
{
    // 1. Générer l'embedding de la question
    var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(request.Query, cancellationToken);

    // 2. Récupérer les chunks de la base
    var chunks = await _context.DocumentChunks
        .Include(dc => dc.Document)
        .Where(dc => dc.Document.IsActive)
        .Take(1000)
        .ToListAsync(cancellationToken);

    // 3. Calculer la similarité pour chaque chunk
    var results = chunks
        .Select(chunk => new
        {
            Chunk = chunk,
            SimilarityScore = CalculateCosineSimilarity(chunk.Embedding.ToArray(), queryEmbedding.ToArray())
        })
        // 4. Filtrer par score minimum
        .Where(x => x.SimilarityScore >= request.MinSimilarityScore)
        // 5. Trier par score décroissant
        .OrderByDescending(x => x.SimilarityScore)
        // 6. Prendre les N meilleurs résultats
        .Take(request.MaxResults)
        .ToList();

    return new SearchResponse
    {
        Query = request.Query,
        Results = results,
        TotalResults = results.Count
    };
}
```

### Paramètres configurables

**MinSimilarityScore** : Score minimum pour qu'un chunk soit retourné
- Par défaut : **0.5**
- Valeurs typiques : 0.3 à 0.8
- Plus le seuil est élevé, plus les résultats sont précis (mais moins nombreux)

**MaxResults** : Nombre maximum de résultats à retourner
- Par défaut : **5**
- Valeurs typiques : 3 à 20
- Plus il y a de résultats, plus il y a de contexte (mais plus de bruit)

---

## Exemple avec des valeurs réelles

### Vecteurs simplifiés (3 dimensions au lieu de 384)

Pour mieux comprendre, voici un exemple avec des vecteurs de 3 dimensions :

**Question** : "registre accumulateur"
```
Embedding Q = [0.8, 0.5, 0.2]
```

**Chunk 1** : "Le registre A est un accumulateur 8 bits"
```
Embedding C1 = [0.75, 0.48, 0.25]
```

**Chunk 2** : "La mémoire vidéo est située à 0x4000"
```
Embedding C2 = [0.1, 0.9, -0.3]
```

### Calcul pour Chunk 1

```
1. Produit scalaire :
   A · B = (0.8 × 0.75) + (0.5 × 0.48) + (0.2 × 0.25)
         = 0.60 + 0.24 + 0.05
         = 0.89

2. Magnitude de Q :
   ||Q|| = √(0.8² + 0.5² + 0.2²)
         = √(0.64 + 0.25 + 0.04)
         = √0.93
         = 0.96

3. Magnitude de C1 :
   ||C1|| = √(0.75² + 0.48² + 0.25²)
          = √(0.56 + 0.23 + 0.06)
          = √0.85
          = 0.92

4. Similarité cosinus :
   cos(Q, C1) = 0.89 / (0.96 × 0.92)
              = 0.89 / 0.88
              = 1.01 ≈ 1.0  ⭐⭐⭐
```

**Résultat** : Score de **1.0** → Quasi identique !

### Calcul pour Chunk 2

```
1. Produit scalaire :
   A · B = (0.8 × 0.1) + (0.5 × 0.9) + (0.2 × -0.3)
         = 0.08 + 0.45 - 0.06
         = 0.47

2. Magnitude de Q : 0.96 (déjà calculée)

3. Magnitude de C2 :
   ||C2|| = √(0.1² + 0.9² + (-0.3)²)
          = √(0.01 + 0.81 + 0.09)
          = √0.91
          = 0.95

4. Similarité cosinus :
   cos(Q, C2) = 0.47 / (0.96 × 0.95)
              = 0.47 / 0.91
              = 0.52
```

**Résultat** : Score de **0.52** → Moyennement similaire

**Conclusion** : Chunk 1 (1.0) est beaucoup plus pertinent que Chunk 2 (0.52) !

---

## Visualisation géométrique

### Représentation 2D (simplifiée)

Imaginons des vecteurs en 2D pour visualiser :

```
      Y
      ↑
      |
      |    ↗ Q (question)
      |   /
      |  / 15°
      | /___→ C1 (chunk 1)
      |
      |
      |        ↗ C2 (chunk 2)
      |       /
      |      / 60°
      |     /
      |____/________________→ X
```

**Angle petit (15°)** → Similarité élevée (cos(15°) ≈ 0.97)
**Angle grand (60°)** → Similarité moyenne (cos(60°) = 0.50)

Plus l'angle est petit, plus les vecteurs pointent dans la même direction, plus ils sont similaires.

---

## 💬 Résumé

### Qu'est-ce que la similarité cosinus ?

Une **formule mathématique** qui mesure l'angle entre deux vecteurs.

**Score de -1 à 1** :
- **1** = Identique
- **0** = Aucun lien
- **-1** = Opposé

### Comment ça marche ?

**3 étapes** :
1. Calculer le **produit scalaire** (A · B)
2. Calculer les **magnitudes** (||A|| et ||B||)
3. Diviser : **similarité = (A · B) / (||A|| × ||B||)**

### Pourquoi c'est utile ?

**✅ Compare les embeddings** : Trouve les chunks les plus proches de la question
**✅ Rapide** : Une seule boucle sur les 384 dimensions
**✅ Normalisé** : Score toujours entre -1 et 1
**✅ Optimisé** : Index IVFFlat de pgvector pour la performance

### Dans le RAG Server

1. Question → Embedding Q
2. Pour chaque chunk → Calculer similarité(Q, chunk)
3. Trier par score décroissant
4. Retourner les top N résultats

**C'est grâce à la similarité cosinus qu'on peut trouver les passages pertinents dans la documentation !** 🚀

**Optimisation** : On calcule tout en une seule boucle pour être plus rapide.

---

## Exemple concret avec le RAG Server

### Scénario

Vous posez la question : **"Comment utiliser le registre X ?"**

Le système a 3 chunks dans la base :

**Chunk 1** : "Le registre X est utilisé pour l'indexation mémoire..."
**Chunk 2** : "La mémoire vidéo du MO5 est située à l'adresse..."
**Chunk 3** : "Le registre X permet d'accéder aux tableaux..."

### Processus

#### 1. Génération des embeddings

```
Question : "Comment utiliser le registre X ?"
→ Embedding Q : [0.5, 0.3, 0.8, ..., 0.2]  (384 dimensions)

Chunk 1 : "Le registre X est utilisé pour l'indexation..."
→ Embedding C1 : [0.52, 0.28, 0.75, ..., 0.19]

Chunk 2 : "La mémoire vidéo du MO5..."
→ Embedding C2 : [-0.1, 0.6, -0.3, ..., 0.5]

Chunk 3 : "Le registre X permet d'accéder aux tableaux..."
→ Embedding C3 : [0.48, 0.32, 0.77, ..., 0.21]
```



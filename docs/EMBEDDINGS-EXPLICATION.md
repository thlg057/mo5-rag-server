# 🧮 Comment sont générés les embeddings ?

> Une explication simple de la génération des embeddings dans le RAG Server

## Les deux méthodes disponibles

Le projet propose **deux méthodes** pour générer les embeddings :

1. **TF-IDF Simple** (par défaut) - 100% C#, formule mathématique
2. **Sentence Transformers** (optionnel) - Modèle neuronal via Python

---

## 1️⃣ TF-IDF Simple (par défaut, 100% C#)

**Fichier** : `src/Mo5.RagServer.Infrastructure/Services/SimpleTfIdfEmbeddingService.cs`

### C'est quoi ?

Une **formule mathématique** pure, implémentée en C#.

**Pas de NuGet externe**, juste des calculs mathématiques ! 🎯

### Comment ça marche ?

#### Étape 1 : Construction du vocabulaire

Au démarrage, le système lit tous les chunks et construit un **vocabulaire** (liste de tous les mots uniques).

```csharp
// Construire le vocabulaire
var allTerms = new HashSet<string>();
foreach (var text in texts)
{
    var terms = TokenizeAndFilter(text);
    foreach (var term in terms)
    {
        allTerms.Add(term);
    }
}
```

**Exemple** :

```
Document 1 : "Le registre A est un accumulateur"
Document 2 : "Le registre X est un index"

Vocabulaire : ["registre", "accumulateur", "index"]
```

Les **mots vides** (le, un, est, etc.) sont filtrés automatiquement.

#### Étape 2 : Calcul des scores IDF

Pour chaque mot du vocabulaire, on calcule son **IDF** (Inverse Document Frequency).

**Formule** : `IDF = log(nombre total de documents / nombre de documents contenant le mot)`

```csharp
// Calculer les scores IDF
var totalDocuments = documentTerms.Count;
foreach (var term in _vocabulary.Keys)
{
    var documentsContainingTerm = documentTerms.Count(doc => doc.Contains(term));
    _idfScores[term] = Math.Log((double)totalDocuments / (1 + documentsContainingTerm));
}
```

**Exemple** :

```
Total de documents : 100

Mot "registre" : présent dans 80 documents
→ IDF = log(100 / 80) = 0.22  (mot commun, score faible)

Mot "accumulateur" : présent dans 5 documents
→ IDF = log(100 / 5) = 3.00  (mot rare, score élevé)
```

**Principe** : Les mots rares sont plus importants pour le sens.

#### Étape 3 : Génération de l'embedding pour un texte

Pour chaque chunk, on calcule le **TF-IDF** de chaque mot.

**Formule** : `TF-IDF = (fréquence du mot dans le texte / nombre total de mots) × IDF`

```csharp
// Calculer TF-IDF pour chaque terme du vocabulaire
var totalTerms = terms.Count;
foreach (var kvp in _vocabulary)
{
    var term = kvp.Key;
    var index = kvp.Value;

    if (termFrequency.ContainsKey(term))
    {
        var tf = (double)termFrequency[term] / totalTerms;
        var idf = _idfScores.GetValueOrDefault(term, 0);
        vector[index] = (float)(tf * idf);
    }
}
```

**Exemple** :

```
Texte : "Le registre A est un accumulateur 8 bits"

Mots après filtrage : ["registre", "accumulateur", "bits"]

Calcul TF-IDF :
- "registre" : TF = 1/3 = 0.33, IDF = 0.22 → TF-IDF = 0.07
- "accumulateur" : TF = 1/3 = 0.33, IDF = 3.00 → TF-IDF = 1.00
- "bits" : TF = 1/3 = 0.33, IDF = 1.50 → TF-IDF = 0.50

Vecteur : [0.07, 1.00, 0.50, 0, 0, 0, ..., 0]  (384 dimensions)
```

#### Étape 4 : Normalisation

Le vecteur est **normalisé** pour que sa longueur soit 1.

**Formule** : `vecteur normalisé = vecteur / magnitude`

```csharp
// Normaliser le vecteur
var magnitude = Math.Sqrt(vector.Sum(x => x * x));
if (magnitude > 0)
{
    for (int i = 0; i < vector.Length; i++)
    {
        vector[i] = (float)(vector[i] / magnitude);
    }
}
```

**Pourquoi normaliser ?**

Pour que tous les vecteurs aient la même "longueur", et qu'on puisse les comparer équitablement.

**Résultat** : Un vecteur de **384 nombres** (float) qui représente le texte.

### Avantages et inconvénients

**✅ Avantages** :
- 100% C#, pas de dépendance externe
- Très rapide
- Fonctionne hors ligne
- Pas de configuration complexe

**❌ Inconvénients** :
- Basé sur les mots-clés (pas de compréhension sémantique profonde)
- Ne comprend pas les synonymes automatiquement
- Moins performant que les modèles neuronaux

---

## 2️⃣ Sentence Transformers (optionnel, via Python)

**Fichier** : `src/Mo5.RagServer.Infrastructure/Services/LocalEmbeddingService.cs`

### C'est quoi ?

Un **modèle neuronal** (réseau de neurones) qui comprend le sens du texte.

**Pas de NuGet**, mais utilise **Python** + bibliothèque `sentence-transformers`.

### Comment ça marche ?

#### Étape 1 : Vérification de Python

Le service vérifie que Python est installé sur le système.

```csharp
// Vérifier que Python est disponible
if (!IsPythonAvailable())
{
    throw new InvalidOperationException("Python is not available. Please install Python 3.8+ and ensure it's in PATH.");
}
```

Si Python n'est pas trouvé, le service retourne une erreur.

#### Étape 2 : Installation des dépendances Python

Si nécessaire, le service installe automatiquement les packages Python requis :
- `sentence-transformers` - Bibliothèque de modèles d'embeddings
- `torch` - Framework de deep learning (PyTorch)
- `numpy` - Calculs numériques

```csharp
private void EnsurePythonDependencies()
{
    var requiredPackages = new[] { "sentence-transformers", "torch", "numpy" };

    foreach (var package in requiredPackages)
    {
        if (!IsPythonPackageInstalled(package))
        {
            _logger.LogInformation("Installing Python package: {Package}", package);
            InstallPythonPackage(package);
        }
    }
}
```

**Note** : L'installation se fait automatiquement au premier démarrage (peut prendre quelques minutes).

#### Étape 3 : Génération de l'embedding via Python

Le service C# génère un **script Python** et l'exécute.

**Script Python généré** :

```python
import json
import sys
from sentence_transformers import SentenceTransformer

try:
    # Load model (cached after first use)
    model = SentenceTransformer('paraphrase-multilingual-MiniLM-L12-v2')

    # Input texts
    texts = ["Le registre A est un accumulateur"]

    # Generate embeddings
    embeddings = model.encode(texts, convert_to_numpy=True)

    # Convert to list and output as JSON
    result = embeddings.tolist()
    print(json.dumps(result))

except Exception as e:
    print(f'Error: {e}', file=sys.stderr)
    sys.exit(1)
```

**Modèle utilisé** : `paraphrase-multilingual-MiniLM-L12-v2`
- Multilingue (français, anglais, etc.)
- Optimisé pour la paraphrase (comprend les synonymes)
- 384 dimensions
- Taille : ~420 MB (téléchargé au premier usage)

#### Étape 4 : Récupération du résultat

Le service C# récupère le JSON retourné par Python et le désérialise.

```csharp
private async Task<float[]> GenerateEmbeddingInternalAsync(string text, CancellationToken cancellationToken)
{
    var pythonScript = CreatePythonScript(new[] { text });
    var result = await ExecutePythonScriptAsync(pythonScript, cancellationToken);

    var embeddings = JsonSerializer.Deserialize<float[][]>(result);
    return embeddings?[0] ?? throw new InvalidOperationException("Failed to parse embedding result");
}
```

**Résultat** : Un vecteur de **384 nombres** (float) généré par le modèle neuronal.

### Comment fonctionne le modèle neuronal ?

**C'est un réseau de neurones** entraîné sur des millions de phrases.

**Principe** :
1. Le texte est découpé en tokens (mots ou sous-mots)
2. Chaque token passe dans plusieurs couches de neurones
3. Le modèle "comprend" le contexte et le sens
4. Il produit un vecteur de 384 dimensions

**Exemple** :

```
Texte : "Le registre A est un accumulateur"

Tokens : ["Le", "registre", "A", "est", "un", "accumulateur"]
    ↓
Réseau de neurones (12 couches)
    ↓
Vecteur : [0.23, -0.45, 0.12, ..., 0.67]  (384 dimensions)
```

**Magie** : Le modèle comprend que "registre accumulateur" et "accumulator register" ont le même sens, même si les mots sont différents !

### Avantages et inconvénients

**✅ Avantages** :
- Compréhension sémantique profonde
- Comprend les synonymes automatiquement
- Multilingue (français, anglais, etc.)
- Qualité supérieure pour la recherche

**❌ Inconvénients** :
- Nécessite Python + dépendances
- Plus lent (surtout au premier démarrage)
- Télécharge un modèle de ~420 MB
- Plus complexe à configurer

---

## 📊 Comparaison des deux méthodes

| Critère | TF-IDF Simple | Sentence Transformers |
|---------|---------------|----------------------|
| **Technologie** | Formule mathématique | Réseau de neurones |
| **Langage** | 100% C# | C# + Python |
| **Dépendances** | Aucune | Python + packages |
| **Qualité** | Bonne (mots-clés) | Excellente (sémantique) |
| **Vitesse** | Très rapide | Plus lent (1ère fois) |
| **Compréhension** | Basée sur les mots | Comprend le sens |
| **Multilingue** | Oui (avec filtres) | Oui (modèle multilingue) |
| **Taille** | ~50 KB de code | ~420 MB de modèle |
| **Configuration** | Aucune | Python 3.8+ requis |

---

## 🎯 Quelle méthode utiliser ?

### Utilisez TF-IDF Simple si :

- Vous voulez une solution **simple et rapide**
- Vous n'avez pas Python installé
- Vous développez sur un environnement contraint (Raspberry Pi, etc.)
- Votre documentation utilise des termes techniques précis

### Utilisez Sentence Transformers si :

- Vous voulez la **meilleure qualité** de recherche
- Vous avez Python installé (ou pouvez l'installer)
- Vous avez de l'espace disque (~500 MB)
- Votre documentation contient des synonymes, paraphrases, etc.

---

## 🔧 Configuration

### TF-IDF Simple (par défaut)

**Aucune configuration nécessaire !**

Le service est enregistré par défaut dans `Program.cs` :

```csharp
builder.Services.AddSingleton<IEmbeddingService, SimpleTfIdfEmbeddingService>();
```

### Sentence Transformers (optionnel)

**1. Installer Python 3.8+**

```bash
# Vérifier la version
python --version
```

**2. Modifier `Program.cs`**

Remplacer :

```csharp
builder.Services.AddSingleton<IEmbeddingService, SimpleTfIdfEmbeddingService>();
```

Par :

```csharp
builder.Services.AddSingleton<IEmbeddingService, LocalEmbeddingService>();
```

**3. (Optionnel) Configurer le modèle dans `appsettings.json`**

```json
{
  "LocalEmbedding": {
    "ModelName": "paraphrase-multilingual-MiniLM-L12-v2",
    "PythonPath": "python"
  }
}
```

**4. Démarrer le serveur**

Au premier démarrage, les dépendances Python seront installées automatiquement.

---

## 🧪 Exemple concret

### Avec TF-IDF Simple

**Texte** : "Le registre A est un accumulateur 8 bits"

**Processus** :
1. Filtrage : ["registre", "accumulateur", "bits"]
2. Calcul TF : registre=0.33, accumulateur=0.33, bits=0.33
3. Calcul IDF : registre=0.22, accumulateur=3.00, bits=1.50
4. TF-IDF : [0.07, 1.00, 0.50, 0, 0, ..., 0]
5. Normalisation : [0.05, 0.71, 0.35, 0, 0, ..., 0]

**Résultat** : Vecteur de 384 dimensions

### Avec Sentence Transformers

**Texte** : "Le registre A est un accumulateur 8 bits"

**Processus** :
1. Tokenisation : ["Le", "registre", "A", "est", "un", "accumulateur", "8", "bits"]
2. Passage dans le réseau de neurones (12 couches)
3. Agrégation des représentations
4. Normalisation

**Résultat** : Vecteur de 384 dimensions (valeurs différentes de TF-IDF)

---

## 💬 Résumé

### TF-IDF Simple

**C'est une formule mathématique** :
1. On compte la fréquence des mots (TF)
2. On calcule la rareté des mots (IDF)
3. On multiplie les deux (TF × IDF)
4. On normalise le vecteur

**Pas de NuGet externe**, juste du code C# avec des calculs mathématiques.

### Sentence Transformers

**C'est un modèle neuronal** :
1. Le texte est passé dans un réseau de neurones
2. Le modèle a été entraîné sur des millions de phrases
3. Il comprend le **sens** du texte, pas juste les mots
4. Il retourne un vecteur de 384 dimensions

**Utilise Python** + bibliothèque `sentence-transformers` (qui elle-même utilise PyTorch).

---

**Les deux méthodes produisent des vecteurs de 384 nombres qui peuvent être comparés avec la similarité cosinus.** 🚀



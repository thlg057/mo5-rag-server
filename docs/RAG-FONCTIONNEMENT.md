# 🧠 Comment fonctionne le RAG Server

> Une explication simple du fonctionnement métier du serveur RAG pour la documentation Thomson MO5

## Le problème de départ

Quand on développe pour le Thomson MO5, on a besoin de consulter beaucoup de documentation :
- Les instructions du processeur 6809
- La cartographie mémoire
- Les registres vidéo
- Des exemples de code

Le problème, c'est qu'on ne sait pas toujours **où** chercher exactement.

On se retrouve à ouvrir 10 fichiers différents, à faire des recherches par mots-clés, à espérer tomber sur le bon passage...

**Et si on pouvait simplement poser une question et obtenir les passages pertinents ?**

C'est exactement ce que fait le RAG Server. 🎯

## L'idée générale

Le RAG Server, c'est comme avoir un **bibliothécaire expert** qui connaît toute la documentation par cœur.

Vous lui posez une question :
> "Comment utiliser le registre X pour l'indexation ?"

Et il vous apporte directement les passages pertinents de la documentation, même si vous n'avez pas utilisé les mots exacts.

**Pas de recherche manuelle, pas de mots-clés à deviner.**

Le système comprend le **sens** de votre question et trouve les réponses.

## Les concepts clés (avant d'aller plus loin)

Avant de détailler comment ça marche, il faut comprendre quelques termes.

Pas de panique, je vais expliquer simplement. 😉

### RAG (Retrieval-Augmented Generation)

**RAG** = Recherche + Génération augmentée

En gros :
1. On **recherche** les passages pertinents dans la documentation
2. On les **fournit** à une IA (comme ChatGPT) pour qu'elle génère une réponse

Dans notre cas, on s'occupe surtout de la partie **recherche**.

### Chunk (morceau)

Un **chunk**, c'est un morceau de texte découpé d'un document.

Pourquoi découper ?
- Les documents sont trop longs pour être traités d'un coup
- On veut trouver **précisément** le passage qui répond à la question
- Pas tout le document, juste la partie utile

**Exemple** :

Un document de 5000 caractères sera découpé en 5 chunks de ~1000 caractères chacun.

Chaque chunk = une "carte de visite" d'un concept.

### Embedding (empreinte numérique)

Un **embedding**, c'est une représentation numérique du **sens** d'un texte.

Concrètement, c'est un vecteur (une liste) de 384 nombres.

**Pourquoi faire ça ?**

Parce qu'on ne peut pas comparer du texte directement.

Comment savoir que "registre accumulateur" et "accumulator register" parlent de la même chose ?

Avec les embeddings, on transforme le texte en nombres, et on peut calculer la **similarité** entre deux textes.

**Exemple** :

```
"registre accumulateur"     → [0.5, 0.3, 0.1, ..., 0.2]
"accumulator register"      → [0.52, 0.28, 0.12, ..., 0.19]  ← PROCHE !
"mémoire vidéo"             → [-0.2, 0.8, -0.5, ..., 0.6]   ← LOIN
```

Les textes similaires ont des embeddings proches.

### TF-IDF (la technique d'embedding)

**TF-IDF** = Term Frequency - Inverse Document Frequency

C'est la technique qu'on utilise pour générer les embeddings.

En gros :
- **TF** (Term Frequency) : À quelle fréquence un mot apparaît dans le texte ?
- **IDF** (Inverse Document Frequency) : Est-ce que ce mot est rare ou commun dans tous les documents ?

Un mot rare et présent dans le texte = important pour le sens.

**Avantage** : Tout est calculé **localement**, pas besoin d'appeler une API externe (OpenAI, etc.).

### Similarité cosinus

C'est la méthode pour comparer deux embeddings.

On calcule un **score de 0 à 1** :
- **0** = complètement différent
- **1** = identique

Plus le score est élevé, plus les textes sont similaires.

### pgvector (la base de données vectorielle)

**pgvector** est une extension de PostgreSQL qui permet de stocker des vecteurs (embeddings).

Elle offre aussi des **index spéciaux** (IVFFlat) pour faire des recherches vectorielles ultra-rapides.

Au lieu de comparer manuellement avec tous les chunks (lent), l'index permet de trouver rapidement les plus proches.

---

💬 **En résumé** : On découpe les documents en chunks, on transforme chaque chunk en embedding (vecteur de nombres), et on stocke tout ça dans PostgreSQL avec pgvector. Quand on pose une question, on la transforme aussi en embedding, et on cherche les chunks les plus proches.

---

## Comment ça marche concrètement ?

Maintenant qu'on a les bases, voyons comment le système fonctionne au quotidien.

Il y a trois grandes étapes :
1. **Ingestion** : Remplir la base de connaissances
2. **Surveillance** : Détecter les changements
3. **Recherche** : Répondre aux questions

### Étape 1 : Ingestion (remplir la bibliothèque)

Au démarrage du serveur, il va lire tous les fichiers Markdown dans le dossier `/knowledge`.

**Voici ce qui se passe pour chaque fichier** :

#### 1.1 - Lecture du fichier

Le serveur lit le contenu du fichier.

Exemple : `guide-6809.md`

```markdown
# Motorola 6809 - Registres

Le processeur 6809 possède plusieurs registres :
- A et B : registres accumulateurs 8 bits
- D : registre 16 bits (combinaison de A et B)
- X et Y : registres d'index 16 bits
- U et S : pointeurs de pile 16 bits
- PC : compteur de programme 16 bits

## Utilisation des registres

Le registre A est utilisé pour...
```

#### 1.2 - Découpage en chunks

Le document est découpé en morceaux de ~1000 caractères.

**Stratégie de découpage** :
- On respecte les sections Markdown (titres `#`, `##`, etc.)
- On évite de couper au milieu d'une phrase
- On ajoute un **chevauchement** de 200 caractères entre les chunks

**Pourquoi un chevauchement ?**

Pour ne pas perdre le contexte entre deux chunks.

Si un chunk se termine par "...le registre X est utilisé pour", le chunk suivant commencera par "le registre X est utilisé pour l'indexation...".

Comme ça, on ne coupe pas les concepts en deux.

**Résultat** :

```
📦 Chunk 0 : "# Motorola 6809 - Registres\n\nLe processeur 6809..."
📦 Chunk 1 : "## Utilisation des registres\n\nLe registre A..."
📦 Chunk 2 : "## Modes d'adressage\n\nLe 6809 supporte..."
...
```

Chaque chunk garde aussi des **métadonnées** :
- Position dans le document (début, fin)
- Titre de la section (`# Registres`)
- Nombre de tokens estimés
- Index du chunk (0, 1, 2, ...)

#### 1.3 - Génération des embeddings

Pour chaque chunk, on génère un **embedding** (vecteur de 384 nombres).

C'est fait avec **TF-IDF**, une technique locale (pas d'API externe).

**Comment ça marche ?**

1. On construit un **vocabulaire** à partir de tous les documents
2. Pour chaque chunk, on calcule l'importance de chaque mot
3. On obtient un vecteur de 384 dimensions

**Exemple** :

```
Chunk : "Le registre A est un accumulateur 8 bits"
    ↓
Embedding : [0.23, -0.45, 0.12, ..., 0.67]  (384 nombres)
```

#### 1.4 - Stockage dans PostgreSQL

Tout est stocké dans la base de données PostgreSQL avec l'extension **pgvector**.

**Structure** :

```
📊 Table "Documents"
- Id : UUID unique
- FileName : "guide-6809.md"
- FilePath : "/knowledge/cpu/guide-6809.md"
- Title : "Guide du Motorola 6809"
- Content : texte complet du document
- ContentHash : empreinte du contenu (pour détecter les modifications)
- CreatedAt, UpdatedAt : dates
- IsActive : true/false

📊 Table "DocumentChunks"
- Id : UUID unique
- DocumentId : lien vers le document parent
- ChunkIndex : 0, 1, 2, ...
- Content : "Le registre A est un accumulateur..."
- Embedding : vecteur de 384 dimensions ⭐
- StartPosition, EndPosition : position dans le document
- SectionHeading : "# Registres"
- TokenCount : nombre de tokens estimés
- CreatedAt : date de création
```

L'index **IVFFlat** sur la colonne `Embedding` permet de faire des recherches vectorielles ultra-rapides.

#### 1.5 - Détection des tags

Le système analyse le contenu du document et détecte automatiquement des **tags**.

Exemples de tags : "CPU", "Mémoire", "Vidéo", "Assembleur", etc.

Ces tags permettent de filtrer les résultats de recherche plus tard.

**Résultat** :

```
📄 Document "guide-6809.md"
   ├── 🏷️ Tag "CPU"
   ├── 🏷️ Tag "Registres"
   └── 🏷️ Tag "6809"
```

💬 **En résumé** : Pour chaque fichier, on lit le contenu, on découpe en chunks, on génère les embeddings, et on stocke tout dans PostgreSQL avec des tags.

---

### Étape 2 : Surveillance (détecter les changements)

Une fois l'ingestion initiale terminée, le serveur ne s'arrête pas là.

Il surveille en permanence le dossier `/knowledge` pour détecter les changements.

**Comment ça marche ?**

Un **File Watcher** (surveillant de fichiers) observe le dossier.

Dès qu'un fichier est créé, modifié, supprimé ou renommé, il déclenche une action.

**Exemple** :

```
📁 /knowledge/cpu/guide-6809.md
    ↓ (vous modifiez le fichier)
🔔 Événement "FileChanged" détecté
    ↓
⏱️  Attente de 2 secondes (batch processing)
    ↓
🔄 Ré-indexation automatique du fichier
    ↓
✅ Base de données mise à jour
```

**Pourquoi attendre 2 secondes ?**

Pour **grouper** les changements.

Si vous modifiez le fichier 10 fois en 2 secondes (sauvegarde automatique de l'éditeur, par exemple), le système ne va pas ré-indexer 10 fois.

Il attend que ça se calme, puis traite une seule fois.

**Optimisations** :
- **Batching** : Grouper les changements
- **Déduplication** : Si le même fichier change plusieurs fois, traiter une seule fois
- **Hash de contenu** : Ne ré-indexer que si le contenu a vraiment changé (pas juste la date de modification)

💬 **En résumé** : Le serveur surveille le dossier `/knowledge` en permanence. Dès qu'un fichier change, il le ré-indexe automatiquement. Pas besoin de redémarrer le serveur.

---

### Étape 3 : Recherche (répondre aux questions)

C'est là que la magie opère. 🎩

Vous posez une question via l'API REST, et le système trouve les passages pertinents.

**Exemple de question** :

> "Comment utiliser le registre X pour l'indexation ?"

**Voici ce qui se passe** :

#### 3.1 - Génération de l'embedding de la question

La question est transformée en embedding, exactement comme les chunks.

```
Question : "Comment utiliser le registre X pour l'indexation ?"
    ↓
Embedding : [0.45, -0.23, 0.67, ..., 0.12]  (384 dimensions)
```

#### 3.2 - Recherche vectorielle

Le système compare l'embedding de la question avec **tous** les embeddings des chunks dans la base.

Il calcule la **similarité cosinus** entre la question et chaque chunk.

```
Comparaison avec tous les chunks :
- Chunk 1 : 0.92 ⭐ (très pertinent)
- Chunk 2 : 0.87 ⭐ (pertinent)
- Chunk 3 : 0.45 (peu pertinent)
- Chunk 4 : 0.12 (pas pertinent)
- ...
```

**Comment c'est rapide ?**

Grâce à l'index **IVFFlat** de pgvector.

Au lieu de comparer avec tous les chunks un par un (lent), l'index permet de trouver rapidement les plus proches.

#### 3.3 - Filtrage (optionnel)

On peut filtrer les résultats par **tags**.

Par exemple, si vous cherchez uniquement dans la documentation CPU :

```
Filtrer par tags : ["CPU", "Registres"]
```

Seuls les chunks des documents ayant ces tags seront considérés.

On peut aussi filtrer par **document actif** (`IsActive = true`).

#### 3.4 - Tri et limitation

Les chunks sont triés par **score de similarité** (du plus pertinent au moins pertinent).

On prend les **top N** résultats (par exemple, les 5 meilleurs).

#### 3.5 - Retour des résultats

Le système retourne un JSON avec les chunks les plus pertinents.

**Exemple de réponse** :

```json
[
  {
    "content": "Le registre X est utilisé pour l'indexation...",
    "score": 0.92,
    "documentTitle": "Guide du 6809",
    "sectionHeading": "Registres d'index",
    "tags": ["CPU", "Registres"]
  },
  {
    "content": "Exemple d'utilisation du registre X...",
    "score": 0.87,
    "documentTitle": "Exemples de code",
    "sectionHeading": "Indexation",
    "tags": ["CPU", "Exemples"]
  },
  ...
]
```

💬 **En résumé** : La question est transformée en embedding, comparée avec tous les chunks, et les plus pertinents sont retournés. Tout ça en quelques millisecondes grâce à l'index pgvector.



---

## Un exemple complet de bout en bout

Pour bien comprendre, voici un scénario complet.

### Scénario : J'ajoute un nouveau document

Vous venez d'écrire un nouveau fichier de documentation sur l'affichage vidéo du MO5.

Vous le copiez dans `/knowledge/video/ecran-mo5.md`.

**Voici ce qui se passe automatiquement** :

```
1️⃣ DÉTECTION
   📄 /knowledge/video/ecran-mo5.md (nouveau fichier)
       ↓
   🔔 File Watcher détecte la création

2️⃣ INGESTION
   📖 Lecture du contenu (3500 caractères)
   ✂️  Découpage en 4 chunks de ~1000 caractères
   🧮 Génération de 4 embeddings (TF-IDF)
   💾 Insertion dans PostgreSQL
       ↓
   ✅ 4 nouveaux chunks dans la base

3️⃣ DÉTECTION DE TAGS
   🏷️  Analyse du contenu
   🏷️  Tags détectés : "Vidéo", "Écran", "Mémoire"
   💾 Association Document ↔ Tags

4️⃣ MISE À JOUR DU VOCABULAIRE
   📚 Mise à jour du vocabulaire TF-IDF global
   🔄 Régénération de TOUS les embeddings
       ↓
   ✅ Base de données cohérente
```

**Résultat** : Votre nouveau document est immédiatement disponible pour la recherche.

Pas besoin de redémarrer le serveur, pas de commande manuelle à lancer.

### Scénario : Je pose une question

Vous développez un jeu et vous voulez afficher un pixel à l'écran.

Vous posez la question à l'API :

> "Comment afficher un pixel à l'écran ?"

**Voici ce qui se passe** :

```
1️⃣ GÉNÉRATION EMBEDDING QUESTION
   "Comment afficher un pixel à l'écran ?"
       ↓
   [0.34, -0.56, 0.78, ..., 0.21]  (384 dimensions)

2️⃣ RECHERCHE VECTORIELLE
   🔍 Comparaison avec tous les chunks (disons 150)
   📊 Calcul des scores de similarité
       ↓
   Résultats triés par score

3️⃣ TOP 5 RÉSULTATS
   ⭐ Chunk 42 (score: 0.94) - "Affichage pixel par pixel"
   ⭐ Chunk 15 (score: 0.89) - "Mémoire vidéo du MO5"
   ⭐ Chunk 67 (score: 0.85) - "Modes graphiques"
   ⭐ Chunk 23 (score: 0.82) - "Palette de couleurs"
   ⭐ Chunk 91 (score: 0.78) - "Exemples de code graphique"

4️⃣ RETOUR À L'UTILISATEUR
   📄 JSON avec les 5 chunks + métadonnées
```

**Résultat** : Vous obtenez les 5 passages les plus pertinents de la documentation.

Même si vous n'avez pas utilisé les mots exacts ("pixel", "affichage"), le système a compris le sens de votre question.

---

## Pourquoi c'est mieux qu'une recherche par mots-clés ?

Vous vous demandez peut-être : "Pourquoi ne pas juste faire une recherche par mots-clés ?"

Bonne question. 😉

**Avec une recherche par mots-clés** :

Vous cherchez "registre accumulateur".

Le système trouve uniquement les documents contenant **exactement** ces mots.

Si un document parle de "accumulator register" (en anglais), il ne sera pas trouvé.

Si un document parle de "registre A" sans mentionner "accumulateur", il ne sera pas trouvé non plus.

**Avec une recherche sémantique (embeddings)** :

Vous cherchez "registre accumulateur".

Le système comprend le **sens** de votre question.

Il trouve :
- Les documents parlant de "accumulator register" (même concept)
- Les documents parlant de "registre A" (c'est un accumulateur)
- Les documents parlant de "registres 8 bits" (contexte similaire)

**Résultat** : Vous trouvez beaucoup plus de résultats pertinents, même si les mots exacts ne sont pas présents.

---

## Les avantages du système

### ✅ Recherche sémantique

Le système comprend le **sens**, pas juste les mots-clés.

Vous pouvez poser des questions naturelles, comme si vous parliez à quelqu'un.

### ✅ Automatique

Dès que vous ajoutez ou modifiez un fichier dans `/knowledge`, il est automatiquement indexé.

Pas besoin de redémarrer le serveur, pas de commande manuelle.

### ✅ Local

Tout est calculé **localement** avec TF-IDF.

Pas besoin d'appeler une API externe (OpenAI, etc.).

Pas de coût, pas de dépendance, pas de problème de confidentialité.

### ✅ Rapide

Grâce à l'index **IVFFlat** de pgvector, les recherches sont ultra-rapides.

Même avec des milliers de chunks, la réponse arrive en quelques millisecondes.

### ✅ Évolutif

Vous pouvez ajouter autant de documents que vous voulez.

Le système s'adapte automatiquement.

---

## Configuration et maintenance

### Paramètres ajustables

Vous pouvez configurer plusieurs paramètres dans `appsettings.json` :

- **Taille des chunks** : 1000 caractères (défaut)
- **Overlap** : 200 caractères (défaut)
- **Nombre de résultats** : configurable par requête (ex: top 5, top 10)
- **Filtrage par tags** : optionnel

### Opérations courantes

**Ajouter un document** :
- Copiez le fichier Markdown dans `/knowledge`
- C'est tout ! Le système l'indexe automatiquement

**Modifier un document** :
- Éditez le fichier dans `/knowledge`
- Le système détecte le changement et ré-indexe automatiquement

**Supprimer un document** :
- Supprimez le fichier de `/knowledge`
- Le système marque le document comme inactif dans la base

**Réinitialiser complètement** :
- Videz la base de données
- Redémarrez le serveur
- L'ingestion initiale se relance automatiquement

---

## Cas d'usage concrets

Pour finir, voici quelques exemples de questions que vous pourriez poser au système.

### Cas 1 : Développeur débutant MO5

**Question** : "Comment initialiser le processeur 6809 ?"

**Résultats attendus** :
- Chunk 1 : "Séquence de démarrage du 6809"
- Chunk 2 : "Initialisation des registres"
- Chunk 3 : "Vecteur de reset"
- Chunk 4 : "Exemple de code d'initialisation"

### Cas 2 : Développeur expérimenté

**Question** : "Optimisation des accès mémoire en mode direct"

**Résultats attendus** :
- Chunk 1 : "Mode d'adressage direct vs étendu"
- Chunk 2 : "Performance des instructions"
- Chunk 3 : "Techniques d'optimisation"
- Chunk 4 : "Exemples de code optimisé"

### Cas 3 : Documentation technique

**Question** : "Registres du contrôleur vidéo"

**Résultats attendus** :
- Chunk 1 : "Cartographie mémoire vidéo"
- Chunk 2 : "Registres de configuration"
- Chunk 3 : "Modes graphiques disponibles"
- Chunk 4 : "Exemples de programmation vidéo"

---

## Résumé

Pour résumer tout ça en quelques points :

### Le principe

Le RAG Server transforme la documentation en une base de connaissances **recherchable sémantiquement**.

Au lieu de chercher par mots-clés, vous posez des questions naturelles et le système trouve les passages pertinents.

### Les 3 étapes

1. **Ingestion** : Les documents sont découpés en chunks et transformés en embeddings
2. **Surveillance** : Les modifications de fichiers déclenchent une ré-indexation automatique
3. **Recherche** : Les questions sont comparées aux chunks pour trouver les plus pertinents

### Les avantages

- ✅ Recherche sémantique (comprend le sens)
- ✅ Automatique (pas de commande manuelle)
- ✅ Local (pas d'API externe)
- ✅ Rapide (index pgvector)
- ✅ Évolutif (ajoutez autant de documents que vous voulez)

### L'utilisation

**Ajouter un document** : Copiez-le dans `/knowledge`

**Poser une question** : Appelez l'API REST

**Obtenir les résultats** : Les chunks les plus pertinents vous sont retournés

---

## Glossaire

Pour référence, voici les termes techniques utilisés dans ce document :

| Terme | Définition |
|-------|------------|
| **RAG** | Retrieval-Augmented Generation - Recherche + Génération augmentée |
| **Chunk** | Morceau de texte découpé d'un document (~1000 caractères) |
| **Embedding** | Vecteur numérique représentant le sens d'un texte (384 dimensions) |
| **TF-IDF** | Term Frequency - Inverse Document Frequency (technique d'embedding locale) |
| **pgvector** | Extension PostgreSQL pour stocker et rechercher des vecteurs |
| **Similarité cosinus** | Mesure de proximité entre deux vecteurs (score de 0 à 1) |
| **IVFFlat** | Type d'index pour accélérer les recherches vectorielles |
| **File Watcher** | Service qui surveille les modifications de fichiers |
| **Batch processing** | Traitement groupé des changements (optimisation) |
| **Overlap** | Chevauchement entre les chunks (200 caractères par défaut) |

---

💬 **En résumé** : Le RAG Server, c'est une bibliothèque intelligente pour la documentation Thomson MO5. Vous posez une question, il trouve les passages pertinents. Simple, rapide, et tout en local. 🚀


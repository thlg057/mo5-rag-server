# Choisir un format RLE pour les assets Thomson MO5

## Goal

Comparer plusieurs variantes de RLE et choisir la plus adaptée aux assets d’un jeu MO5.

## Rappel : principe du RLE

Au lieu de stocker chaque octet séparément, on stocke des **séries de répétitions**.

Exemple :

```
Données brutes  →  Encodage RLE
AA AA AA AA AA  →  05 AA       ("répéter AA 5 fois")
```

Le gain dépend fortement de la présence de longues séries identiques.

## Variante 1 : RLE simple `(count, value)`

Format :

- Encodage : `[count] [value] [count] [value] ...`
- Exemple : `05 AA 03 BB 01 CC` → `AA AA AA AA AA BB BB BB CC`

Avantages :
- Décodeur **très simple** à écrire.
- Idéal pour des données avec de longs runs de mêmes octets.

Inconvénients :
- Pour des données peu répétitives, chaque octet devient une paire `(1, value)` → **2 octets au lieu de 1**.
- Le format peut donc **agrandir** les données.

## Variante 2 : RLE avec mode littéral (recommandée)

Idée : distinguer deux types de blocs :
- **Bloc répétition** : répéter une valeur.
- **Bloc littéral** : copier tel quel une séquence de valeurs.

On utilise un bit de poids fort pour indiquer le mode :

- Si bit7 du compteur = 0 → **répétition** : `[0b0_count] [value]`.
- Si bit7 du compteur = 1 → **littéral** : `[0b1_count] [val1] [val2] ...`.

Exemple :

```
Encodage : 05 AA  84 1B 2C 3D 4E  03 FF
Décodé   : AA AA AA AA AA  1B 2C 3D 4E  FF FF FF
```

Avantages :
- Les séquences non répétitives sont stockées en **littéral** avec un seul octet d’overhead pour jusqu’à 127 octets.
- Très bon compromis taille / simplicité pour des données mixtes (aplats + détails).

Inconvénients :
- Décodeur un peu plus complexe que pour le RLE simple.

## Variante 3 : token de fin de flux

On peut réserver une séquence impossible à générer autrement comme **fin de données**, par exemple :

- `00` (ou `00 00` selon le format) pour marquer la fin du stream.

Avantages :
- Évite de devoir passer la taille décompressée à la fonction de décodage.
- Simplifie certaines boucles (décompression jusqu’au token de fin).

## Alternatives au RLE

Si les données restent trop grosses malgré le RLE, d’autres algorithmes existent :

| Algorithme            | Gain typique   | Complexité décodeur | Adapté au MO5 ? |
|-----------------------|----------------|----------------------|-----------------|
| RLE simple            | 30–70 %        | Très faible          | ✅ Oui          |
| RLE + littéraux       | 40–60 %        | Faible               | ✅ Oui          |
| LZ77 / LZSS           | 50–70 %        | Moyenne              | ⚠️ À évaluer    |
| Huffman               | 40–60 %        | Élevée               | ❌ Trop lourd   |

Pour la majorité des jeux MO5, le format **RLE avec littéraux + token de fin** est un très bon choix.

Source: `mo5-docs/mo5/compression-rle-MO5.md`


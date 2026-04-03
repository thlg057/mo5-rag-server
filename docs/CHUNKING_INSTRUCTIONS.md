# Instructions pour le découpage de documentation en chunks

Ce document explique comment découper de gros fichiers `.md` en chunks cohérents pour la base de connaissances MO5.

## Principe fondamental

Chaque fichier `.md` représente **une seule unité de connaissance autonome** : une procédure, une fonction, un pattern, ou un concept précis. L'objectif est d'optimiser la recherche sémantique.

## Structure de l'arborescence

L'arborescence encode les **mots-clés** de classification :

```
knowledge/
└── thomson/                    # Famille de machines
    └── mo5/                    # Machine spécifique
        ├── language/           # Fonctions CMOC, syntaxe C
        ├── memory/             # Gestion mémoire
        │   ├── layout/         # Sections, organisation mémoire
        │   └── stack/          # Pile et débordements
        ├── patterns/           # Patterns de jeu/application
        │   ├── collisions/     # Détection de collisions
        │   ├── enemies/        # IA ennemis
        │   ├── game-loop/      # Boucle principale
        │   ├── input/          # Gestion clavier
        │   ├── items/          # Objets collectables
        │   └── level-geometry/ # Géométrie des niveaux
        ├── system/             # Bas niveau système
        │   ├── abi/            # Conventions d'appel
        │   ├── interrupts/     # Interruptions
        │   ├── pia/            # Registres PIA
        │   └── timing/         # Synchronisation
        ├── toolchain/          # Compilation, linkage
        ├── video/              # Affichage
        │   ├── addressing/     # Calcul adresses VRAM
        │   ├── colors/         # Palette, attributs
        │   ├── drawing/        # Dessin primitives
        │   ├── initialization/ # Init video
        │   ├── sprites/        # Sprites
        │   └── text/           # Affichage texte
        └── sdk/                # API du SDK externe sdk_mo5
            ├── defs/           # Types de base, SWI I/O, constantes
            ├── ctype/          # Fonctions de classification de caractères
            ├── stdio/          # I/O texte de haut niveau
            ├── utils/          # Fonctions utilitaires (clamp, etc.)
            ├── video/          # Couche vidéo du SDK (PRC, VBL, palette)
            └── sprite/         # Sprites et acteurs du SDK
```

## Limite des mots-clés

**Maximum 3 niveaux de profondeur** après `knowledge/thomson/mo5/` :
- Niveau 1 : Domaine (`video`, `memory`, `system`, `language`, `toolchain`, `patterns`, `sdk`)
- Niveau 2 : Sous-domaine (optionnel, ex: `sprites`, `layout`, `abi`)
- Niveau 3 : INTERDIT - Ne pas créer de sous-sous-répertoires

## Convention de nommage des fichiers

Format : `action-sujet-details-contexte-thomson-mo5.md`

Exemples :
- `draw-sprite-32x32-1bpp-thomson-mo5.md`
- `compile-c-program-cmoc-thomson-mo5.md`
- `detect-aabb-collision-thomson-mo5.md`

Règles :
1. Commencer par un **verbe d'action** (`use`, `draw`, `set`, `compile`, `detect`, `implement`)
2. Mots séparés par des tirets
3. Finir par `-thomson-mo5.md` (ou `-cmoc-thomson-mo5.md` si spécifique CMOC)
4. Tout en minuscules
5. Nom descriptif mais concis (max ~60 caractères avant le suffixe)

## Taille idéale d'un chunk

- **15-50 lignes** de Markdown
- Chaque fichier doit être **auto-suffisant** : lisible sans contexte externe
- Un seul sujet par fichier

## Structure d'un fichier chunk

```markdown
# Titre action (Machine)

Description courte d'une ligne.

## Preconditions / Prerequisites (optionnel)

- Condition 1
- Condition 2

## Goal / Steps / Procedure

1. Étape 1
2. Étape 2

## C example / Assembly example (si applicable)

\`\`\`c
// code minimal et fonctionnel
\`\`\`

## Notes (optionnel)

- Remarques importantes

Source: `chemin/vers/source/originale.md`
```

## Règles de découpage d'un gros fichier

### 1. Identifier les unités logiques

Chercher les sections qui répondent à UNE question :
- "Comment faire X ?"
- "Qu'est-ce que Y ?"
- "Quelle est la procédure pour Z ?"

### 2. Créer un fichier par unité

Chaque section autonome devient un fichier séparé.

### 3. Choisir le bon répertoire

Placer le fichier selon son **sujet principal** :
- Code C/fonctions standard → `language/`
- Compilation/linkage → `toolchain/`
- Affichage/VRAM → `video/` + sous-répertoire approprié
- Mémoire/sections → `memory/`
- Matériel (PIA, interrupts) → `system/`
- Patterns réutilisables → `patterns/`

### 4. Éviter la duplication

Si un concept apparaît dans plusieurs contextes, créer UN seul fichier dans le répertoire le plus approprié.

## Mots-clés autorisés (liste fermée)

### Niveau 1 (obligatoire)
`language`, `memory`, `patterns`, `system`, `toolchain`, `video`, `sdk`

### Niveau 2 (optionnel, selon niveau 1)

| Niveau 1   | Niveau 2 autorisés                                           |
|------------|--------------------------------------------------------------|
| language   | *(pas de sous-répertoire)*                                   |
| memory     | `layout`, `stack`                                            |
| patterns   | `collisions`, `enemies`, `game-loop`, `input`, `items`, `level-geometry` |
| system     | `abi`, `interrupts`, `pia`, `timing`                         |
| toolchain  | *(pas de sous-répertoire)*                                   |
| video      | `addressing`, `colors`, `drawing`, `initialization`, `sprites`, `text` |
| sdk        | `defs`, `ctype`, `stdio`, `utils`, `video`, `sprite`         |

## Exemple de découpage

Fichier source : `mo5_graphics_guide.md` (500 lignes)

Chunks résultants :
1. `video/initialization/initialize-vram-banks-black-bg-white-fg-thomson-mo5.md`
2. `video/addressing/compute-vram-offset-40x200-thomson-mo5.md`
3. `video/colors/encode-color-attribute-ffffbbbb-thomson-mo5.md`
4. `video/sprites/draw-sprite-16x16-1bpp-thomson-mo5.md`
5. `video/sprites/draw-sprite-32x32-1bpp-thomson-mo5.md`
6. `video/drawing/fill-rectangle-vram-thomson-mo5.md`

Chaque fichier contient UNE procédure autonome avec son exemple de code.


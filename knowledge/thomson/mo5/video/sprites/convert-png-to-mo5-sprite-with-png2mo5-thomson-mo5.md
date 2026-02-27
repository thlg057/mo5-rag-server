# Convertir un PNG en sprite MO5 avec `png2mo5.py`

## Goal

Expliquer comment convertir un PNG en données de sprite exploitables par le SDK MO5 (`mo5_sprite.h/.c`).

## Commande Makefile

Le projet fournit une cible `make` pour convertir un PNG :

```bash
make convert IMG=./assets/sprite.png
```

Cette commande :
- Appelle automatiquement `png2mo5.py`.
- Génère un fichier `.h` (ex: `sprite.h`) à côté du PNG.

## Contenu du header généré

Pour `perso.png`, le script génère typiquement `perso.h` :

```c
// Dimensions
#define SPRITE_PERSO_WIDTH_BYTES 4
#define SPRITE_PERSO_HEIGHT      32

// Données brutes (à ne pas modifier)
unsigned char sprite_perso_form[128]  = { ... };
unsigned char sprite_perso_color[128] = { ... };

// Macro d'initialisation à utiliser dans le code
#define SPRITE_PERSO_INIT \
    { sprite_perso_form, sprite_perso_color, \
      SPRITE_PERSO_WIDTH_BYTES, SPRITE_PERSO_HEIGHT }
```

Dans le code C :

```c
#include "assets/perso.h"

MO5_Sprite spr_perso = SPRITE_PERSO_INIT;
```

## Contraintes sur le PNG source

- Largeur **multiple de 8 pixels** (sinon elle est ajustée).
- Maximum 16 couleurs au total (palette MO5).
- **2 couleurs maxi par groupe de 8 pixels horizontaux** (contrainte hardware MO5).
- Le fond transparent est converti en couleur de fond (noir par défaut).

## Couleur de fond personnalisée

On peut changer la couleur de fond lors de la conversion :

```bash
make convert IMG=./assets/sprite.png BG=4   # fond bleu (couleur MO5 n°4)
```

Source: `mo5-docs/mo5/mo5_sprite.md`


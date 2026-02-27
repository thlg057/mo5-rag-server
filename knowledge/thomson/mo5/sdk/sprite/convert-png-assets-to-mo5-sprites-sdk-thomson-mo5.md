# Convertir des PNG en sprites MO5 avec le pipeline du SDK (Thomson MO5)

Ce chunk résume le pipeline PNG → sprite décrit par `mo5_sprite.h` et les fichiers générés.

## Goal

- Préparer des PNG compatibles avec le MO5.
- Utiliser le script de conversion pour générer les données et macros C.

## Contraintes sur le PNG

- Largeur multiple de 8 pixels (ajustée automatiquement sinon).
- Au plus 2 couleurs par groupe de 8 pixels horizontaux (contrainte de couleur MO5).
- 16 couleurs maximum sur l'image.
- La transparence est convertie en couleur de fond (noir par défaut ou configurable).

## Conversion avec `make`

```bash
make convert IMG=./assets/perso.png
make convert IMG=./assets/perso.png BG=4   # fond bleu
```

Le script génère un header C, par exemple `assets/perso.h` :

```c
#define SPRITE_PERSO_WIDTH_BYTES  4
#define SPRITE_PERSO_HEIGHT      32

unsigned char sprite_perso_form[128];
unsigned char sprite_perso_color[128];

#define SPRITE_PERSO_INIT \
    { sprite_perso_form, sprite_perso_color, \
      SPRITE_PERSO_WIDTH_BYTES, SPRITE_PERSO_HEIGHT }
```

## Utilisation dans le code

```c
#include "assets/perso.h"

MO5_Sprite sprite_perso = SPRITE_PERSO_INIT;
```

## Notes

- Les macros `SPRITE_*_INIT` garantissent que `width_bytes` et `height` sont cohérents avec les données générées.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_sprite_h.md`

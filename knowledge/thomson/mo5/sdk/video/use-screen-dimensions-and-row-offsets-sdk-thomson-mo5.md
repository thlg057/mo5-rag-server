# Utiliser les dimensions écran et la table `row_offsets` du SDK MO5 (Thomson MO5)

Ce chunk présente les constantes de dimension d'écran et la table `row_offsets` définies dans `mo5_video.h`.

## Goal

- Connaître la taille de l'écran en octets et en lignes.
- Accéder rapidement à la VRAM d'une ligne donnée via `row_offsets`.

## Dimensions écran

```c
#define SCREEN_WIDTH_BYTES  40
#define SCREEN_HEIGHT      200
#define SCREEN_SIZE_BYTES  8000
```

- Largeur : 40 octets (320 pixels / 8).
- Hauteur : 200 lignes pixels.
- Taille totale : 8000 octets.

## Table `row_offsets`

```c
extern unsigned int row_offsets[SCREEN_HEIGHT];
```

- Remplie par `mo5_video_init`.
- Pour une ligne `y`, `row_offsets[y]` donne l'offset de début dans la VRAM.

Comparaison :

```c
// Sans table (plus lent : multiplication)
offset = y * SCREEN_WIDTH_BYTES + x;

// Avec table (plus rapide)
offset = row_offsets[y] + x;
```

## Notes

- Toujours appeler `mo5_video_init` avant d'utiliser `row_offsets`.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_video_h.md`

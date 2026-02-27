# Dessiner des panneaux d'UI avec `mo5_fill_rect` du SDK MO5 (Thomson MO5)

Ce chunk documente `mo5_fill_rect` de `mo5_video.h`, utilisée pour remplir des rectangles couleur sur l'écran.

## Goal

- Comprendre les unités utilisées par `mo5_fill_rect`.
- Dessiner des HUD, barres et blocs de décor.

## Prototype

```c
void mo5_fill_rect(int tx, int ty, int w, int h, unsigned char color);
```

Paramètres :

- `tx` : position X en **octets** (1 octet = 8 pixels).
- `ty` : position Y en lignes pixels.
- `w`  : largeur en octets.
- `h`  : hauteur en lignes pixels.
- `color` : octet couleur au format `COLOR(bg, fg)`.

## Exemples

Zone de score en bas de l'écran :

```c
mo5_fill_rect(0, 192, 40, 8, COLOR(C_BLACK, C_WHITE));
```

Bloc de fond 32×32 pixels à (80px, 40px) :

```c
mo5_fill_rect(10, 40, 4, 32, COLOR(C_BLUE, C_BLUE));
```

## Notes

- `mo5_fill_rect` est une brique utile pour dessiner rapidement des zones pleines (HUD, barres de vie, fond par blocs) sans passer par les sprites.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_video_h.md`

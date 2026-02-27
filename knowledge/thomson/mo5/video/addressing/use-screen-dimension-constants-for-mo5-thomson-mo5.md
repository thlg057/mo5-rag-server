# Utiliser les constantes de dimensions écran sur MO5

## Goal

Décrire les constantes d’écran fournies par le SDK sprite et comment les utiliser pour les calculs de position.

## Constantes écran

```c
#define SCREEN_WIDTH_BYTES  40    // 320 pixels / 8
#define SCREEN_HEIGHT       200
#define SCREEN_SIZE_BYTES   8000   // 40 × 200
```

Interprétation :
- Largeur écran : 40 **octets** (320 pixels / 8 pixels par octet).
- Hauteur écran : 200 lignes pixels.
- Taille totale de la zone forme : 8000 octets.

## Exemple : centrage horizontal d’un sprite

Pour un sprite 16 pixels de large (`width_bytes = 2`) :

```c
player.pos.x = (SCREEN_WIDTH_BYTES - sprite.width_bytes) / 2;
```

Rappel :
- `x` est en **octets**, pas en pixels.
- Cette formule fonctionne pour tout sprite aligné sur 8 pixels.

## Impact sur les clamps et collisions

- Utiliser `SCREEN_WIDTH_BYTES` et `SCREEN_HEIGHT` pour :
  - Limiter la position (`mo5_actor_clamp` ou clamp manuel).
  - Calculer les bornes de déplacement.
- Éviter de travailler en pixels horizontaux si le moteur reste aligné sur 8 pixels.

Source: `mo5-docs/mo5/mo5_sprite.md`


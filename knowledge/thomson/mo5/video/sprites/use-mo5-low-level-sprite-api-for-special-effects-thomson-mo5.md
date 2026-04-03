# Utiliser l’API bas niveau des sprites (`mo5_draw/clear/move_sprite`) sur MO5

## Goal

Présenter les fonctions bas niveau du SDK sprite pour les cas spéciaux (HUD, décor, effets).

## `mo5_draw_sprite`

```c
void mo5_draw_sprite(int tx, int ty,
                     unsigned char *form_data,
                     unsigned char *color_data,
                     int width_bytes,
                     int height);
```

- Dessine un sprite brut à la position `(tx, ty)`.
- Optimisé :
  - **1 seul** changement de banque PRC pour écrire toutes les couleurs.
  - **1 seul** changement de banque PRC pour écrire toutes les formes.

## `mo5_clear_sprite`

```c
void mo5_clear_sprite(int tx, int ty,
                      int width_bytes,
                      int height);
```

- Efface la zone occupée par le sprite.
- Utilise la même stratégie optimisée que `mo5_draw_sprite` (2 switches PRC pour toute la zone).

## `mo5_move_sprite`

```c
void mo5_move_sprite(int old_tx, int old_ty,
                     int new_tx, int new_ty,
                     unsigned char *form_data,
                     unsigned char *color_data,
                     int width_bytes,
                     int height);
```

- Combine **clear + draw** en un seul appel.
- Ne cleare que la zone **non recouverte** entre ancienne et nouvelle position.
- Fallback automatique sur clear + draw classique si le déplacement est plus grand que la taille du sprite.

### Gain typique (sprite 16×16, déplacement horizontal de 8px)

| Approche                 | Écritures VRAM | Économie |
|--------------------------|----------------|----------|
| `clear` + `draw` naïf    | 128            | —        |
| `mo5_move_sprite`        | 96             | ~25 %    |

## Quand utiliser l’API bas niveau

- Dessin de HUD / interface.
- Effets spéciaux non basés sur `MO5_Actor`.
- Cas où on veut un contrôle total sur le moment où clear/draw sont effectués.

Pour les entités classiques du jeu (joueur, ennemis, tirs), préférer l’API **Actor**.

Source: `mo5-docs/mo5/mo5_sprite.md`


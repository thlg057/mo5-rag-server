# Utiliser l'API sprite bas niveau du SDK MO5 (Thomson MO5)

Ce chunk documente les fonctions bas niveau `mo5_draw_sprite`, `mo5_clear_sprite` et `mo5_move_sprite` de `mo5_sprite.h`.

## Goal

- Dessiner un sprite à partir de données brutes.
- Effacer une zone rectangulaire correspondant à un sprite.
- Déplacer un sprite sans utiliser l'API Actor.

## Prototypes

```c
void mo5_draw_sprite(int tx, int ty,
                     unsigned char *form_data, unsigned char *color_data,
                     int width_bytes, int height);

void mo5_clear_sprite(int tx, int ty, int width_bytes, int height);

void mo5_move_sprite(int old_tx, int old_ty, int new_tx, int new_ty,
                     unsigned char *form_data, unsigned char *color_data,
                     int width_bytes, int height);
```

- `tx`, `old_tx`, `new_tx` : positions X en **octets**.
- `ty`, `old_ty`, `new_ty` : positions Y en pixels.
- `width_bytes`, `height` : dimensions en octets / lignes pixels.

## Quand utiliser l'API bas niveau

- HUD fixe ou tuiles de décor.
- Effets graphiques spéciaux qui ne s'intègrent pas bien avec `MO5_Actor`.

`mo5_move_sprite` peut retomber sur un pattern `clear` + `draw` complet si le déplacement dépasse la taille du sprite.

## Notes

- Pour les entités de jeu mobiles classiques, préfère l'API Actor plus haut niveau.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_sprite_h.md`

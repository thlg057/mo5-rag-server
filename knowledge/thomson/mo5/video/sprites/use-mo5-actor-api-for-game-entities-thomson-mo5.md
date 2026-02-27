# Utiliser l’API Actor (`mo5_actor_*`) pour les entités de jeu (MO5)

## Goal

Montrer comment utiliser `MO5_Actor` et l’API `mo5_actor_*` pour gérer les sprites de jeu.

## Initialiser un acteur

```c
#include "assets/perso.h"

MO5_Sprite sprite_perso = SPRITE_PERSO_INIT;

MO5_Actor player;
player.sprite  = &sprite_perso;
player.pos.x   = 10;     // en OCTETS
player.pos.y   = 84;     // en pixels
player.old_pos = player.pos;  // important !
```

- `sprite` pointe vers un `MO5_Sprite` partagé.
- `pos` contient la position courante.
- `old_pos` doit être initialisé identique à `pos`.

## `mo5_actor_draw` – premier affichage

```c
void mo5_actor_draw(const MO5_Actor *actor);

// Exemple
mo5_actor_draw(&player);
```

À utiliser pour le **premier dessin** d’un acteur. Ensuite, utiliser `mo5_actor_move()`.

## `mo5_actor_clear` – effacer un acteur

```c
void mo5_actor_clear(const MO5_Actor *actor);

// Exemple : retirer un ennemi de l'écran
mo5_actor_clear(&ennemi);
```

Efface complètement le sprite à sa position courante.

## `mo5_actor_move` – déplacement optimisé

```c
void mo5_actor_move(MO5_Actor *actor, int new_x, int new_y);

// Exemple
mo5_actor_move(&player, player.pos.x + 1, player.pos.y);
```

Comportement :
- Ne fait rien si la position ne change pas.
- Ne cleare que la **zone hors recouvrement** entre ancienne et nouvelle position.
- Met à jour `old_pos` et `pos` automatiquement.

## `mo5_actor_clamp` – rester dans l’écran

```c
void mo5_actor_clamp(MO5_Actor *actor);
```

Limite la position aux bords de l’écran en tenant compte de la taille du sprite.

## Pattern recommandé dans la boucle de jeu

```c
MO5_Position new_pos = player.pos;
new_pos.x += dx;
new_pos.y += dy;

// Clamp manuel sur new_pos
int max_x = SCREEN_WIDTH_BYTES - player.sprite->width_bytes;
int max_y = SCREEN_HEIGHT      - player.sprite->height;
if (new_pos.x < 0)      new_pos.x = 0;
if (new_pos.x > max_x)  new_pos.x = max_x;
if (new_pos.y < 0)      new_pos.y = 0;
if (new_pos.y > max_y)  new_pos.y = max_y;

mo5_actor_move(&player, new_pos.x, new_pos.y);
```

Source: `mo5-docs/mo5/mo5_sprite.md`


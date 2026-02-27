# Exemple : déplacer un joueur avec l’API Actor sur MO5

## Goal

Montrer un exemple complet (simplifié) d’utilisation de `MO5_Actor` et `mo5_actor_move()` pour déplacer un sprite avec le clavier.

## Code d’exemple (simplifié)

```c
#include "mo5_sprite.h"
#include "assets/perso.h"

#define KEY_UP    'Z'
#define KEY_DOWN  'S'
#define KEY_LEFT  'A'
#define KEY_RIGHT 'E'
#define SPEED_X   1   // octets par déplacement
#define SPEED_Y   4   // pixels par déplacement

int main(void) {
    // 1. Initialisation vidéo
    mo5_init_graphic_mode(COLOR(C_BLACK, C_BLACK));

    // 2. Sprite et acteur
    MO5_Sprite spr_perso = SPRITE_PERSO_INIT;
    MO5_Actor  player;
    player.sprite = &spr_perso;
    player.pos.x  = (SCREEN_WIDTH_BYTES - spr_perso.width_bytes) / 2;
    player.pos.y  = (SCREEN_HEIGHT      - spr_perso.height)      / 2;
    player.old_pos = player.pos;
    mo5_actor_draw(&player);

    // 3. Boucle principale
    while (1) {
        char key = wait_key();    // fonction de lecture clavier bloquante

        MO5_Position new_pos = player.pos;
        switch (key) {
            case KEY_UP:    new_pos.y -= SPEED_Y; break;
            case KEY_DOWN:  new_pos.y += SPEED_Y; break;
            case KEY_LEFT:  new_pos.x -= SPEED_X; break;
            case KEY_RIGHT: new_pos.x += SPEED_X; break;
        }

        // Clamp manuel dans l'écran
        int max_x = SCREEN_WIDTH_BYTES - spr_perso.width_bytes;
        int max_y = SCREEN_HEIGHT      - spr_perso.height;
        if (new_pos.x < 0)      new_pos.x = 0;
        if (new_pos.x > max_x)  new_pos.x = max_x;
        if (new_pos.y < 0)      new_pos.y = 0;
        if (new_pos.y > max_y)  new_pos.y = max_y;

        mo5_actor_move(&player, new_pos.x, new_pos.y);
    }
}
```

## Points à retenir

- Toujours initialiser `old_pos` = `pos` avant le premier `mo5_actor_move()`.
- Utiliser les constantes écran (`SCREEN_WIDTH_BYTES`, `SCREEN_HEIGHT`) pour les clamps.
- Ne pas redessiner avec `mo5_actor_draw()` dans la boucle, mais utiliser `mo5_actor_move()`.

Source: `mo5-docs/mo5/mo5_sprite.md`


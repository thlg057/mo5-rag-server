# Éviter les erreurs courantes avec le SDK sprites MO5

## Goal

Lister les pièges classiques lors de l’utilisation de `MO5_Sprite` / `MO5_Actor` et leurs correctifs.

## Ne jamais dupliquer un `MO5_Sprite`

```c
// ❌ Mauvais : duplication inutile des données graphiques
MO5_Actor e1; /* copie des données dans e1 */
MO5_Actor e2; /* autre copie */

// ✅ Correct : un seul MO5_Sprite, plusieurs acteurs qui pointent dessus
MO5_Sprite spr = SPRITE_ENNEMI_INIT;
e1.sprite = &spr;
e2.sprite = &spr;
```

Sur 48 Ko de RAM, les données de sprite doivent être partagées entre tous les acteurs.

## Toujours initialiser `old_pos`

```c
// ❌ old_pos non initialisée → premier move() peut clearer n'importe où
MO5_Actor player;
player.pos.x = 10;
player.pos.y = 20;
// oubli de player.old_pos !

// ✅ Correct
player.pos     = (MO5_Position){10, 20};
player.old_pos = player.pos;
```

`mo5_actor_move()` utilise `old_pos` pour savoir où effacer.

## Clamper AVANT d’appeler `mo5_actor_move`

`mo5_actor_move()` prend `new_x/new_y` comme cible. Si on clamp **après**, le clear a déjà été fait avec de mauvaises coordonnées.

Pattern correct :

1. Calculer `new_pos`.
2. Clamper `new_pos` dans l’écran.
3. Appeler `mo5_actor_move(actor, new_pos.x, new_pos.y)`.

## `x` est en octets, pas en pixels

```c
// ✅ Correct pour centrer un sprite 16px (2 octets de large)
player.pos.x = (SCREEN_WIDTH_BYTES - 2) / 2;

// ❌ Trompeur : travaille en pixels, source d'erreurs
player.pos.x = (320 - 16) / 2;
```

Toujours raisonner en **octets horizontaux** pour les coordonnées `x`.

## Ne pas utiliser `mo5_actor_draw` dans la boucle de jeu

```c
// ✅ Premier affichage uniquement
mo5_actor_draw(&player);

// ✅ Dans la boucle
mo5_actor_move(&player, new_x, new_y);

// ❌ Dans la boucle : redessine par-dessus sans effacer l'ancienne position
mo5_actor_draw(&player);
```

`mo5_actor_draw()` ignore `old_pos` et ne clear rien. Pour les déplacements continus, utiliser exclusivement `mo5_actor_move()`.

Source: `mo5-docs/mo5/mo5_sprite.md`


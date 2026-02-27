# Utiliser l'API Actor du SDK MO5 pour déplacer des entités (Thomson MO5)

Ce chunk décrit les fonctions `mo5_actor_draw`, `mo5_actor_clear`, `mo5_actor_move` et `mo5_actor_clamp` fournies par `mo5_sprite.h`.

## Goal

- Afficher un acteur à l'écran.
- Déplacer un acteur avec un rendu différentiel optimisé.
- Empêcher un acteur de sortir de l'écran.

## Prototypes

```c
void mo5_actor_draw(const MO5_Actor *actor);
void mo5_actor_clear(const MO5_Actor *actor);
void mo5_actor_move(MO5_Actor *actor, int new_x, int new_y);
void mo5_actor_clamp(MO5_Actor *actor);
```

## Premier affichage

```c
mo5_actor_draw(&player);   // à utiliser une seule fois au début
```

- Dessine le sprite à la position `pos` actuelle.
- Dans la boucle de jeu, préfère toujours `mo5_actor_move`.

## Déplacement optimisé

```c
mo5_actor_move(&player, new_pos.x, new_pos.y);
```

- Met à jour `old_pos` et `pos` automatiquement.
- N'efface que la partie de l'ancienne position qui ne sera pas recouverte.
- No-op si la position ne change pas (aucun accès VRAM inutile).

## Clamp aux bords de l'écran

```c
mo5_actor_clamp(&player);
```

- Limite `pos` aux bords de l'écran en tenant compte de la taille du sprite.
- À appeler sur les coordonnées calculées **avant** le `move` (pattern : clamp sur `new_pos`, puis `mo5_actor_move`).

## Notes

- Toujours initialiser `old_pos` à `pos` avant le premier `mo5_actor_move`.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_sprite_h.md`

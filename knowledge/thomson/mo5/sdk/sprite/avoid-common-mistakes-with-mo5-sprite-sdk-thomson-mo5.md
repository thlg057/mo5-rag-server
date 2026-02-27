# Éviter les erreurs courantes avec l'API sprite du SDK MO5 (Thomson MO5)

Ce chunk liste les pièges fréquents décrits dans `mo5_sprite.h` et comment les éviter.

## Goal

- Initialiser correctement un `MO5_Actor`.
- Respecter les unités (octets vs pixels).
- Utiliser les bonnes fonctions dans la boucle de jeu.

## Initialiser `old_pos`

```c
player.pos.x = 10; player.pos.y = 20;
player.old_pos = player.pos;   // ✅ avant le premier move
```

Sans cette initialisation, le premier `mo5_actor_move` peut effacer une zone aléatoire.

## X en octets, pas en pixels

```c
player.pos.x = 20;    // 20 octets = 160 pixels
```

- Ne pas confondre coordonnées en octets et en pixels (`SCREEN_WIDTH_BYTES` vs 320 pixels).

## Ne pas appeler `mo5_actor_draw` en boucle

```c
// ❌ fantômes : redessine sans clearer l'ancienne position
while (1) { mo5_actor_draw(&player); }

// ✅ rendu différentiel
while (1) { mo5_actor_move(&player, new_x, new_y); }
```

## Clamp avant le move

Toujours appliquer le clamp sur la nouvelle position **avant** d'appeler `mo5_actor_move`.

## Notes

- Ces règles viennent directement des recommandations de `mo5_sprite.h` et évitent une grande partie des artefacts visuels.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_sprite_h.md`

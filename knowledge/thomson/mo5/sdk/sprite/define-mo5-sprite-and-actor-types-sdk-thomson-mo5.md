# Définir les types `MO5_Sprite` et `MO5_Actor` du SDK MO5 (Thomson MO5)

Ce chunk présente les structures de base définies dans `mo5_sprite.h` : `MO5_Position`, `MO5_Sprite` et `MO5_Actor`.

## Goal

- Comprendre ce que représentent les structures du module sprite.
- Initialiser correctement les acteurs à partir de sprites générés.

## Structures principales

```c
typedef struct {
    int x;   // en octets (1 octet = 8 pixels)
    int y;   // en pixels
} MO5_Position;

typedef struct {
    unsigned char *form;
    unsigned char *color;
    int width_bytes;
    int height;
} MO5_Sprite;

typedef struct {
    const MO5_Sprite *sprite;
    MO5_Position      pos;
    MO5_Position      old_pos;
} MO5_Actor;
```

Points importants :

- `x` est en **octets**, pas en pixels.
- Un `MO5_Sprite` représente des données graphiques **partagées** (utilisables par plusieurs acteurs).
- `old_pos` doit être initialisé à la même valeur que `pos` avant le premier `mo5_actor_move`.

## Exemple d'initialisation

```c
MO5_Sprite spr_enemy = SPRITE_ENNEMI_INIT;
MO5_Actor ennemi;
ennemi.sprite  = &spr_enemy;
ennemi.pos.x   = 10;
ennemi.pos.y   = 20;
ennemi.old_pos = ennemi.pos;
```

## Notes

- Plusieurs `MO5_Actor` peuvent pointer vers le même `MO5_Sprite` pour économiser la mémoire.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_sprite_h.md`

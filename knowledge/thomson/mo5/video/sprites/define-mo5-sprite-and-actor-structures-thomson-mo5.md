# Définir les structures `MO5_Position`, `MO5_Sprite` et `MO5_Actor`

## Goal

Documenter les structures de base du SDK sprite MO5 et leurs règles d’utilisation mémoire.

## `MO5_Position`

Représente la position d’un objet à l’écran.

```c
typedef struct {
    int x;   // Position horizontale en OCTETS (1 octet = 8 pixels)
    int y;   // Position verticale en lignes pixels
} MO5_Position;
```

Points clés :
- `x` est exprimé en **octets**, pas en pixels.
- 1 octet = 8 pixels horizontaux.
- `y` est en pixels (0..199 sur un écran 200 lignes).

## `MO5_Sprite`

Contient les données graphiques statiques d’un sprite.

```c
typedef struct {
    unsigned char *form;    // Bitmap 1 bit/pixel  (1 = forme, 0 = fond)
    unsigned char *color;   // Attributs couleur (1 octet par 8 pixels)
    int width_bytes;        // Largeur en octets
    int height;             // Hauteur en lignes pixels
} MO5_Sprite;
```

Règles :
- `form` et `color` pointent vers des tableaux générés (souvent par `png2mo5.py`).
- `width_bytes` = largeur en octets, pas en pixels.
- Un même `MO5_Sprite` peut être partagé par plusieurs acteurs.

## `MO5_Actor`

Associe un sprite à une position courante et précédente.

```c
typedef struct {
    const MO5_Sprite *sprite;   // Pointeur vers le sprite (ressource partagée)
    MO5_Position      pos;      // Position courante
    MO5_Position      old_pos;  // Ancienne position (pour move optimisé)
} MO5_Actor;
```

Idées importantes :
- Plusieurs `MO5_Actor` peuvent utiliser **le même** `MO5_Sprite` :

```c
MO5_Sprite sprite_ennemi = SPRITE_ENNEMI_INIT;
MO5_Actor  ennemis[5];
int i;
for (i = 0; i < 5; i++)
    ennemis[i].sprite = &sprite_ennemi;  // partage des données graphiques
```

- `old_pos` est utilisé par `mo5_actor_move()` pour know où clearer minimalement.
- Au démarrage, `old_pos` doit être **initialisé** à la même valeur que `pos`.

Source: `mo5-docs/mo5/mo5_sprite.md`


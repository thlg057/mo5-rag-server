# SDK Graphique MO5 — Documentation

> Référence complète de `mo5_sprite.h` / `mo5_sprite.c` et du pipeline de création de sprites

---

## Table des matières

1. [Vue d'ensemble](#1-vue-densemble)
2. [Structures de données](#2-structures-de-données)
3. [Initialisation](#3-initialisation)
4. [Créer un sprite depuis un PNG](#4-créer-un-sprite-depuis-un-png)
5. [API Actor — niveau jeu](#5-api-actor--niveau-jeu)
6. [API bas niveau — accès direct](#6-api-bas-niveau--accès-direct)
7. [Constantes et macros utiles](#7-constantes-et-macros-utiles)
8. [Exemple complet](#8-exemple-complet)
9. [Bonnes pratiques et pièges](#9-bonnes-pratiques-et-pièges)

---

## 1. Vue d'ensemble

Le SDK graphique fournit deux niveaux d'abstraction :

```
┌─────────────────────────────────────────┐
│           Code de jeu (main.c)          │
├─────────────────────────────────────────┤
│    API Actor  (mo5_actor_*)             │  ← à utiliser en priorité
├─────────────────────────────────────────┤
│    API bas niveau  (mo5_draw/clear/move)│  ← cas spéciaux uniquement
├─────────────────────────────────────────┤
│    VRAM / PRC / row_offsets             │  ← matériel MO5
└─────────────────────────────────────────┘
```

**Fichiers du SDK :**

| Fichier | Rôle |
|---|---|
| `mo5_sprite.h` | Structures, constantes, déclarations |
| `mo5_sprite.c` | Implémentation de toutes les fonctions |
| `png2mo5.py` | Script de conversion PNG → header C |

---

## 2. Structures de données

### `MO5_Position`

Coordonnées d'un objet à l'écran.

```c
typedef struct {
    int x;   // Position horizontale en octets (1 octet = 8 pixels)
    int y;   // Position verticale en lignes pixels
} MO5_Position;
```

> **Important :** `x` est en **octets**, pas en pixels. Pour un sprite aligné à 8 pixels près, `x=1` signifie 8 pixels depuis la gauche, `x=2` signifie 16 pixels, etc.

### `MO5_Sprite`

Données graphiques statiques d'un sprite. Ne contient pas de position — c'est une ressource partageable entre plusieurs acteurs.

```c
typedef struct {
    unsigned char *form;    // Bitmap 1 bit/pixel  (1=forme, 0=fond)
    unsigned char *color;   // Attributs couleur    (1 octet par groupe de 8 pixels)
    int width_bytes;        // Largeur en octets
    int height;             // Hauteur en lignes pixels
} MO5_Sprite;
```

### `MO5_Actor`

Entité du jeu : associe un sprite à une position. Conserve l'ancienne position pour le rendu optimisé.

```c
typedef struct {
    const MO5_Sprite *sprite;   // Pointeur vers le sprite (ne pas copier !)
    MO5_Position      pos;      // Position courante
    MO5_Position      old_pos;  // Position précédente (gérée automatiquement)
} MO5_Actor;
```

> **Règle mémoire :** plusieurs acteurs peuvent pointer vers le **même** `MO5_Sprite`. Sur 48 Ko de RAM, ne jamais dupliquer les données graphiques.
>
> ```c
> // ✅ Correct : 5 ennemis, 1 seul jeu de données graphiques
> MO5_Sprite sprite_ennemi = SPRITE_ENNEMI_INIT;
> MO5_Actor  ennemis[5];
> for (i = 0; i < 5; i++)
>     ennemis[i].sprite = &sprite_ennemi;
> ```

---

## 3. Initialisation

```c
void mo5_init_graphic_mode(unsigned char color);
```

À appeler **une seule fois** au démarrage, avant tout affichage. Elle :
- Précalcule la table `row_offsets` (offsets de lignes, évite les multiplications à l'affichage)
- Passe en mode graphique
- Remplit l'écran avec la couleur donnée

```c
// Exemple : fond noir
mo5_init_graphic_mode(COLOR(C_BLACK, C_BLACK));
```

---

## 4. Créer un sprite depuis un PNG

### Commande

```bash
make convert IMG=./assets/sprite.png
```

Le Makefile appelle automatiquement `png2mo5.py` et génère un fichier `.h` dans le même répertoire que le PNG.

### Ce que le script génère

Pour un fichier `perso.png`, le script produit `perso.h` contenant :

```c
// Dimensions
#define SPRITE_PERSO_WIDTH_BYTES 4
#define SPRITE_PERSO_HEIGHT      32

// Données brutes (à ne pas modifier manuellement)
unsigned char sprite_perso_form[128]  = { ... };
unsigned char sprite_perso_color[128] = { ... };

// Macro d'initialisation → à utiliser dans le code
#define SPRITE_PERSO_INIT \
    { sprite_perso_form, sprite_perso_color, \
      SPRITE_PERSO_WIDTH_BYTES, SPRITE_PERSO_HEIGHT }
```

### Contraintes du PNG source

Le MO5 impose **2 couleurs maximum par groupe de 8 pixels horizontaux**. Le script détecte automatiquement les 2 couleurs dominantes de chaque groupe, mais le résultat sera meilleur si le PNG respecte cette contrainte dès sa conception.

- Largeur **multiple de 8 pixels** (ajustée automatiquement sinon)
- Maximum 16 couleurs au total (palette MO5)
- Fond transparent → converti en couleur de fond (noir par défaut)

### Option couleur de fond

```bash
make convert IMG=./assets/sprite.png BG=4   # fond bleu (couleur MO5 n°4)
```

---

## 5. API Actor — niveau jeu

C'est l'API à utiliser dans le code de jeu. Elle gère automatiquement `old_pos` et utilise le rendu optimisé.

### Initialisation d'un acteur

```c
#include "assets/perso.h"

MO5_Sprite sprite_perso = SPRITE_PERSO_INIT;

MO5_Actor player;
player.sprite  = &sprite_perso;
player.pos.x   = 10;     // en octets
player.pos.y   = 84;     // en pixels
player.old_pos = player.pos;
```

### `mo5_actor_draw`

Dessine l'acteur à sa position courante. À utiliser pour le premier affichage.

```c
void mo5_actor_draw(const MO5_Actor *actor);

// Exemple
mo5_actor_draw(&player);
```

### `mo5_actor_clear`

Efface l'acteur à sa position courante.

```c
void mo5_actor_clear(const MO5_Actor *actor);

// Exemple : retirer un ennemi de l'écran
mo5_actor_clear(&ennemi);
```

### `mo5_actor_move`

Déplace l'acteur vers une nouvelle position de façon **optimisée** :
- Ne fait rien si la position n'a pas changé
- Ne cleare que la zone qui ne sera pas recouverte par le nouveau dessin
- Met à jour `old_pos` et `pos` automatiquement

```c
void mo5_actor_move(MO5_Actor *actor, int new_x, int new_y);

// Exemple
mo5_actor_move(&player, player.pos.x + 1, player.pos.y);
```

### `mo5_actor_clamp`

Limite la position de l'acteur aux bords de l'écran (tient compte de la taille du sprite).

```c
void mo5_actor_clamp(MO5_Actor *actor);
```

> **Usage typique :** calculer la nouvelle position souhaitée, appliquer le clamp sur les coordonnées brutes, puis appeler `mo5_actor_move`.

```c
// Pattern recommandé dans la boucle de jeu
MO5_Position new_pos = player.pos;
new_pos.x += dx;
new_pos.y += dy;

// Clamp manuel sur new_pos avant de l'envoyer à move
int max_x = SCREEN_WIDTH_BYTES - player.sprite->width_bytes;
int max_y = SCREEN_HEIGHT      - player.sprite->height;
if (new_pos.x < 0)      new_pos.x = 0;
if (new_pos.x > max_x)  new_pos.x = max_x;
if (new_pos.y < 0)      new_pos.y = 0;
if (new_pos.y > max_y)  new_pos.y = max_y;

mo5_actor_move(&player, new_pos.x, new_pos.y);
```

---

## 6. API bas niveau — accès direct

À utiliser uniquement pour des cas spéciaux (HUD, effets, tiles de décor...). Pour les sprites de jeu, préférer l'API Actor.

### `mo5_draw_sprite`

```c
void mo5_draw_sprite(int tx, int ty,
                     unsigned char *form_data, unsigned char *color_data,
                     int width_bytes, int height);
```

Optimisé : **1 seul switch PRC** pour toutes les couleurs, **1 seul switch PRC** pour toutes les formes (au lieu d'un switch par ligne dans la version naïve).

### `mo5_clear_sprite`

```c
void mo5_clear_sprite(int tx, int ty, int width_bytes, int height);
```

Même optimisation : 2 passes complètes, 2 switches PRC au total.

### `mo5_move_sprite`

```c
void mo5_move_sprite(int old_tx, int old_ty,
                     int new_tx, int new_ty,
                     unsigned char *form_data, unsigned char *color_data,
                     int width_bytes, int height);
```

Version optimisée du déplacement : ne cleare que la zone hors recouvrement entre l'ancienne et la nouvelle position.

**Gain typique (sprite 16×16, déplacement de 8px) :**

| Approche | Écritures VRAM | Économie |
|---|---|---|
| `clear` + `draw` naïf | 128 | — |
| `mo5_move_sprite` | 96 | ~25% |

Fallback automatique sur `clear` + `draw` si le déplacement est supérieur à la taille du sprite (pas de recouvrement).

---

## 7. Constantes et macros utiles

### Palette MO5

| Constante | Valeur | Couleur |
|---|---|---|
| `C_BLACK` | 0 | Noir |
| `C_RED` | 1 | Rouge |
| `C_GREEN` | 2 | Vert |
| `C_YELLOW` | 3 | Jaune |
| `C_BLUE` | 4 | Bleu |
| `C_MAGENTA` | 5 | Magenta |
| `C_CYAN` | 6 | Cyan |
| `C_WHITE` | 7 | Blanc |
| `C_GRAY` | 8 | Gris |
| `C_LIGHT_RED` | 9 | Rouge clair |
| `C_LIGHT_GREEN` | 10 | Vert clair |
| `C_LIGHT_YELLOW` | 11 | Jaune clair |
| `C_LIGHT_BLUE` | 12 | Bleu clair |
| `C_PURPLE` | 13 | Violet |
| `C_LIGHT_CYAN` | 14 | Cyan clair |
| `C_ORANGE` | 15 | Orange |

### Macro `COLOR`

Construit un octet couleur MO5 au format `FFFFBBBB` :

```c
#define COLOR(bg, fg)   // bg = couleur fond, fg = couleur forme

// Exemples
COLOR(C_BLACK, C_WHITE)   // texte blanc sur fond noir  → 0x70
COLOR(C_BLUE,  C_YELLOW)  // jaune sur bleu             → 0x34
```

### Dimensions écran

```c
#define SCREEN_WIDTH_BYTES  40    // 320 pixels / 8
#define SCREEN_HEIGHT      200
#define SCREEN_SIZE_BYTES  8000   // 40 × 200
```

---

## 8. Exemple complet

```c
#include "mo5_sprite.h"
#include "assets/perso.h"
#include "assets/ennemi.h"

#define KEY_UP    'Z'
#define KEY_DOWN  'S'
#define KEY_LEFT  'A'
#define KEY_RIGHT 'E'
#define SPEED_X   1     // octets par déplacement
#define SPEED_Y   4     // pixels par déplacement

static char wait_key(void) {
    char c;
    do { c = mo5_getchar(); } while (c == 0);
    return (c >= 'a' && c <= 'z') ? c - 32 : c;
}

int main(void) {
    // 1. Initialisation
    mo5_init_graphic_mode(COLOR(C_BLACK, C_BLACK));

    // 2. Sprites (données graphiques, partagées)
    MO5_Sprite spr_perso  = SPRITE_PERSO_INIT;
    MO5_Sprite spr_ennemi = SPRITE_ENNEMI_INIT;

    // 3. Acteurs (entités du jeu avec position)
    MO5_Actor player;
    player.sprite = &spr_perso;
    player.pos.x  = (SCREEN_WIDTH_BYTES - SPRITE_PERSO_WIDTH_BYTES) / 2;
    player.pos.y  = (SCREEN_HEIGHT      - SPRITE_PERSO_HEIGHT)      / 2;
    player.old_pos = player.pos;
    mo5_actor_draw(&player);

    // Plusieurs ennemis, même sprite
    MO5_Actor ennemis[3];
    int i;
    for (i = 0; i < 3; i++) {
        ennemis[i].sprite  = &spr_ennemi;   // même sprite partagé
        ennemis[i].pos.x   = i * 10;
        ennemis[i].pos.y   = 10;
        ennemis[i].old_pos = ennemis[i].pos;
        mo5_actor_draw(&ennemis[i]);
    }

    // 4. Boucle de jeu
    while (1) {
        char key = wait_key();

        MO5_Position new_pos = player.pos;
        switch (key) {
            case KEY_UP:    new_pos.y -= SPEED_Y; break;
            case KEY_DOWN:  new_pos.y += SPEED_Y; break;
            case KEY_LEFT:  new_pos.x -= SPEED_X; break;
            case KEY_RIGHT: new_pos.x += SPEED_X; break;
        }

        // Clamp puis déplacement optimisé
        int max_x = SCREEN_WIDTH_BYTES - spr_perso.width_bytes;
        int max_y = SCREEN_HEIGHT      - spr_perso.height;
        if (new_pos.x < 0)      new_pos.x = 0;
        if (new_pos.x > max_x)  new_pos.x = max_x;
        if (new_pos.y < 0)      new_pos.y = 0;
        if (new_pos.y > max_y)  new_pos.y = max_y;

        mo5_actor_move(&player, new_pos.x, new_pos.y);
    }

    return 0;
}
```

---

## 9. Bonnes pratiques et pièges

### Ne jamais dupliquer un `MO5_Sprite`

```c
// ❌ Mauvais : les données graphiques sont copiées pour chaque ennemi
MO5_Actor e1; e1.sprite->form = sprite_data; // copie locale
MO5_Actor e2; e2.sprite->form = sprite_data; // encore une copie

// ✅ Correct : un seul MO5_Sprite, plusieurs acteurs qui pointent dessus
MO5_Sprite spr = SPRITE_ENNEMI_INIT;
e1.sprite = &spr;
e2.sprite = &spr;
```

### Toujours initialiser `old_pos`

```c
// ❌ old_pos non initialisée → premier move() peut clearer n'importe où
MO5_Actor player;
player.pos.x = 10;
player.pos.y = 20;
// oubli de player.old_pos !

// ✅
player.pos     = {10, 20};
player.old_pos = player.pos;   // identique à pos au départ
```

### Le clamp avant le move, pas après

`mo5_actor_move` utilise `pos` comme point de départ du clear. Si on clamp après le move, les coordonnées incorrectes ont déjà été utilisées pour le rendu.

### `x` est en octets, pas en pixels

Un sprite de 16px de large = `width_bytes = 2`. Pour le centrer sur 320px (40 octets) :

```c
// ✅ Correct
player.pos.x = (40 - 2) / 2;   // = 19 octets = 152 pixels

// ❌ Erreur classique
player.pos.x = (320 - 16) / 2; // = 152 → ça marche par hasard ici, mais la logique est fausse
```

### Ne pas appeler `mo5_actor_draw` dans la boucle principale

`mo5_actor_draw` redessine le sprite entier sans tenir compte de `old_pos`. Dans la boucle, utiliser exclusivement `mo5_actor_move` qui gère le delta et n'écrit que ce qui a changé.

```c
// ✅ Premier affichage uniquement
mo5_actor_draw(&player);

// ✅ Dans la boucle
mo5_actor_move(&player, new_x, new_y);

// ❌ Dans la boucle : redessine tout sans clearer l'ancienne position
mo5_actor_draw(&player);
```

---

*Ce SDK fait partie du projet de jeu Thomson MO5.*

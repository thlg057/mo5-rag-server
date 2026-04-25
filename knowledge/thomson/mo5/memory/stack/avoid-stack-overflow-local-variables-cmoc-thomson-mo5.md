# Avoid Stack Overflow with Too Many Local Variables in CMOC (Thomson MO5)

cmoc alloue les variables locales sur la pile via `LEAS -N,S` à l'entrée de la fonction. Un trop grand nombre de locales provoque un stack overflow silencieux.

## Symptôme

Le programme **freeze à l'entrée de la fonction**, avant d'exécuter la moindre instruction.
Le stack check de cmoc échoue silencieusement — aucune erreur visible.

## Exemple

```c
/* ❌ Trop de locales — crash au démarrage de la fonction */
void game_loop(void) {
    unsigned char score;
    unsigned char live;
    unsigned char new_x;
    unsigned char enemies_tick;
    unsigned char bullets_tick;
    unsigned char result;         /* une de trop → freeze avant while(1) */
    char key;
    unsigned char i;
    while (1) { ... }
}
```

## Solution — promouvoir en static global

Les variables `static` vivent en BSS/DATA — leur coût sur la pile est **zéro**.

```c
/* ✅ Promouvoir en static global avec préfixe gl_ */
static unsigned char gl_score;
static unsigned char gl_live;
static unsigned char gl_new_x;
static unsigned char gl_enemies_tick;
static unsigned char gl_bullets_tick;
static unsigned char gl_result;
static char          gl_key;
static unsigned char gl_i;

void game_loop(void) {
    /* frame quasi-vide → stack check passe */
    while (1) { ... }
}
```

## Convention de nommage

Utiliser un préfixe pour distinguer les variables promues des vraies globales :
- `gl_` pour les variables de `game_loop`
- `gu_` pour `game_update`, etc.

## Attention aux chaînes d'appel

Le stack s'accumule à chaque appel de fonction :
`game_loop → check_collisions → display_score` (avec `char buf[4]` dans display_score)
peut déborder même si chaque fonction semble raisonnable isolément.

Préférer retourner des flags au caller plutôt qu'appeler des fonctions d'affichage
depuis des fonctions de logique.

Source: `mo5_hardware_reference.md` section 7 — Fiabilité ÉLEVÉE (observé en pratique)

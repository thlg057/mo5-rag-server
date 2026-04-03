# Stratégies techniques — Jeu MO5 (6809)

## Contexte

Le Motorola 6809 du Thomson MO5 est un processeur 8-bit cadencé à ~1 MHz, sans FPU, avec très peu de RAM.
Chaque cycle CPU est précieux. Les stratégies ci-dessous sont des compromis entre lisibilité du code, jouabilité et contraintes matérielles.

---

## 1. AABB — Axis-Aligned Bounding Box

**Principe** : la collision entre deux sprites est détectée en comparant leurs rectangles englobants (position + dimensions). Pas de vérification pixel par pixel.

```c
static unsigned char collide(unsigned char ax, unsigned char ay,
                              unsigned char aw, unsigned char ah,
                              unsigned char bx, unsigned char by,
                              unsigned char bw, unsigned char bh)
{
    return (ax < bx + bw) && (ax + aw > bx) &&
           (ay < by + bh) && (ay + ah > by);
}
```

**Pourquoi pas le pixel-perfect ?**
La détection pixel-perfect nécessite de comparer les bitmaps des deux sprites pixel par pixel — des dizaines à centaines de comparaisons par paire. Sur 6809 à 1 MHz, avec plusieurs ennemis et balles à tester chaque frame, c'est inenvisageable. L'AABB se résout en 4 comparaisons d'octets.

**Compromis accepté** : légère imprécision aux coins des sprites, imperceptible en pratique à cette résolution.

---

## 2. Tick-based movement — Découplage du déplacement du VBL

**Principe** : au lieu de déplacer chaque entité à chaque VBL (Vertical Blank, ~50 Hz), on utilise un compteur de frames. Le déplacement n'est effectué que tous les N frames.

```c
enemies_tick++;
if (enemies_tick == ENEMY_FRAME_SPEED) {
    enemies_tick = 0;
    game_update_enemies();
}
```

**Avantages** :
- Contrôle précis de la vitesse de chaque type d'entité indépendamment
- Réduit le nombre d'appels aux fonctions de dessin/déplacement
- Permet d'ajuster la difficulté en modifiant une seule constante (`ENEMY_FRAME_SPEED`, `BULLET_FRAME_SPEED`)

**Variante utilisée** : ennemis et balles ennemies ont des compteurs séparés, ce qui permet des vitesses différentes sans calcul en virgule flottante.

---

## 3. Pool d'objets (Object Pool)

**Principe** : au lieu d'allouer/libérer dynamiquement des balles ou ennemis, on pré-alloue un tableau de slots de taille fixe. Un champ `active` indique si le slot est occupé.

```c
typedef struct {
    MO5_Actor     actor;
    unsigned char active; // 0 = slot libre
} ActiveActor;

static ActiveActor bullets_player[MAX_BULLETS_PLAYER];
static ActiveActor bullets_enemies[MAX_BULLETS_ENEMIES];
```

**Pourquoi ?**
Il n'y a pas de `malloc`/`free` utilisables efficacement sur MO5. L'allocation dynamique serait de toute façon trop coûteuse et risquerait la fragmentation mémoire. Le pool garantit une occupation mémoire fixe et prévisible, et la recherche d'un slot libre est un simple parcours de tableau.

**Règle d'or** : toujours initialiser **tous** les slots au démarrage (champ `active = 0` et pointeur `sprite` assigné), même ceux qui ne seront pas immédiatement utilisés — sinon les slots non initialisés contiennent de la mémoire aléatoire et sont traités comme actifs.

---

## 4. Invincibilité temporaire par compteur de frames

**Principe** : après avoir subi un dégât, le joueur devient invincible pendant N frames. Un compteur décrémenté à chaque VBL mesure cette durée.

```c
if (player_invincible > 0) {
    player_invincible--;
    // clignotement...
} else {
    // vérifier les collisions avec le joueur
}
```

**Feedback visuel** : le joueur clignote pendant l'invincibilité en alternant `mo5_actor_draw_bg` / `mo5_actor_clear_bg` tous les `PLAYER_BLINK_PERIOD` frames. Pas d'animation dédiée nécessaire.

**Pourquoi des frames plutôt que des millisecondes ?**
Pas d'accès à une horloge temps réel simple sur MO5. Le VBL est le seul timer fiable (~50 Hz), donc toute durée est exprimée en nombre de frames.

---

## 5. Variables statiques globales

**Principe** : tous les objets du jeu (player, ennemis, balles) sont déclarés en `static` au niveau fichier, pas sur la pile.

```c
static MO5_Actor   player;
static ActiveActor bullets_player[MAX_BULLETS_PLAYER];
static ActiveActor enemies[ENEMY_COUNT];
```

**Pourquoi ?**
- La pile du 6809 est très limitée. Mettre des tableaux de structs sur la pile risque le stack overflow.
- Les variables `static` sont garanties initialisées à zéro par le standard C — ce qui évite certaines corruptions (mais ne dispense pas d'initialiser explicitement les champs métier comme `active`).
- Pas de passage de pointeurs coûteux entre fonctions : les fonctions accèdent directement aux globales.

---

## 6. Déclarations C89 strictes

**Principe** : cmoc compile du C89/C90. Toutes les déclarations de variables doivent être placées **en tête de bloc**, avant tout statement.

```c
// Correct
void ma_fonction(void) {
    unsigned char i;
    unsigned char new_x;   // déclarations en premier

    game_init();           // statements ensuite
    new_x = player.pos.x;
}

// Incorrect — provoque un comportement indéfini avec cmoc
void ma_fonction(void) {
    game_init();           // statement
    unsigned char new_x;  // déclaration après : INTERDIT en C89
}
```

**Pourquoi c'est critique** : cmoc ne génère pas toujours d'erreur visible. Le code se compile mais le comportement au runtime est imprévisible (entrées ignorées, valeurs corrompues).

---

## 7. Pseudo-aléatoire par LFSR

**Principe** : pour choisir quel ennemi tire, on utilise un LFSR (Linear Feedback Shift Register) 8-bit — un registre à décalage avec rétroaction XOR sur certains bits.

```c
unsigned char feedback = ((rand_seed >> 7) & 1) ^
                         ((rand_seed >> 5) & 1) ^
                         ((rand_seed >> 4) & 1) ^
                         ((rand_seed >> 3) & 1);
rand_seed = (rand_seed << 1) | feedback;
```

**Pourquoi pas `rand()` ?**
`rand()` de la libc fait intervenir des multiplications 16 ou 32-bit — très coûteuses sur 6809. Le LFSR 8-bit ne nécessite que des shifts et des XOR, opérations natives et rapides sur le 6809. Il produit une séquence de 255 valeurs non répétitives avant de boucler.

---

## Récapitulatif

| Stratégie | Problème résolu | Coût évité |
|---|---|---|
| AABB | Détection de collision | Comparaison pixel-par-pixel |
| Tick-based movement | Vitesse indépendante du VBL | Déplacement chaque frame |
| Object pool | Gestion des balles/ennemis | malloc/free dynamique |
| Invincibilité par frames | Fairplay après dégât | Horloge temps réel |
| Variables statiques globales | Stabilité mémoire | Débordement de pile |
| C89 strict | Compilation correcte avec cmoc | Comportement indéfini |
| LFSR | Aléatoire léger | rand() 32-bit coûteux |

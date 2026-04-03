# Synchronisation Verticale (VBL) sur Thomson MO5

## Qu'est-ce que la VBL ?

L'écran d'un MO5 est redessiné par un faisceau électronique qui balaie l'écran de haut en bas, ligne par ligne, **50 fois par seconde** (50Hz, standard PAL européen). Ce balayage se décompose en deux phases :

- **Balayage actif** (~18.7ms) : le faisceau parcourt l'écran de haut en bas, ligne par ligne. C'est la phase visible.
- **Retour vertical / VBL** (~1.2ms) : le faisceau retourne en haut de l'écran. Pendant ce temps, rien n'est affiché.

```
         balayage actif        VBL
|████████████████████████████|____|████████████████████████████|____|
0ms                        18.7ms 20ms                        38.7ms 40ms
```

---

## Pourquoi c'est important ?

### 1. Vitesse de jeu incontrôlable sans VBL

Sans synchronisation, la game loop tourne à fond — aussi vite que le CPU le permet. Le jeu va donc à une vitesse dépendant du nombre d'opérations dans la boucle :

- Ajouter un ennemi → le jeu ralentit
- Supprimer des calculs → le jeu accélère
- Impossible à calibrer de manière fiable

Avec la VBL, la boucle est naturellement cadencée à **50 tours/seconde**, quoi qu'il arrive.

### 2. Flickering (scintillement) des sprites

Si on modifie la mémoire vidéo **pendant** le balayage actif, le faisceau peut afficher une partie de l'ancien sprite et une partie du nouveau sur la même frame :

```
Sans VBL :                    Avec VBL :
|  ancien sprite  |           |  ancien sprite  |
|----déchirure----|           |  ancien sprite  |
|  nouveau sprite |           |=== VBL : on dessine ici ===|
|  nouveau sprite |           |  nouveau sprite |
                              |  nouveau sprite |
```

En dessinant uniquement pendant la VBL, l'écran affiche toujours un état cohérent.

---

## Implémentation sur MO5

### Le registre hardware

Le MO5 utilise une PIA 6821. Le **bit 7** du registre `$A7E7` reflète le signal `INITRAME` du chip vidéo MC6847 :

| Valeur bit 7 | Signification |
|:---:|---|
| `1` | Balayage actif (faisceau sur l'écran) |
| `0` | Retour vertical — VBL |

### La fonction

```c
#define VBL_REG   ((unsigned char *)0xA7E7)  // bit7=1 pendant le balayage actif
#define VBL_BIT   0x80

void mo5_wait_vbl(void)
{
    while ( *VBL_REG &  VBL_BIT) ;   // attendre fin du balayage actif (bit7 → 0)
    while (!(*VBL_REG & VBL_BIT)) ;  // attendre début du prochain balayage (bit7 → 1)
}
```

La fonction se positionne sur le **front montant** de `INITRAME` : elle attend que le balayage actif commence, ce qui marque la fin du VBL précédent et le début de la nouvelle trame.

> **Note :** L'accès direct au registre via le pointeur `VBL_REG` est nécessaire pour que CMOC relise effectivement la valeur à chaque itération. Le mot-clé `volatile` n'est pas supporté par CMOC — éviter d'inliner cette fonction ou de la déclarer `static` pour empêcher l'optimisation de la relecture mémoire.

> **Note :** Il n'y a pas de timer programmable sur MO5 pour se synchroniser automatiquement. L'attente active sur `$A7E7` est la seule méthode fiable.

---

## Placement dans la game loop

Il existe deux stratégies de placement de `mo5_wait_vbl()` dans la boucle principale. Aucune n'est universellement supérieure — le choix dépend de la structure du jeu.

---

### Stratégie A — VBL en début de boucle (frame-locked loop)

```c
while (1) {
    mo5_wait_vbl();         // synchronisation en début de trame

    read_inputs();
    game_update_player();
    game_update_enemies();
    check_collisions();
    draw_all();
}
```

```
VBL   balayage actif
|____|  inputs + logique + dessin...  |____|  inputs + logique + dessin...
↑
mo5_wait_vbl()
```

Toute la logique et le dessin s'exécutent dans la même fenêtre de temps, après le VBL. C'est le pattern **"frame-locked loop"** : simple à implémenter, facile à raisonner. La boucle fait exactement un tour par trame.

**Limite** : si la logique + le dessin dépassent 20ms, une trame est sautée et le jeu ralentit — sans que ce soit détectable directement dans le code.

---

### Stratégie B — VBL juste avant le dessin (late sync)

```c
while (1) {
    read_inputs();
    game_update_player();
    game_update_enemies();
    check_collisions();

    mo5_wait_vbl();         // synchronisation juste avant de toucher la VRAM

    draw_all();
}
```

```
balayage actif                  VBL   balayage actif
|  inputs + logique...          |draw| |  inputs + logique...          |draw|
                                ↑
                          mo5_wait_vbl()
```

La logique s'exécute pendant le balayage actif (~18.7ms disponibles). Le dessin est réservé à la fenêtre VBL (~1.2ms). C'est le pattern **"late sync"** : il maximise le temps CPU disponible pour la logique et garantit que le dessin commence toujours au bon moment.

**Limite** : si le dessin seul dépasse la fenêtre VBL (~1200 cycles), des artefacts peuvent apparaître en bas de l'écran — les lignes déjà balayées au moment où on les modifie.

---

### Résumé comparatif

| | Stratégie A — début de boucle | Stratégie B — avant le dessin |
|---|---|---|
| Simplicité | ✓ Simple | Plus structuré |
| Temps pour la logique | ~20ms (logique + dessin mélangés) | ~18.7ms dédiés |
| Temps pour le dessin | ~20ms (logique + dessin mélangés) | ~1.2ms dédiés |
| Risque de tearing | Faible (dessin après VBL) | Minimal (dessin dans VBL) |
| Usage typique | Jeux simples, peu de sprites | Jeux avec beaucoup d'entités |

Pour un jeu simple avec peu de sprites, la **stratégie A** est suffisante et plus lisible. Pour un jeu avec de nombreuses entités à dessiner, la **stratégie B** est plus robuste.

### Terminologie

Ces deux patterns correspondent à des concepts bien connus dans la littérature sur les game loops :

- **Stratégie A** — connue sous le nom de **"fixed timestep"** ou **"frame-locked loop"** : la boucle entière est cadencée sur le VBL, logique et rendu sont traités dans la même fenêtre de temps.
- **Stratégie B** — proche du **"decouple update from render"** ou dans sa forme plus élaborée du **"game loop with variable timestep"** : on sépare la phase de mise à jour (qui peut prendre le temps qu'il faut) de la phase de rendu (synchronisée sur le signal vidéo).

La référence classique sur ce sujet est l'article **"Fix Your Timestep!"** de Glenn Fiedler (2004), qui décrit plusieurs variantes de game loop et leurs compromis. Sur le MO5 il n'est pas nécessaire d'aller aussi loin — pas de delta time, pas de virgule flottante — mais les concepts de base sont les mêmes.

En pratique dans la littérature retro et démo sur 6809 et machines similaires, on parle simplement de **"sync on VBL"** sans distinguer précisément où dans la boucle ça se passe.

---

## Contrainte de temps

La fenêtre VBL dure environ **1.2ms** sur MO5. À 1MHz, ça représente environ **1200 cycles CPU**.

### Coût des opérations sprite

| Opération | Octets écrits | Coût approximatif |
|---|---|---|
| `draw_sprite` 8×8    | 8  | ~50 cycles  |
| `clear_sprite` 8×8   | 8  | ~65 cycles  |
| `draw_sprite` 16×16  | 32 | ~200 cycles |
| `clear_sprite` 16×16 | 32 | ~200 cycles |
| `draw_sprite` 24×24  | 72 | ~450 cycles |
| `clear_sprite` 24×24 | 72 | ~450 cycles |

> **Note : valeurs théoriques.** Ces estimations sont calculées à partir du modèle `coût ≈ 5 cycles/octet + 3 cycles/ligne`, cohérent avec les instructions 6809 (STB = 4-5 cycles, LDX indexé = 5-6 cycles). Elles n'incluent pas le coût du switch de banque VRAM ni l'overhead d'appel de fonction. À mesurer sur émulateur pour des valeurs précises.

Pour estimer le coût d'une taille quelconque :

```
coût ≈ (W_bytes × H × 5) + (H × 3)
```

Exemple pour un sprite 4×32 (4 octets × 32 lignes) :
```
(4 × 32 × 5) + (32 × 3) = 640 + 96 = ~736 cycles
```

### Coût des autres traitements courants

| Opération | Coût approximatif | Notes |
|---|---|---|
| `mo5_wait_vbl()` | ~0 cycles utiles | Attente active — CPU bloqué jusqu'au VBL |
| Lecture touche (`mo5_getchar`) | ~10-20 cycles | Lecture registre PIA |
| `mo5_fill_rect` 40×8 | ~1000 cycles | Toute la largeur écran, 8 lignes |
| `mo5_font6_puts` (1 caractère) | ~30-50 cycles | 1 octet × 6 lignes |
| `mo5_font6_puts` (10 caractères) | ~300-500 cycles | |
| Switch banque VRAM (`*PRC`) | ~5 cycles | Par switch — draw/clear font 2 switchs |
| Accès `row_offsets[y]` | ~8 cycles | Lecture table précalculée |
| Collision AABB | ~20 cycles | 4 comparaisons d'octets |
| LFSR `pseudo_rand` | ~15 cycles | Shifts + XOR sur 8-bit |

### Budget type pour un jeu avec 4 ennemis

| Contenu | Coût estimé |
|---|---|
| 1 joueur `move_sprite` 16×16 | ~200 cycles |
| 4 ennemis `move_sprite` 16×16 | ~800 cycles |
| 3 tirs joueur `move_sprite` 8×8 | ~150 cycles |
| 4 tirs ennemis `move_sprite` 8×8 | ~200 cycles |
| Collisions (4×3 + 4×3 paires) | ~240 cycles |
| Affichage score + vies (font6) | ~400 cycles |
| **Total estimé** | **~2000 cycles** |

Ce budget dépasse la fenêtre VBL (~1200 cycles) si tout le dessin est concentré dedans. Dans ce cas deux options : utiliser la **stratégie A** (logique + dessin dans les 20ms de la trame complète), ou optimiser avec `mo5_move_sprite()` qui fusionne clear + draw en une seule passe (2 switches de banque au lieu de 4).

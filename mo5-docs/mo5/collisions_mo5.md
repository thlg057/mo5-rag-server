# Détection de collisions sur Thomson MO5

## Pourquoi c'est un sujet à part entière ?

La détection de collision est l'une des opérations les plus coûteuses d'un jeu. Sur un processeur moderne, tester toutes les paires d'objets est trivial. Sur le 6809 à 1MHz, chaque test consomme des cycles précieux, et le nombre de tests croît en **O(n²)** avec le nombre d'entités.

Il faut donc choisir la bonne stratégie selon la complexité du jeu.

---

## Stratégie 1 — AABB (Axis-Aligned Bounding Box)

C'est la stratégie de référence sur MO5. On représente chaque entité par un rectangle aligné sur les axes, et on teste si deux rectangles se chevauchent.

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

**Coût** : 4 comparaisons + 2 additions — environ **~20 cycles** par paire testée.

**Pourquoi pas le pixel-perfect ?**
La détection pixel-perfect compare les bitmaps des deux sprites octet par octet. Pour deux sprites 16×16, ça représente jusqu'à 32 comparaisons par ligne, soit ~500 paires à tester. Sur 6809, c'est inenvisageable à 50 Hz avec plusieurs entités.

**Compromis accepté** : légère imprécision aux coins des sprites, imperceptible en pratique à la résolution du MO5.

---

## Stratégie 2 — Réduction du nombre de tests

L'AABB est rapide par test, mais le nombre de paires à tester peut exploser. Avec `n` entités, le nombre de paires est `n × (n-1) / 2`.

| Entités | Paires naïves |
|---|---|
| 5 | 10 |
| 10 | 45 |
| 20 | 190 |

Plusieurs techniques permettent de réduire ce nombre sans changer l'algorithme de test lui-même.

### 2a — Tests catégoriels uniquement

Sur le MO5, il n'est généralement pas utile de tester toutes les paires possibles. On teste uniquement les combinaisons qui ont un sens dans le jeu :

| Paire testée | Pertinent ? |
|---|---|
| Tirs joueur vs ennemis | ✓ |
| Tirs ennemis vs joueur | ✓ |
| Ennemis vs joueur (contact direct) | ✓ |
| Ennemis vs ennemis | ✗ (ils ne s'éliminent pas entre eux) |
| Tirs joueur vs tirs ennemis | ✗ (les balles se croisent) |

Cette sélection réduit drastiquement le nombre de tests réels sans aucun coût algorithmique supplémentaire.

### 2b — Early exit

Dès qu'une collision est détectée, on arrête immédiatement la boucle pour cette entité. Une balle ne peut toucher qu'une cible à la fois.

```c
for (i = 0; i < MAX_BULLETS_PLAYER; i++) {
    if (!bullets_player[i].active) continue;
    for (j = 0; j < ENEMY_COUNT; j++) {
        if (!enemies[j].active) continue;
        if (collide(...)) {
            bullets_player[i].active = 0;
            enemies[j].active = 0;
            return;   // ← early exit : cette balle est consommée
        }
    }
}
```

### 2c — Skip des entités inactives

Toujours vérifier `active` avant de tester une collision. Sur un pool de 8 balles dont 2 sont actives, cela divise le nombre de tests par 4.

---

## Stratégie 3 — Invincibilité temporaire (iframes)

Après avoir subi un dégât, le joueur devient invincible pendant N frames. Pendant cette période, les tests de collision joueur vs tirs ennemis et joueur vs ennemis sont ignorés.

```
frame 0   : collision → dégât, player_invincible = 60
frame 1–59: collisions ignorées, joueur clignote
frame 60  : invincibilité terminée, collisions réactivées
```

**Double bénéfice** : mécanique de fairplay indispensable, et réduction du nombre de tests pendant les frames d'invincibilité.

**Feedback visuel** : le clignotement du joueur (alternance draw/clear tous les N frames) signale visuellement l'invincibilité sans animation dédiée ni coût supplémentaire.

---

## Stratégie 4 — Réduction des boîtes de collision

Pour améliorer la précision sans passer au pixel-perfect, on peut réduire volontairement la boîte de collision par rapport à la taille visuelle du sprite. Un sprite 16×16 peut avoir une boîte de collision de 12×12 centrée — les coins "vides" du sprite ne comptent pas.

```
Sprite visuel 16×16     Boîte de collision 12×12
┌────────────────┐       ┌──────────────┐
│                │       │              │
│   ██████████   │       │  ██████████  │
│   ██████████   │  →    │  ██████████  │
│   ██████████   │       │  ██████████  │
│                │       │              │
└────────────────┘       └──────────────┘
```

**Coût** : zéro — c'est juste un ajustement des constantes `aw`, `ah`, `bw`, `bh` passées à `collide()`. La fonction reste identique.

**Compromis** : le joueur peut visuellement "frôler" un ennemi sans être touché — ressenti souvent comme plus juste par le joueur.

---

## Stratégie 5 — Séparation logique / affichage des collisions

Les collisions sont traitées **après** la mise à jour des positions et **avant** le dessin. Cet ordre garantit que l'affichage reflète toujours l'état résolu du jeu (entités mortes déjà effacées, score mis à jour).

```
1. Lecture des entrées
2. Mise à jour des positions (logique)
3. Détection des collisions      ← ici
4. mo5_wait_vbl()
5. Dessin
```

Traiter les collisions après le dessin produirait une frame de décalage visible : l'ennemi apparaît touché un frame trop tard.

---

## Budget cycles — collisions

Pour un Space Invaders typique (4 ennemis, 3 tirs joueur, 4 tirs ennemis) :

| Tests effectués | Paires max | Coût estimé |
|---|---|---|
| Tirs joueur vs ennemis (3×4) | 12 | ~240 cycles |
| Tirs ennemis vs joueur (4×1) | 4 | ~80 cycles |
| Ennemis vs joueur (4×1) | 4 | ~80 cycles |
| **Total** | **20** | **~400 cycles** |

Ces 400 cycles s'exécutent pendant le balayage actif — pas dans la fenêtre VBL. Ils ne consomment donc pas le budget dessin.

---

## Récapitulatif des stratégies

| Stratégie | Gain | Coût d'implémentation |
|---|---|---|
| AABB | Base — rapide par test | Faible |
| Tests catégoriels uniquement | Réduit le nombre de paires | Nul |
| Early exit | Évite les tests inutiles après impact | Nul |
| Skip entités inactives | Divise les tests par le taux d'inactivité | Nul |
| Invincibilité temporaire (iframes) | Fairplay + réduction tests | Faible |
| Réduction boîte de collision | Meilleure précision perçue | Nul |
| Collisions avant dessin | Cohérence logique/affichage | Nul |

---

*Voir `vbl_mo5.md` pour le budget cycles global et le placement dans la game loop.*
*Voir `strategies_techniques_mo5.md` pour le contexte général des optimisations sur 6809.*

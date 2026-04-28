# Guide de développement sur Thomson MO5

> Conseils exhaustifs pour créer un jeu sur le MO5 (8-bit, années 80)

---

## 1. Architecture matérielle

### CPU : Motorola 6809 @ 1 MHz

Le 6809 est un processeur riche pour un 8-bit (registres 16-bit, modes d'adressage puissants), mais **1 MHz reste serré**. Chaque cycle compte.

- Favorise les opérations sur les registres `D` (A+B), `X`, `Y` plutôt que les accès mémoire répétés
- Évite les multiplications/divisions logicielles coûteuses — préfère les **décalages de bits** et les **tables de lookup**
- Le 6809 dispose d'une instruction `MUL` (8×8→16 bits) mais pas de division hardware

### Mémoire : 48 Ko RAM

La mémoire est paginée. La RAM utilisateur cohabite avec la ROM et les zones d'I/O mappées en mémoire.

| Plage         | Contenu                                      |
|---------------|----------------------------------------------|
| `$0000-$1F3F` | RAM vidéo (FORME et COULEUR, bank-switched)  |
| `$1F40-$1FFF` | Variables système (moniteur ROM)             |
| `$2000-$20FF` | Registres du moniteur RAM                    |
| `$2100-$21FF` | Registres de l'application                   |
| `$2200-$9FFF` | RAM utilisateur (~32 Ko)                     |
| `$A7C0-$A7C3` | PIA système (clavier, son, cassette, vidéo)  |
| `$A7CC-$A7CF` | PIA extension (manettes, DAC son)            |
| `$A7E4-$A7E7` | Gate-array vidéo (VBL, compteurs)            |
| `$B000-$EFFF` | Cartouche ROM                                |
| `$F000-$FFFF` | ROM moniteur                                 |

**Pas de `malloc`** — tout est statique. Planifie dès le départ l'emplacement de ton code, tes données et tes buffers.

### Outils recommandés

- **Cross-assembleur 6809** : `lwasm` (lwtools) — excellent
- **Émulateur** : MAME (profil MO5) ou Theodora
- **Workflow** : compiler sur PC → charger le binaire dans l'émulateur → itérer rapidement

Travaille directement en **assembleur 6809** pour les parties critiques (affichage, boucle principale). Du C avec un cross-compileur est envisageable pour la logique de haut niveau.

---

## 2. Affichage

### Le modèle graphique du MO5

Résolution : **320×200 pixels**, mais avec une contrainte fondamentale :

- Chaque octet de la zone **forme** (`$0000–$1F3F`) encode 8 pixels horizontaux (1 bit/pixel)
- Chaque octet a un octet **couleur** associé qui donne exactement **2 couleurs possibles** pour ces 8 pixels
- Résultat : **clash de couleurs** similaire au ZX Spectrum — impossible d'avoir plus de 2 couleurs sur un bloc de 8 pixels horizontaux

> **Conception graphique** : les sprites et décors doivent être conçus dès le départ en tenant compte de cette contrainte. Utilise un outil qui simule le clash couleur.

### Synchronisation VBL (Vertical Blank)

Le MO5 génère une **interruption à chaque retour de trame (50 Hz en PAL)**. C'est le point de synchronisation central du jeu.

Structure de boucle recommandée :

```
LOOP:
    WAIT_VBL         ; attendre l'interruption VBL
    READ_INPUTS      ; lire clavier/joystick (bufferisé par IRQ)
    UPDATE_LOGIC     ; mettre à jour la logique de jeu
    DRAW             ; dessiner
    GOTO LOOP
```

Sans synchronisation sur le VBL : tearing visuel et comportement non déterministe.

**Budget CPU par frame** : à 1 MHz / 50 Hz → **20 000 cycles par frame**. Profile tôt et souvent.

### Double buffering et dirty rectangles

La RAM limitée rend le double buffering complet difficile. Alternative recommandée : le **dirty rectangle**.

- Maintenir une liste des zones modifiées à chaque frame
- Effacer et redessiner **uniquement** ces zones
- Réduction significative du coût CPU d'affichage

### Sprites logiciels

Le MO5 n'a **pas de hardware sprites**. Tout est dessiné en logiciel :

- Utilise des **masques (AND/OR)** pour la transparence
- **Aligne tes sprites sur des frontières de 8 pixels** pour éviter les décalages coûteux
- Si tu dois décaler au pixel près, **précalcule les 8 versions décalées** en ROM/RAM et utilise une table
- Limite le nombre de sprites (5–10 selon leur taille) pour respecter le budget CPU

---

## 3. Entrées clavier et joystick

### Lecture clavier via PIA 6821

Le clavier est organisé en **matrice** scannable via la PIA 6821. Il faut le scanner manuellement.

**Bonne pratique** : scanner la matrice dans l'**interruption VBL**, stocker l'état dans des variables, et lire ces variables depuis la boucle principale.

Cela permet :
- Une lecture cohérente sans rebond entre deux frames
- La **détection d'edge** (touche venant d'être pressée) en comparant état courant et précédent
- La gestion de l'**autorepeat** avec un simple compteur

### Joystick

Les joysticks Thomson sont **analogiques** (potentiomètres) lus via le convertisseur A/N interne — ce qui prend du temps CPU.

- Lire dans le VBL, pas nécessairement à chaque frame
- Tous les 2–3 frames peut suffire pour un jeu d'action

### Pas de multi-process

Toute la logique doit être une **machine à états**. Aucun thread disponible. Si une opération est longue (décompression, chargement), elle bloque tout.

> Découpe les traitements longs en petites tranches exécutées sur plusieurs frames.

---

## 4. Son

Le MO5 dispose d'un son très basique : **un bit de sortie** (bip) géré via la PIA, et optionnellement un **DAC 6-bit** via le port cartouche.

### Son bloquant (simple)

Les effets sonores simples (bip, explosion) se font en toggleant le bit son à la bonne fréquence dans une boucle serrée — mais **cela bloque le CPU**.

### Son non bloquant (avancé)

Pour du son pendant le jeu sans bloquer, générer le son dans l'interruption VBL ou Timer :

- Avancer d'un pas par frame dans une séquence musicale
- Technique PWM possible pour des sons plus riches

> **Conseil** : si tu vises de la musique en tâche de fond, conçois un "player" appelé dans le VBL dès le début du projet.

---

## 5. Optimisations et techniques avancées

### Tables précalculées

La multiplication hardware du 6809 est limitée (MUL : 8×8→16 bits). Pour tout calcul répétitif :

- Trigonométrie, trajectoires paraboliques, distances → **tables en ROM**
- Coût en mémoire mais gain de vitesse incomparable

**Exemple fondamental : `row_offsets`**

Sans table, chaque accès à une ligne d'écran nécessite une multiplication :

```c
// ❌ Sans table : multiplication à chaque accès
offset = (ty + i) * 40 + tx;   // coûteux sur 6809
```

Avec une table précalculée une seule fois au démarrage :

```c
// ✅ Avec table : simple accès indexé
unsigned int row_offsets[200];
for (int y = 0; y < 200; y++) {
    row_offsets[y] = y * 40;    // 320px / 8px par byte = 40 bytes/ligne
}

// Ensuite dans le code d'affichage :
offset = row_offsets[ty + i] + tx;  // accès tableau = quelques cycles seulement
```

C'est l'optimisation la plus rentable pour tout code d'affichage sur MO5 — à mettre en place dès le début.

### Optimisation des sprites : la fonction `move`

#### Le problème du clear + draw naïf

Pour un sprite **16×16** qui se déplace de 8 pixels, l'approche naïve (clear total puis draw total) réécrit inutilement la zone de recouvrement :

```
Déplacement → de 8px (1 byte)

Avant :          Après clear :    Après draw :
┌──┬──┐          ┌──┬──┐          ┌──┬──┐
│A │B │   →→→   │  │  │   →→→   │  │A │
└──┴──┘          └──┴──┘          └──┴──┘
                  ^^^^  tout       ^^^^  tout
                  clearé           redessiné
                  dont B           dont A (inutile de l'avoir clearé)
```

La colonne B a été clearée puis A a été dessinée au même endroit — **un clear inutile**.

#### Principe de la fonction `move` optimisée

Ne clearer que la zone **hors recouvrement** entre l'ancienne et la nouvelle position, puis dessiner le nouveau sprite en entier.

```
Déplacement →       Déplacement ←       Déplacement ↓       Déplacement ↑
┌──┬──┐ ┌──┬──┐    ┌──┬──┐ ┌──┬──┐    ┌──────────┐         ┌──────────┐
│  │  │ │  │  │    │  │  │ │  │  │    │ old only │ ← clear  │  NEW     │ ← draw
└──┴──┘ └──┴──┘    └──┴──┘ └──┴──┘    ├──────────┤         ├──────────┤
 old     new         new    old        │  BOTH    │ ← skip  │  BOTH    │ ← skip
│clear│  │draw│    │draw│  │clear│    ├──────────┤         ├──────────┤
 col0    col1        col0    col1      │  NEW     │ ← draw  │ old only │ ← clear
                                       └──────────┘         └──────────┘
```

#### Gain réel pour un sprite 16×16, déplacement horizontal

| Approche | Écritures VRAM couleur | Écritures VRAM forme | Total |
|---|---|---|---|
| Clear + Draw naïf | 32 + 32 = 64 | 32 + 32 = 64 | **128** |
| `move` optimisé | 16 + 32 = 48 | 16 + 32 = 48 | **96** |
| Économie | | | **~25%** |

#### Quand utiliser `move` vs clear + draw ?

- **`move`** : pour le sprite du joueur ou tout sprite se déplaçant de façon continue chaque frame
- **Clear + Draw classique** : pour les apparitions/disparitions, téléportations, ou sprites 8×8 (le gain est négligeable)
- **Règle générale** : ne pas optimiser prématurément — commencer par le clear + draw classique, mesurer, et n'introduire `move` que si le budget CPU est réellement dépassé

#### Limites de `move`

- Suppose un déplacement **au plus égal à la taille du sprite** (sinon pas de recouvrement → fallback automatique)
- Pour un déplacement **diagonal** (dx≠0 ET dy≠0 simultanément), les coins de l'ancienne zone créent un cas complexe — il vaut mieux faire deux appels séparés move_x puis move_y

### Compression des données

Graphismes, niveaux et musique doivent être **compressés**.

- **RLE (Run-Length Encoding)** : facile à décompresser en 6809, très efficace sur les graphismes avec grandes zones uniformes
- D'autres algorithmes légers peuvent être envisagés selon la nature des données

### Carte mémoire statique

Planifie précisément l'usage de la mémoire :

| Zone              | Usage suggéré                         |
|-------------------|---------------------------------------|
| `$2200-$9FFF`     | Code, données, buffers (RAM user)     |
| `$B000-$EFFF`     | Code final, tables, graphismes (ROM)  |
| Stack             | Sommet de la RAM user ($9FFF desc.)   |

---

## 6. Contraintes à anticiper dès la conception

| Contrainte              | Implication                                              |
|-------------------------|----------------------------------------------------------|
| 20 000 cycles/frame     | Profiler tôt, optimiser en assembleur les parties chaudes |
| Clash couleur (8px/2col)| Concevoir les graphismes avec un outil adapté            |
| Pas de hardware sprites | Tout en logiciel — limiter le nombre de sprites          |
| Pas de multi-process    | Architecture en machine à états obligatoire              |
| Mémoire statique        | Cartographier la mémoire dès le début                    |
| Son bloquant par défaut | Prévoir un player VBL si musique en tâche de fond        |
| Joystick analogique lent| Ne pas lire à chaque frame                               |
| Sauvegarde difficile    | Cassette (lente), ou cartouche RAM pour distribution     |

---

## 7. Ressources et références

- **Référence matérielle** : documentation Thomson MO5 (PIA 6821, schémas)
- **Cross-assembleur** : [lwtools / lwasm](http://www.lwtools.ca/)
- **Émulateur** : [MAME](https://www.mamedev.org/) (profil `mo5`) ou Theodora
- **Référence CPU** : Motorola 6809 Programming Reference Manual
- **Communauté** : forums MO5.com, archives DCMOTO

---

*Document rédigé comme aide-mémoire pour le développement de jeu sur Thomson MO5.*

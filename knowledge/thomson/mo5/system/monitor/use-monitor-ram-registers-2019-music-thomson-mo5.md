# Use Monitor RAM Registers $2019 and Music Registers (Thomson MO5)

Le moniteur ROM du MO5 utilise une zone RAM (`$2000–$20FF`) pour stocker ses paramètres. Certains de ces registres sont directement utiles depuis le C.

Source : Guide du MO5 p.275-277 + Clefs Pour MO5 p.91-94 — Fiabilité ÉLEVÉE.

## Registre STATUS ($2019) — le plus utile

| Bit | Fonction |
|-----|----------|
| 0 | Touche clavier déjà lue |
| 1 | Répétition clavier active |
| 2 | État curseur (0=invisible, 1=visible) |
| **3** | **Bruitage clavier : 0=activé, 1=désactivé** ★ |
| 4 | Lecture clavier graphique sans écriture couleur |
| 5 | Scroll caractère sans couleur |
| 6 | 0=majuscule, 1=minuscule |
| 7 | 0=simple hauteur, 1=double hauteur |

### Couper le bip clavier (méthode recommandée)

```c
/* Source : Clefs Pour MO5 p.110 et p.118 */
/* En BASIC : POKE &H2019, PEEK(&H2019) + 8 */
*((unsigned char*)0x2019) |= 0x08;   /* mute */
*((unsigned char*)0x2019) &= ~0x08;  /* unmute */
```

Cette méthode agit au niveau du moniteur ROM — plus propre que d'agir sur PB0 `$A7C1` qui partage le registre avec la matrice clavier.

## Registres musique ($2039–$203F)

Utilisés par le SWI $1E (génération de musique). À initialiser avant chaque appel.

| Adresse | Nom | Description |
|---------|-----|-------------|
| `$2039–$203A` | TEMPO | Tempo (1=prestissimo, 5=allegretto std, 255=très lent) |
| `$203B–$203C` | DUREE | Durée de la note (96=ronde, 24=noire, 12=croche) |
| `$203D` | TIMBRE | Attaque (0=legato/continu, 200=staccato/piqué) |
| `$203E–$203F` | OCTAVE | Octave 1–5 (octave 4 = LA 440 Hz) |

```c
/* Exemple : initialiser pour noire legato allegro octave 4 */
*((unsigned char*)0x2039) = 0;   /* TEMPO haut */
*((unsigned char*)0x203A) = 4;   /* TEMPO bas = allegro */
*((unsigned char*)0x203B) = 0;   /* DUREE haut */
*((unsigned char*)0x203C) = 24;  /* DUREE bas = noire */
*((unsigned char*)0x203D) = 0;   /* TIMBRE = legato */
*((unsigned char*)0x203E) = 0;   /* OCTAVE haut */
*((unsigned char*)0x203F) = 4;   /* OCTAVE bas = 4 (LA440) */
```

## Autres registres utiles

| Adresse | Nom | Description |
|---------|-----|-------------|
| `$2061–$2062` | TIMEPT | Adresse routine IRQ 50Hz personnalisée |
| `$2063` | — | Mettre ≠0 pour activer l'aiguillage IRQ |
| `$206D–$206E` | CHRPTR | Pointeur table décodage clavier (remplaçable) |
| `$2073–$2074` | GENPTR | Pointeur générateur caractères (remplaçable) |
| `$2076` | LATCLV | Latence répétition clavier en 1/10 sec (défaut=7) |

### Exemple : ralentir la répétition clavier

```c
*((unsigned char*)0x2076) = 20;  /* latence 2 secondes */
```

Source: `mo5_hardware_reference.md` sections 10 et 11

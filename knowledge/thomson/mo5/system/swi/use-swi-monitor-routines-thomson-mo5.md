# Use SWI Monitor Routines (Thomson MO5)

Toutes les routines du moniteur ROM sont accessibles via l'instruction `SWI` suivie d'un code 1 octet. Deux codes par fonction : JSR (retour après) et JMP (retour avant).

Source : Guide du MO5 p.274 + Clefs Pour MO5 p.79-80 — Fiabilité ÉLEVÉE.

## Table complète des codes SWI

| Code JSR | Code JMP | Fonction |
|----------|----------|----------|
| `$02` | `$82` | Affichage d'un caractère (B = code ASCII) |
| `$04` | `$84` | Mise en mémoire couleur |
| `$06` | `$86` | Mise en mémoire forme |
| `$08` | `$88` | **Bip sonore** |
| `$0A` | `$8A` | Lecture clavier (retourne ASCII dans B) |
| `$0C` | `$8C` | Lecture rapide clavier (bit Z du CC) |
| `$0E` | `$8E` | Tracé d'un segment de droite |
| `$10` | `$90` | Allumage/extinction d'un point |
| `$12` | `$92` | Écriture d'un point caractère |
| `$14` | `$94` | Lecture couleur d'un point |
| `$16` | `$96` | Lecture bouton crayon optique |
| `$18` | `$98` | Lecture crayon optique (coordonnées) |
| `$1A` | `$9A` | Lecture de l'écran |
| `$1C` | `$9C` | **Lecture manettes de jeu** |
| `$1E` | `$9E` | **Génération de musique** |
| `$20` | `$A0` | Lecture/écriture cassette |
| `$22` | `$A2` | Moteur LEP (marche/arrêt) |
| `$24` | `$A4` | Interface communication |
| `$26` | `$A6` | Contrôleur disques |

## Appel depuis C (cmoc)

```c
/* Bip sonore */
asm { swi \n fcb $08 }

/* Lecture clavier (retourne ASCII dans B) */
asm { swi \n fcb $0A }
```

## SWI $1C — Lecture manettes de jeu

Source : Guide du MO5 p.260.

**Entrée :** registre A = numéro de manette (0 ou 1)

**Retour :**
- registre B = position (0–8) selon table cardinale
- bit C (retenue) du CC = 1 si bouton fire enfoncé

| Valeur B | Direction |
|----------|-----------|
| 0 | Centre (neutre) |
| 1 | Nord (haut) |
| 2 | Nord-Est |
| 3 | Est (droite) |
| 4 | Sud-Est |
| 5 | Sud (bas) |
| 6 | Sud-Ouest |
| 7 | Ouest (gauche) |
| 8 | Nord-Ouest |

## SWI $1E — Génération de musique

Source : Guide du MO5 p.262-263.

**Entrée :** registre B = code de note + registres RAM initialisés.

### Registres RAM à configurer avant l'appel

| Adresse | Nom | Valeur |
|---------|-----|--------|
| `$2039–$203A` | TEMPO | 1=rapide, 5=standard, 255=lent |
| `$203B–$203C` | DUREE | 96=ronde, 48=blanche, 24=noire, 12=croche |
| `$203D` | TIMBRE | 0=legato, 200=staccato |
| `$203E–$203F` | OCTAVE | 1=grave … 5=aigu (4 = LA 440 Hz) |

### Codes de notes (registre B)

| Note | Code | Note | Code |
|------|------|------|------|
| SILENCE | `$00` | SOL | `$08` |
| DO | `$01` | SOL# | `$09` |
| DO# | `$02` | LA | `$0A` |
| RE | `$03` | LA# | `$0B` |
| RE# | `$04` | SI | `$0C` |
| MI | `$05` | UT (DO+1oct) | `$0D` |
| FA | `$06` | — | — |
| FA# | `$07` | — | — |

### Durées (valeur DUREE)

| Note | Valeur | Note | Valeur |
|------|--------|------|--------|
| Ronde | 96 | Croche pointée | 18 |
| Blanche pointée | 72 | Croche | 12 |
| Blanche | 48 | Double croche | 6 |
| Noire pointée | 36 | Triple croche | 3 |
| Noire (std) | 24 | Noire triolet | 16 |

### Exemple C complet

```c
/* Jouer DO noire, octave 4, legato, allegro */
*((unsigned char*)0x2039) = 0;
*((unsigned char*)0x203A) = 4;    /* tempo allegro */
*((unsigned char*)0x203B) = 0;
*((unsigned char*)0x203C) = 24;   /* noire */
*((unsigned char*)0x203D) = 0;    /* legato */
*((unsigned char*)0x203E) = 0;
*((unsigned char*)0x203F) = 4;    /* octave 4 */
asm {
    ldb #$01   /* DO */
    swi
    fcb $1E
}
```

> ⚠️ Le SWI $1E est **bloquant** — préférer `mo5_music_swi.h` qui encapsule ces appels.

Source: `mo5_hardware_reference.md` section 11

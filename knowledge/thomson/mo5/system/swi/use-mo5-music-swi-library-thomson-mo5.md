# Use mo5_music_swi Library for Musical Notes (Thomson MO5)

`mo5_music_swi` encapsule le SWI $1E du moniteur pour jouer des notes musicales en notation standard (Do–Si, durées, octaves, timbre, tempo).

## Quand utiliser mo5_music_swi vs mo5_audio

| | `mo5_audio` | `mo5_music_swi` |
|---|---|---|
| Mécanisme | Buzzer PIA direct (bit PB0) | SWI $1E moniteur ROM |
| Notes | Fréquences libres (half_period) | Do–Si chromatique + silence |
| Timbre | Onde carrée pure | Enveloppe ROM fixe |
| Bloquant | Oui | Oui |
| Usage | Effets custom, mélodies libres | Mélodies en notation musicale |

## Inclusion

```c
#include "mo5_music_swi.h"
```

## Constantes de notes

```c
MO5_NOTE_SILENCE   MO5_NOTE_DO    MO5_NOTE_DOS   /* DO# */
MO5_NOTE_RE        MO5_NOTE_RES   /* RE# */       MO5_NOTE_MI
MO5_NOTE_FA        MO5_NOTE_FAS   /* FA# */       MO5_NOTE_SOL
MO5_NOTE_SOLS      /* SOL# */     MO5_NOTE_LA     MO5_NOTE_LAS  /* LA# */
MO5_NOTE_SI        MO5_NOTE_UT    /* DO+1 octave */
```

## Constantes de durées

```c
MO5_DUR_RONDE       /* 96 */    MO5_DUR_BLANCHE_P  /* 72 */
MO5_DUR_BLANCHE     /* 48 */    MO5_DUR_NOIRE_P    /* 36 */
MO5_DUR_NOIRE       /* 24 */    MO5_DUR_CROCHE_P   /* 18 */
MO5_DUR_CROCHE      /* 12 */    MO5_DUR_DCROCHE    /*  6 */
MO5_DUR_TCROCHE     /*  3 */
/* Triolets */
MO5_DUR_NOIRE_TRIO  /* 16 */    MO5_DUR_CROCHE_TRIO /* 8 */
```

## Constantes de tempo

```c
MO5_TEMPO_PRESTISSIMO  /*  1 */   MO5_TEMPO_ALLEGRO    /*  4 */
MO5_TEMPO_ALLEGRETTO   /*  5 */   MO5_TEMPO_MODERATO   /*  8 */
MO5_TEMPO_ANDANTE      /* 16 */   MO5_TEMPO_ADAGIO     /* 32 */
MO5_TEMPO_LARGO        /* 64 */
```

## Constantes de timbre

```c
MO5_TIMBRE_LEGATO    /*   0 */   MO5_TIMBRE_NORMAL   /*  10 */
MO5_TIMBRE_DETACHE   /* 100 */   MO5_TIMBRE_STACCATO /* 200 */
```

## Octaves

```c
MO5_OCT_1  /* grave */  MO5_OCT_2  MO5_OCT_3
MO5_OCT_4  /* LA440 */  MO5_OCT_5  /* aigu */
```

## Fonctions

```c
/* Jouer une note unique */
void mo5_swi_play_note(unsigned char note,   unsigned char duree,
                       unsigned char octave, unsigned char timbre,
                       unsigned char tempo);

/* Jouer un silence */
void mo5_swi_silence(unsigned char duree, unsigned char tempo);

/* Jouer un tableau de notes (terminé par {0,0,0,0}) */
void mo5_swi_play_melody(const MO5_Note *melody, unsigned char tempo);
```

## Structure MO5_Note

```c
typedef struct {
    unsigned char note;    /* MO5_NOTE_xxx */
    unsigned char duree;   /* MO5_DUR_xxx */
    unsigned char octave;  /* MO5_OCT_1 à MO5_OCT_5 */
    unsigned char timbre;  /* MO5_TIMBRE_xxx */
} MO5_Note;
```

## Exemples

### Note unique

```c
/* DO noire, octave 4, legato, allegro */
mo5_swi_play_note(MO5_NOTE_DO, MO5_DUR_NOIRE, MO5_OCT_4,
                  MO5_TIMBRE_LEGATO, MO5_TEMPO_ALLEGRO);
```

### Mélodie complète

```c
static const MO5_Note fanfare[] = {
    { MO5_NOTE_DO,  MO5_DUR_CROCHE, MO5_OCT_4, MO5_TIMBRE_NORMAL },
    { MO5_NOTE_MI,  MO5_DUR_CROCHE, MO5_OCT_4, MO5_TIMBRE_NORMAL },
    { MO5_NOTE_SOL, MO5_DUR_CROCHE, MO5_OCT_4, MO5_TIMBRE_NORMAL },
    { MO5_NOTE_UT,  MO5_DUR_NOIRE,  MO5_OCT_4, MO5_TIMBRE_LEGATO },
    { 0, 0, 0, 0 }  /* fin du tableau */
};
mo5_swi_play_melody(fanfare, MO5_TEMPO_ALLEGRO);
```

> ⚠️ Toutes les fonctions sont **bloquantes**. Ne pas appeler dans la game loop.

Source: `mo5_music_swi.h` + `mo5_hardware_reference.md` section 11.2

# Initialize Graphics Mode (Thomson MO5)

Activer le mode bitmap et préparer la VRAM pour le rendu graphique.

## Fonction SDK

```c
#include "mo5_video.h"

mo5_video_init(COLOR(C_BLACK, C_WHITE));
```

## Ce que fait mo5_video_init

1. Précalcule la table `row_offsets[200]` (évite `y * 40` à l'exécution)
2. Active le mode bitmap
3. Remplit la banque COULEUR avec la couleur passée en paramètre
4. Efface la banque FORME (tout à zéro = pixels éteints)

## Paramètre — couleur initiale

```c
/* Fond noir, forme noire (écran vide) */
mo5_video_init(COLOR(C_BLACK, C_BLACK));

/* Fond bleu, forme jaune (couleur de jeu) */
mo5_video_init(COLOR(C_BLUE, C_YELLOW));

/* Fond noir, forme blanche (texte standard) */
mo5_video_init(COLOR(C_BLACK, C_WHITE));
```

## Séquence de démarrage typique

```c
int main(void)
{
    mo5_video_init(COLOR(C_BLACK, C_BLACK));  /* 1. init vidéo */
    mo5_mute_beep();                          /* 2. couper bip */
    mo5_joystick_init();                      /* 3. init joystick si nécessaire */

    /* ... initialiser sprites, acteurs, etc. */

    while (1) {
        mo5_wait_vbl();
        /* game loop */
    }
    return 0;
}
```

## Erreurs fréquentes

```text
❌ Ne pas appeler mo5_video_init → rien ne s'affiche correctement
❌ Appeler plusieurs fois → réinitialise row_offsets (coûteux, inutile)
❌ Écrire en VRAM avant mo5_video_init → banque non sélectionnée
```

Source: `mo5_video.h` + `mo5_hardware_reference.md` section 3

# Initialiser la vidéo MO5 et effacer l'écran avec le SDK `mo5_video` (Thomson MO5)

Ce chunk documente `mo5_video_init` et `mo5_clear_screen` définies dans `mo5_video.h`.

## Goal

- Initialiser le mode graphique du MO5 via le SDK.
- Effacer l'écran avec une couleur uniforme.

## Prototype `mo5_video_init`

```c
void mo5_video_init(unsigned char color);
```

- À appeler **une seule fois au démarrage**.
- Active le mode bitmap.
- Précalcule la table `row_offsets`.
- Remplit la banque couleur avec `color`.
- Efface la banque forme.

Exemple :

```c
mo5_video_init(COLOR(C_BLACK, C_BLACK));
```

## Effacer l'écran : `mo5_clear_screen`

```c
void mo5_clear_screen(unsigned char color);
```

- Remplit tout l'écran avec `color`.
- Utile pour les transitions de niveaux ou pour réinitialiser le fond.

Exemples :

```c
mo5_clear_screen(COLOR(C_BLACK, C_BLACK));  // écran noir
mo5_clear_screen(COLOR(C_BLUE,  C_BLUE));   // fond bleu
```

## Notes

- Appelle toujours `mo5_video_init` avant toute autre fonction vidéo ou sprite du SDK.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_video_h.md`

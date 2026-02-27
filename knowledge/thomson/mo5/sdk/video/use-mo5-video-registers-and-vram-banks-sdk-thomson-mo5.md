# Utiliser les registres vidéo et banques VRAM du SDK `mo5_video` (Thomson MO5)

Ce chunk décrit les macros de registres matériels et la sélection de banque VRAM fournies par `mo5_video.h`.

## Goal

- Comprendre le rôle de `PRC`, `VIDEO_REG` et `VRAM`.
- Sélectionner correctement la banque forme ou couleur avant d'écrire en VRAM.

## Régistres matériels

```c
#define PRC       ((volatile unsigned char*)0xA7C0)
#define VIDEO_REG ((volatile unsigned char*)0xA7E7)
#define VRAM      ((volatile unsigned char*)0x0000)
```

- `PRC` : sélection de la banque VRAM (bit 0 : 0 = couleur, 1 = forme).
- `VIDEO_REG` : registre mode vidéo / status VBL.
- `VRAM` : base de la mémoire vidéo.

## Sélection de banque VRAM

```c
*PRC &= ~0x01;   // sélectionner la banque COULEUR
*PRC |=  0x01;   // sélectionner la banque FORME
```

Toujours sélectionner la banque adaptée **avant** de lire ou écrire dans `VRAM`.

## Notes

- `mo5_video` est la couche de base sur laquelle repose tout le SDK graphique (`mo5_sprite`, etc.).

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_video_h.md`

# Map RAM and ROM Address Space (Thomson MO5)

Carte mémoire de référence pour placer code, données et buffers sans chevaucher les zones système.

Source : Manuel Technique MO5 p.60 + Guide du MO5 p.273 + Clefs Pour MO5 p.78 — Fiabilité ÉLEVÉE.

## Carte mémoire complète

| Plage | Taille | Contenu |
|-------|--------|---------|
| `$0000–$1F3F` | 8 Ko | RAM vidéo (FORME et COULEUR, bank-switched via `$A7C0` bit 0) |
| `$1F40–$1FFF` | 192 o | Variables système moniteur |
| `$2000–$20FF` | 256 o | Registres RAM moniteur |
| `$2100–$21FF` | 256 o | Registres application |
| `$2200–$9FFF` | ~32 Ko | **RAM utilisateur** (code + données + pile) |
| `$A000–$A7BF` | ~2 Ko | DOS (disquettes uniquement) |
| `$A7C0–$A7C3` | 4 o | PIA système (clavier, son, cassette, banque vidéo) |
| `$A7CC–$A7CF` | 4 o | PIA extension (manettes, DAC) |
| `$A7E4–$A7E7` | 4 o | Gate-array vidéo (VBL, compteurs) |
| `$B000–$EFFF` | 16 Ko | Cartouche ROM |
| `$F000–$FFFF` | 4 Ko | ROM moniteur |

## Zones de code recommandées

```text
Code CMOC par défaut : $2800 (option --org=0x2800)
Données (--data)     : après le code
Pile                 : sommet de la RAM user ($9FFF descendant)
```

## Ce qu'il ne faut PAS écraser

```text
❌ $0000–$1F3F  → RAM vidéo active (écriture directe via bank-switch uniquement)
❌ $2000–$20FF  → Registres moniteur (certains sont utiles — voir section dédiée)
❌ $A7C0–$A7FF  → Registres I/O matériels
❌ $F000–$FFFF  → ROM moniteur (en lecture seule)
```

## Corrections fréquentes de la documentation

> ⚠️ Certaines documentations indiquent `$0000–$7FFF` comme "user RAM" — **c'est inexact**.
> La zone `$0000–$1F3F` est réservée à la VRAM.
> La RAM utilisateur réelle commence à `$2200`.

> ⚠️ La ROM moniteur est en `$F000–$FFFF`, **pas en `$C000–$FFFF`**.
> C'est différent du TO7 et de nombreux autres systèmes 6809.

Source: `mo5_hardware_reference.md` section 2

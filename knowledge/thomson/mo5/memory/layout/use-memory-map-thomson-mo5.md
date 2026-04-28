# Use Memory Map (Thomson MO5)

Carte mémoire complète du Thomson MO5. Référence pour placer code, données et buffers.

## Carte mémoire complète

Source : Manuel Technique MO5 p.60 + Guide du MO5 p.273 + Clefs Pour MO5 p.78 — Fiabilité ÉLEVÉE.

| Plage | Contenu |
|-------|---------|
| `$0000–$1F3F` | RAM vidéo (FORME et COULEUR, bank-switched via `$A7C0` bit 0) |
| `$1F40–$1FFF` | Variables système (moniteur ROM) |
| `$2000–$20FF` | Registres du moniteur RAM |
| `$2100–$21FF` | Registres de l'application |
| `$2200–$9FFF` | RAM utilisateur (~32 Ko) |
| `$A000–$A7BF` | DOS (si disquettes connectées) |
| `$A7C0–$A7C3` | PIA système 6821 (clavier, son buzzer, cassette, banque vidéo) |
| `$A7C4–$A7CB` | Libre |
| `$A7CC–$A7CF` | PIA extension 6821 jeux/musique (manettes, DAC son) |
| `$A7D0–$A7DF` | Contrôleur de disquettes |
| `$A7E0–$A7E3` | PIA interface communication (imprimante parallèle) |
| `$A7E4–$A7E7` | Gate-array vidéo — compteurs + VBL (`$A7E7` bit 7) |
| `$A7E8–$A7FF` | Extensions |
| `$A800–$AFFF` | Libre (2 Ko) |
| `$B000–$EFFF` | Cartouche ROM (16 Ko) |
| `$F000–$FFFF` | ROM moniteur (4 Ko) |

## Contraintes

- **Pas de `malloc`** — toute allocation est statique
- Budget utilisateur typique : `$2200`–`$9FFF` soit ~32 Ko, dont une partie pour pile et variables système
- Planifier l'occupation mémoire dès le début du projet

## Registres clés du moniteur RAM ($2000–$20FF)

Source : Guide du MO5 p.275-277 — Fiabilité ÉLEVÉE.

| Adresse | Nom | Description |
|---------|-----|-------------|
| `$2019` | STATUS | bit 3 = bruitage clavier (0=actif, 1=désactivé) |
| `$2029` | FORME | Couleur courante pour tracé graphique |
| `$202B` | COLOUR | Couleur courante (FFFFBBBB) |
| `$2039–$203A` | TEMPO | Tempo musique SWI (5=standard, 1=rapide) |
| `$203B–$203C` | DUREE | Durée note musique SWI (24=noire, 96=ronde) |
| `$203D` | TIMBRE | Attaque musique SWI (0=legato, 200=staccato) |
| `$203E–$203F` | OCTAVE | Octave musique SWI (1–5, 4=LA440) |
| `$2061–$2062` | TIMEPT | Adresse routine IRQ 50Hz personnalisée |
| `$2063` | — | ≠0 pour activer l'aiguillage IRQ personnalisé |
| `$206D–$206E` | CHRPTR | Pointeur table décodage clavier (remplaçable) |
| `$2073–$2074` | GENPTR | Pointeur générateur caractères standard (remplaçable) |
| `$2076` | LATCLV | Latence répétition clavier (1/10 sec, défaut=7) |

Source: `mo5_hardware_reference.md` sections 2 et 10

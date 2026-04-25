# Use Memory Map (Thomson MO5)

Carte mémoire complète du Thomson MO5. Référence pour placer code, données et buffers.

## Carte mémoire

| Plage | Contenu |
|-------|---------|
| `$0000–$1F3F` | RAM vidéo (FORME et COULEUR, bank-switched via `$A7C0` bit 0) |
| `$1F40–$1FFF` | Variables système (moniteur ROM) |
| `$2000–$3FFF` | RAM utilisateur basse |
| `$4000–$7FFF` | ROM Basic / extension cartouche |
| `$A7C0–$A7C3` | PIA système (clavier, son buzzer, cassette, banque vidéo) |
| `$A7CC–$A7CF` | PIA extension jeux/musique (manettes, DAC son) |
| `$A7E4–$A7E7` | Gate-array vidéo (dont VBL à `$A7E7` bit 7) |
| `$B000–$EFFF` | Cartouche / extension ROM |
| `$C000–$FFFF` | ROM moniteur |

## Contraintes

- **Pas de `malloc`** — toute allocation est statique
- Budget utilisateur typique : ~24–28 Ko effectifs pour code + données + buffers
- Planifier l'occupation mémoire dès le début du projet

## Notes

- La RAM vidéo `$0000–$1F3F` est partagée avec le CPU via bank-switch
- Le registre U du 6809 est initialisé à `$A7C0` par le moniteur ROM à chaque appel SWI

Source: `mo5_hardware_reference.md` section 2

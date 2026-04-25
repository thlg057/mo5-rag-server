# Use PIA System Registers A7C0-A7C3 (Thomson MO5)

Le PIA système (Motorola 6821) est mappé à `$A7C0`. Il gère la banque vidéo, le buzzer, le clavier et la cassette.

## Architecture 6821 — règle d'accès DDR/OR

| Offset | Registre | Condition d'accès |
|--------|----------|-------------------|
| +0 | ORA / DDRA | CRA bit 2 = 1 → ORA (données) / = 0 → DDRA (direction) |
| +1 | ORB / DDRB | CRB bit 2 = 1 → ORB (données) / = 0 → DDRB (direction) |
| +2 | CRA | Registre de contrôle A |
| +3 | CRB | Registre de contrôle B |

> À la mise sous tension : DDR = 0 → toutes broches en entrée.
> Initialiser le DDR avant tout usage en sortie.

## $A7C0 — PORTA

| Bit | Direction | Fonction |
|-----|-----------|----------|
| 0 | Sortie | **Sélection banque vidéo** : 0 = FORME, 1 = COULEUR |
| 1 | Sortie | Signal cassette (écriture) |
| 2 | Entrée | Signal cassette (lecture) |
| 3 | Entrée | Bouton crayon optique |
| 4 | — | Moteur cassette (via CA2/CRA) |

> ⚠️ Toujours utiliser un masque — modifier bit 0 seul sans toucher les autres.

## $A7C1 — PORTB ★ BUZZER

| Bit | Direction | Fonction |
|-----|-----------|----------|
| 0 | Sortie | **Son buzzer 1-bit** (bip clavier ROM et sons) |
| 1–7 | Entrée | Lignes matrice clavier |

Source MAME confirmée : `m_pia_sys->writepb_handler().set("buzzer", ...)`

```c
/* Couper le bip clavier */
unsigned char val = *((unsigned char*)0xA7C1);
val &= ~0x01;
*((unsigned char*)0xA7C1) = val;
```

## $A7C2 — CRA

| Bits | Fonction |
|------|----------|
| 0–1 | Contrôle entrée CA1 et masque IRQ |
| 2 | 0 = accès DDRA / 1 = accès ORA |
| 3–5 | Contrôle sortie CA2 (moteur cassette) |
| 6–7 | Flags IRQ CA2/CA1 (lecture seule) |

## $A7C3 — CRB

Même structure que CRA pour le port B.
IRQ B → **interruption principale du 6809**.

> ⚠️ Différence TO7/MO5 : c'est **IRQB** (pas IRQA) qui génère l'IRQ principale.
> Source MAME : `// WARNING: differs from TO7 !`

Source: `mo5_hardware_reference.md` section 6 — Fiabilité ÉLEVÉE (Manuel + MAME + EPI)

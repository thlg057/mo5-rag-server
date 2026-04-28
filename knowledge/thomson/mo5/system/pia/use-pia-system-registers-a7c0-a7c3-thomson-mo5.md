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

Source : Manuel Technique MO5 p.18 + p.41-42 + Guide du MO5 p.278 — Fiabilité ÉLEVÉE.

| Bit | Direction | Fonction |
|-----|-----------|----------|
| 0 | Sortie | **Sélection banque vidéo** : 0 = COULEUR, 1 = FORME |
| 1 | Sortie | Couleur du tour (bordure écran) — **Rouge** |
| 2 | Sortie | Couleur du tour — **Vert** |
| 3 | Sortie | Couleur du tour — **Bleu** |
| 4 | Sortie | Couleur du tour — **demi-teinte** (pastel) |
| 5 | Entrée | Bouton crayon optique |
| 6 | Sortie | **Écriture cassette** — broche 5 LEP (PA6) |
| 7 | Entrée | **Lecture cassette** — broche 4 LEP (PA7) |

> ⚠️ Les bits 1-4 encodent la couleur de la bordure en RVB+demi-teinte.
> Toujours utiliser un masque pour ne modifier qu'un bit à la fois.
> Moteur cassette : géré via CA2 du CRA (bits CRA3-CRA5).

## $A7C1 — PORTB ★ BUZZER + CLAVIER

Source : Manuel Technique MO5 p.30-32 + p.40 + Guide du MO5 p.278 — Fiabilité ÉLEVÉE.

| Bit | Direction | Fonction |
|-----|-----------|----------|
| 0 | Sortie | **Son buzzer 1-bit** — PB0, via RC vers amplificateur |
| 1 | Sortie | Sélection colonne clavier A (via 74LS156) |
| 2 | Sortie | Sélection colonne clavier B (via 74LS156) |
| 3 | Sortie | Sélection colonne clavier C (via 74LS156) |
| 4 | Sortie | Sélection ligne clavier A (via 74LS151) |
| 5 | Sortie | Sélection ligne clavier B (via 74LS151) |
| 6 | Sortie | Sélection ligne clavier C (via 74LS151) |
| 7 | Entrée | **Lecture état ligne clavier** (0 = touche pressée) |

**Méthode recommandée pour couper le bip clavier — via registre moniteur :**
```c
/* Via registre STATUS $2019 bit 3 — ne touche pas au clavier */
/* Source : Clefs Pour MO5 p.110 et p.118 */
*((unsigned char*)0x2019) |= 0x08;   /* mute */
*((unsigned char*)0x2019) &= ~0x08;  /* unmute */
```

**Méthode alternative — directement sur PB0 :**
```c
unsigned char val = *((unsigned char*)0xA7C1);
val &= ~0x01;
*((unsigned char*)0xA7C1) = val;
```

## $A7C2 — CRA

| Bits | Fonction |
|------|----------|
| 0–1 | Contrôle entrée CA1 (crayon optique) et masque IRQ |
| 2 | 0 = accès DDRA / 1 = accès ORA |
| 3–5 | Contrôle sortie CA2 (moteur cassette) |
| 6–7 | Flags IRQ CA2/CA1 (lecture seule) |

## $A7C3 — CRB

Même structure que CRA pour le port B.

- **CB1** → interruptions 50 Hz (signal VBL)
- **CB2** → commande d'incrustation vidéo
- **IRQ B** → interruption principale du 6809

> ⚠️ Différence TO7/MO5 : c'est **IRQB** (pas IRQA) qui génère l'IRQ principale.
> Source MAME : `// WARNING: differs from TO7 !`

**Inhibition complète du clavier (jeu manette exclusif) :**
```c
*((unsigned char*)0xA7C3) = 4;  /* Source : Clefs Pour MO5 p.127 */
```

Source: `mo5_hardware_reference.md` section 6 — Fiabilité ÉLEVÉE (Manuel Technique + Guide + MAME)

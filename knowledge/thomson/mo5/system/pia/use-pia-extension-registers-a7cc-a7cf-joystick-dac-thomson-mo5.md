# Use PIA Extension Registers A7CC-A7CF Joystick DAC (Thomson MO5)

Le PIA extension (Motorola 6821) est mappé à `$A7CC`. Il gère les manettes de jeu et le DAC son 6 bits.

## Disponibilité

- **Optionnel** sur MO5 de base (extension jeux/musique)
- **Intégré de série** sur : MO5E, MO6, MO5NR

## $A7CC — PORTA (directions manettes)

Source : Clefs Pour MO5 p.98 et p.107 — Fiabilité ÉLEVÉE.

| Bit | Direction | Fonction |
|-----|-----------|----------|
| 0 | Entrée | Manette 0 — HAUT (actif bas) |
| 1 | Entrée | Manette 0 — BAS (actif bas) |
| 2 | Entrée | Manette 0 — GAUCHE (actif bas) |
| 3 | Entrée | Manette 0 — DROITE (actif bas) |
| 4 | Entrée | Manette 1 — HAUT (actif bas) |
| 5 | Entrée | Manette 1 — BAS (actif bas) |
| 6 | Entrée | Manette 1 — GAUCHE (actif bas) |
| 7 | Entrée | Manette 1 — DROITE (actif bas) |

## $A7CD — PORTB ★ DAC son 6 bits + boutons fire

Source : Clefs Pour MO5 p.99 et p.107 — Fiabilité ÉLEVÉE.

| Bits | Direction | Fonction |
|------|-----------|----------|
| 0–5 | Sortie (si init DAC) | **DAC numérique/analogique 6 bits** (valeurs 0–63) |
| 6 | Entrée | **Bouton fire manette 0** (actif bas) |
| 7 | Entrée | **Bouton fire manette 1** (actif bas) |

> ⚠️ Les bits 6-7 sont les boutons fire sur PORTB — **pas sur CRA**.
> Source confirmée : Clefs Pour MO5 p.107 :
> "B6 : Poussoir manette 0. B7 : Poussoir manette 1."
> Les bits 6-7 doivent rester en **entrées** même si les bits 0-5 sont en sorties (DAC).
> Initialiser joystick AVANT le DAC pour préserver ce câblage.

## Initialisation obligatoire du DAC

Source : Manuel Technique MO5 p.48 — Fiabilité ÉLEVÉE.

```c
/* Séquence correcte : CRB=0 → DDRB=0x3F → CRB=4 */
*((unsigned char*)0xA7CF) = 0x00;    /* accès DDRB */
*((unsigned char*)0xA7CD) = 0x3F;    /* B0-B5 en sorties, B6-B7 restent entrées */
*((unsigned char*)0xA7CF) = 0x04;    /* accès PORTB */
```

## Initialisation manettes (PORTA en entrées)

```c
*((unsigned char*)0xA7CE) = 0x00;    /* accès DDRA */
*((unsigned char*)0xA7CC) = 0x00;    /* DDRA = 0 : toutes entrées */
*((unsigned char*)0xA7CE) = 0x04;    /* accès ORA */
```

## Lecture directions (PORTA)

```c
/* Actif bas — inverser pour normaliser (1 = pressé) */
unsigned char porta = ~(*((unsigned char*)0xA7CC));
unsigned char joy0_dirs = porta & 0x0F;   /* bits 0-3 : manette 0 */
unsigned char joy1_dirs = porta & 0xF0;   /* bits 4-7 : manette 1 */
```

## Lecture boutons fire (PORTB)

```c
unsigned char portb = *((unsigned char*)0xA7CD));
unsigned char fire0 = (portb & 0x40) ? 0 : 1;  /* bit 6, actif bas */
unsigned char fire1 = (portb & 0x80) ? 0 : 1;  /* bit 7, actif bas */
```

## Écriture d'un échantillon DAC

```c
*((unsigned char*)0xA7CD) = valeur & 0x3F;  /* 0 à 63 */
```

## Résumé adresses PIA extension

| Adresse | Nom | Fonction |
|---------|-----|----------|
| `$A7CC` | PORTA | Manettes — directions (entrées, actif bas) |
| `$A7CD` | PORTB | DAC bits 0-5 (sortie) + fire manettes bits 6-7 (entrée) |
| `$A7CE` | CRA | Contrôle PORTA extension |
| `$A7CF` | CRB | Contrôle PORTB extension |

Source: `mo5_hardware_reference.md` section 6 — Fiabilité ÉLEVÉE (Manuel Technique + Clefs Pour MO5)

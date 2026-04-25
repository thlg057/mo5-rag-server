# Use PIA Extension Registers A7CC-A7CF Joystick DAC (Thomson MO5)

Le PIA extension (Motorola 6821) est mappé à `$A7CC`. Il gère les manettes de jeu et le DAC son 6 bits.

## Disponibilité

- **Optionnel** sur MO5 de base (extension jeux/musique)
- **Intégré de série** sur : MO5E, MO6, MO5NR

## $A7CC — PORTA (manettes)

| Bits | Direction | Fonction |
|------|-----------|----------|
| 0–3 | Entrée | Manette 1 (haut/bas/gauche/droite) |
| 4–7 | Entrée | Manette 2 (haut/bas/gauche/droite) |
| CA1 | Entrée | Bouton manette 1 |
| CA2 | Entrée | Bouton manette 2 |

## $A7CD — PORTB ★ DAC son 6 bits

| Bits | Direction | Fonction |
|------|-----------|----------|
| 0–5 | Sortie | **DAC numérique/analogique 6 bits** |
| 6–7 | Entrée | Non utilisés |

Tension de sortie max : ~450 mV. Valeurs 0–63.

## Initialisation obligatoire du DAC

Source : Manuel Technique MO5 p.48

```asm
CLR  $A7CF        ; CRB = 0 → accès DDRB
LDD  #$3F04
STA  $A7CD        ; DDRB : bits B0-B5 en sorties (0x3F)
STB  $A7CF        ; CRB bit 2 = 1 → accès PORTB
```

En C (cmoc) :

```c
*((unsigned char*)0xA7CF) = 0x00;    /* accès DDRB */
*((unsigned char*)0xA7CD) = 0x3F;    /* B0-B5 en sorties */
*((unsigned char*)0xA7CF) = 0x04;    /* accès PORTB */
```

## Écriture d'un échantillon

```c
*((unsigned char*)0xA7CD) = valeur & 0x3F;  /* 0 à 63 */
```

## Résumé adresses PIA extension

| Adresse | Nom | Fonction |
|---------|-----|----------|
| `$A7CC` | PORTA | Manettes de jeu (entrées) |
| `$A7CD` | PORTB | DAC son 6 bits (bits 0–5 sortie) |
| `$A7CE` | CRA | Contrôle PORTA extension |
| `$A7CF` | CRB | Contrôle PORTB extension |

Source: `mo5_hardware_reference.md` section 6 — Fiabilité ÉLEVÉE (Manuel Technique p.48-51)

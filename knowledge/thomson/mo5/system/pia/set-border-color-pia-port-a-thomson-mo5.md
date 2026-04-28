# Set Border Color with PIA Port A (Thomson MO5)

Changer la couleur du cadre (bordure) de l'écran via les bits PA1–PA4 du PIA système à `$A7C0`.

Source : Manuel Technique MO5 p.18 + Guide du MO5 p.278 — Fiabilité ÉLEVÉE.

## Câblage PORTA $A7C0 bits 1-4

Les bits 1 à 4 encodent la couleur du tour en RVB+demi-teinte, identique au codage couleur VRAM :

| Bit | Signal | Couleur |
|-----|--------|---------|
| PA1 | RT | Rouge |
| PA2 | VT | Vert |
| PA3 | BT | Bleu |
| PA4 | PT | demi-teinte (pastel) |

> ⚠️ L'adresse correcte est `$A7C0`, **pas `$A700`**.

## Exemple C (cmoc)

```c
void set_border_color(unsigned char color)
{
    unsigned char val;
    val  = *((unsigned char*)0xA7C0);
    val &= 0xE1;                          /* garder PA0 (banque VRAM), PA5-PA7 */
    val |= (unsigned char)((color & 0x0F) << 1);
    *((unsigned char*)0xA7C0) = val;
}
```

## Notes importantes

- **PA0** (bit 0) : sélection banque vidéo — NE PAS écraser
- **PA5** (bit 5) : entrée crayon optique — NE PAS écraser
- **PA6/PA7** : cassette — NE PAS écraser
- Utiliser impérativement un read-modify-write avec masque `0xE1`
- La couleur du cadre utilise le même codage 4 bits que la palette VRAM

## Utilisation avec les constantes SDK

```c
/* Cadre rouge */
set_border_color(C_RED);   /* C_RED = 1 */

/* Cadre bleu */
set_border_color(C_BLUE);  /* C_BLUE = 4 */
```

Source: `mo5_hardware_reference.md` section 6 — Fiabilité ÉLEVÉE (Manuel Technique + Guide)

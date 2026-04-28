# Use Gate-Array Video Registers A7E4-A7E7 (Thomson MO5)

Le gate-array MC1300 ALS expose 4 registres en lecture qui permettent de connaître la position exacte du balayage vidéo. Le registre `$A7E7` bit 7 est le plus utilisé pour la synchronisation VBL.

Source : Manuel Technique MO5 p.116 — Fiabilité ÉLEVÉE.

## Table des registres gate-array

| Adresse | D7 | D6 | D5 | D4 | D3 | D2 | D1 | D0 |
|---------|----|----|----|----|----|----|----|----|
| `$A7E4` | T12 | T11 | T10 | T9 | T8 | T7 | T6 | T5 |
| `$A7E5` | T4 | T3 | TL2 | TL1 | TL0 | H1 | H2 | H4 |
| `$A7E6` | LT3 | INILN | — | — | — | — | — | — |
| `$A7E7` | **INITN** | **INILN** | — | — | — | — | — | — |

## Significations

- **T3–T12** : compteur trame (position verticale dans la trame)
- **TL0–TL2** : compteur ligne (position horizontale en octets)
- **H1/H2/H4** : horloges internes 1/2/4 MHz (position au niveau du pixel)
- **INITN** (bit 7 de `$A7E7`) : `1` = balayage actif, `0` = retour vertical (VBL)
- **INILN** : `1` = balayage ligne actif, `0` = retour ligne

## Utilisation courante — synchronisation VBL

```c
#define VBL_REG  ((unsigned char *)0xA7E7)
#define VBL_BIT  0x80   /* INITN = bit 7 */

void mo5_wait_vbl(void)
{
    while ( *VBL_REG &  VBL_BIT) ;  /* attendre fin balayage actif */
    while (!(*VBL_REG & VBL_BIT)) ; /* attendre début prochain balayage */
}
```

## Utilisation avancée — raster timing

Les registres `$A7E4`–`$A7E6` permettent de déclencher un traitement à une ligne précise de l'écran (effet raster, split-screen, etc.).

```c
/* Attendre la ligne 100 */
while ((*((unsigned char*)0xA7E4) & 0xE0) != 0x60) ; /* T7-T5 = 011 */
```

> Pour un jeu, `$A7E7` bit 7 suffit. Les autres registres sont utiles uniquement
> pour des effets graphiques avancés ou de la mesure de timing précis.

Source: `mo5_hardware_reference.md` section 4

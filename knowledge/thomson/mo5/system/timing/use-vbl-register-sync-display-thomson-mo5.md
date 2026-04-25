# Use VBL Register to Synchronize Display (Thomson MO5)

Le MO5 expose le signal de retour vertical (VBL) via le gate-array vidéo au registre `$A7E7` bit 7. C'est le seul mécanisme de synchronisation disponible — il n'y a pas de timer programmable.

## Registre VBL

| Registre | Bit | Valeur | Signification |
|----------|-----|--------|---------------|
| `$A7E7` | 7 | `1` | Balayage actif (faisceau sur l'écran) |
| `$A7E7` | 7 | `0` | Retour vertical — fenêtre VBL |

> ⚠️ `$A7E7` appartient au **gate-array vidéo**, pas au PIA système (`$A7C0`).
> Ne pas confondre ces deux zones d'I/O.

## Timing PAL 50 Hz

```
|  balayage actif (~18.7ms)  | VBL (~1.2ms) |  balayage actif ...
0ms                        18.7ms          20ms
```

- Budget total par frame : ~20 000 cycles à 1 MHz
- Fenêtre VBL : ~1 200 cycles — période idéale pour écrire en VRAM

## Implémentation

```c
#define VBL_REG  ((unsigned char *)0xA7E7)
#define VBL_BIT  0x80

void mo5_wait_vbl(void)
{
    while ( *VBL_REG &  VBL_BIT) ;  /* attendre fin balayage actif */
    while (!(*VBL_REG & VBL_BIT)) ; /* attendre début prochain balayage */
}
```

## Notes cmoc

> ⚠️ cmoc n'a pas `volatile`. Ne pas déclarer `mo5_wait_vbl` en `static inline` —
> l'inlining peut provoquer la mise en cache de la valeur lue, rendant la boucle infinie.
> L'attente active sur `$A7E7` est la seule méthode fiable sur MO5.

Source: `mo5_hardware_reference.md` section 4

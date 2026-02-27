# Implémenter `mo5_wait_vbl()` sur Thomson MO5

## Goal

Écrire une fonction C `mo5_wait_vbl()` qui se synchronise sur la VBL du MO5 en lisant le registre PIA.

## Registre matériel

Le MO5 utilise une PIA 6821. Le **bit 7** du registre mémoire-mappé `$A7E7` reflète le signal `INITRAME` du chip vidéo MC6847 :

| Bit 7 | Signification                         |
|:-----:|----------------------------------------|
| 1     | Balayage actif (écran en cours d'affichage) |
| 0     | Retour vertical / VBL                 |

## Implémentation en C

```c
unsigned char peek(unsigned int addr) {
    return *((unsigned char *)addr);
}

void mo5_wait_vbl(void) {
    // Attendre la fin de la VBL courante (INITRAME = 1)
    while ((peek(0xA7E7) & 0x80) == 0)
        ;

    // Attendre le début de la prochaine VBL (INITRAME = 0)
    while ((peek(0xA7E7) & 0x80) != 0)
        ;
}
```

## Notes spécifiques à CMOC

- `peek()` **ne doit pas** être `static` :
  - Sinon le compilateur peut l'inliner et optimiser les lectures mémoire successives.
  - On veut forcer une **lecture réelle** du registre à chaque itération.
- Le mot-clé `volatile` n'est pas supporté par CMOC, d'où ce contournement.
- Il n'existe pas de timer programmable sur MO5 utilisable simplement pour ce rôle :
  - L'attente occupée sur `$A7E7` est la méthode standard et fiable.

Source: `mo5-docs/mo5/vbl_mo5.md`


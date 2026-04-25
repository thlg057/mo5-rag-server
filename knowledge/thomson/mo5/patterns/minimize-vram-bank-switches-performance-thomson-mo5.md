# Minimize VRAM Bank Switches for Performance (Thomson MO5)

Chaque switch de banque VRAM coûte ~5 cycles et est une opération I/O. Les grouper par passe réduit significativement le coût CPU.

## Anti-pattern — switch par pixel

```c
/* ❌ 2 switches par pixel — très coûteux */
for (i = 0; i < height; i++) {
    for (j = 0; j < width; j++) {
        *PRC &= ~0x01;          /* switch couleur */
        VRAM[offset + j] = color;
        *PRC |=  0x01;          /* switch forme */
        VRAM[offset + j] = form;
    }
}
```

## Pattern optimal — une passe par banque

```c
/* ✅ 2 switches au total pour tout le sprite */
*PRC &= ~0x01;                  /* une seule fois */
for (i = 0; i < height; i++) {
    for (j = 0; j < width; j++)
        VRAM[offset + j] = *color_src++;
    offset += SCREEN_WIDTH_BYTES;
}

*PRC |= 0x01;                   /* une seule fois */
offset = base_offset;
for (i = 0; i < height; i++) {
    for (j = 0; j < width; j++)
        VRAM[offset + j] = *form_src++;
    offset += SCREEN_WIDTH_BYTES;
}
```

## Règle générale

```text
❌ FORM → COLOR → FORM → COLOR  (un pixel à la fois)
✔  FORM (sprite entier) → COLOR (sprite entier)
```

## Accès groupés pendant le VBL

- Regrouper les écritures mémoire en rafales courtes
- Éviter les accès dispersés pendant l'affichage actif
- Préférer les gros transferts pendant la fenêtre VBL (~1 200 cycles)

## Double buffering

> ⚠️ RAM limitée (~24–28 Ko user) → pas de vrai double buffer complet possible.
> Solution : buffer partiel + dirty rectangles (ne mettre à jour que les zones modifiées).

Source: `mo5_hardware_reference.md` section 8

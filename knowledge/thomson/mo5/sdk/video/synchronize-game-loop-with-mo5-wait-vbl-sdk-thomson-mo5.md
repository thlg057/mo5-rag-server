# Synchroniser la boucle de jeu avec `mo5_wait_vbl` du SDK MO5 (Thomson MO5)

Ce chunk décrit la fonction `mo5_wait_vbl` définie dans `mo5_video.h` et son utilisation pour cadencer la boucle de jeu.

## Goal

- Comprendre le rôle de `mo5_wait_vbl`.
- Placer correctement l'appel dans la boucle principale.

## Prototype

```c
void mo5_wait_vbl(void);
```

- Attend le prochain retour de trame vertical (VBL) sur un MO5 PAL à 50 Hz (~20 ms par frame).
- Garantit une cadence stable (~50 FPS) si la logique de jeu reste dans le budget de cycles.

## Pattern de boucle de jeu

```c
while (1) {
    mo5_wait_vbl();    // synchronisation trame
    read_inputs();
    update_logic();
    draw();
}
```

- Appeler `mo5_wait_vbl` en **début** de boucle.
- Effectuer la lecture des entrées, la logique et le dessin **après** l'attente.

## Notes

- `mo5_wait_vbl` utilise le registre `VIDEO_REG` pour détecter le VBL ; tu n'as pas à gérer les bits manuellement.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_video_h.md`

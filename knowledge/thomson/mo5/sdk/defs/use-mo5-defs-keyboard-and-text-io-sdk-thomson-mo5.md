# Utiliser les I/O clavier et texte de bas niveau de `mo5_defs.h` (Thomson MO5)

Ce chunk décrit les primitives d'entrée/sortie texte de bas niveau fournies par `mo5_defs.h` : `mo5_getchar`, `mo5_putchar`, `mo5_newline`, `mo5_wait_key` et `wait_for_key`.

## Goal

- Lire le clavier en mode non bloquant ou bloquant.
- Afficher des caractères et des fins de ligne.
- Attendre une touche précise ou n'importe quelle touche.

## Lecture clavier

```c
char mo5_getchar(void);
```

- Lit le clavier via `SWI $0A`.
- **Non bloquant** : retourne `0` si aucune touche n'est pressée.
- Pour une lecture bloquante, boucle sur `mo5_getchar()` jusqu'à ce que le retour soit non nul.

```c
char ch;
do {
    ch = mo5_getchar();
} while (ch == 0);
```

```c
char wait_for_key(void);
```

- Encapsule ce pattern : attend n'importe quelle touche et la retourne.

## Affichage texte

```c
void mo5_putchar(char c);
void mo5_newline(void);
```

- `mo5_putchar` envoie un caractère à l'écran via `SWI $02`.
- `mo5_newline` envoie `MO5_ENTER_CHAR` puis `MO5_LINE_FEED` pour faire un retour à la ligne complet (CR+LF).

## Attendre une touche donnée

```c
void mo5_wait_key(char key);
```

- Boucle sur la lecture de caractères (via `getchar`/`mo5_getchar`) jusqu'à ce que `key` soit pressée.
- Utile pour les validations ("Appuie sur ESPACE pour continuer", menus, etc.).

## Notes

- Toutes ces fonctions reposent sur les SWI système du MO5 et constituent la base de `mo5_stdio.h`.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_defs_h.md`

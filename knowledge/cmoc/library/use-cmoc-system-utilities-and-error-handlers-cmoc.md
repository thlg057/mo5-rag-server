# Utiliser les utilitaires système et handlers d’erreurs CMOC

Ce chunk présente quelques fonctions de `<cmoc.h>` qui influencent le comportement global du programme.

## Goal

- Comprendre le rôle des utilitaires système (`exit`, hooks d’affichage).
- Installer des handlers pour gérer pointeur nul et overflow de pile.

## Quitter proprement avec `exit`

```c
void exit(int status);
```

- Termine le programme et retourne à l’environnement appelant (par ex. BASIC Thomson si le binaire a été lancé depuis là).
- La valeur de `status` peut être ignorée par l’environnement.

Sur MO5, évite d’appeler `exit` si tu as modifié profondément l’environnement BASIC (mapping mémoire exotique, ROM paginée hors mémoire, etc.).

## Rediriger la sortie console globale

```c
ConsoleOutHook setConsoleOutHook(ConsoleOutHook routine);
```

- Change la routine utilisée par `printf`, `putchar`, `putstr`.
- Retourne l’ancienne routine (que tu peux restaurer plus tard).

Voir le chunk dédié à `printf` et au hook console pour un exemple complet.

## Handlers d’erreurs runtime

```c
void set_null_ptr_handler(void (*newHandler)(void *));
void set_stack_overflow_handler(void (*newHandler)(void *, void *));
```

- `set_null_ptr_handler` : appelé lorsqu’un pointeur nul est déréférencé.
- `set_stack_overflow_handler` : appelé en cas de dépassement de pile détecté par le runtime CMOC.

Pattern typique :

```c
void myNullHandler(void *addr) {
    // Afficher un message d’erreur, éventuellement freeze
}

void myStackHandler(void *sp, void *limit) {
    // Afficher info de debug, puis freeze
}

int main(void) {
    set_null_ptr_handler(myNullHandler);
    set_stack_overflow_handler(myStackHandler);
    // ...
}
```

## Notes

- Ces handlers sont particulièrement utiles en phase de debug sur MO5, où la visibilité des erreurs est limitée.

Source: `mo5-docs/cmoc/cmoc_h.md`

# Utiliser `printf` CMOC et `setConsoleOutHook` sur MO5

Ce chunk décrit l’I/O texte de `<cmoc.h>` et la redirection de sortie via `setConsoleOutHook` pour afficher sur l’écran MO5.

## Goal

- Comprendre comment `printf` et Cie écrivent leurs caractères.
- Rediriger cette sortie vers ta propre routine d’affichage MO5.

## Fonctions d’I/O texte de `<cmoc.h>`

- `int printf(const char *format, ...)`
- `int sprintf(char *dest, const char *format, ...)`
- `int vprintf(const char *format, va_list ap)`
- `int vsprintf(char *dest, const char *format, va_list ap)`
- `void putstr(const char *s, size_t n)`
- `void putchar(int c)`

`printf`/`putchar` envoient les caractères **un par un** à une routine pointée par un hook global interne.

## Rediriger la sortie avec `setConsoleOutHook`

Prototype simplifié :

```c
typedef void (*ConsoleOutHook)(void);
ConsoleOutHook setConsoleOutHook(ConsoleOutHook routine);
```

- La routine reçoit le caractère à afficher dans le registre **A**.
- Elle doit préserver au moins les registres **B, X, Y, U**.
- Elle ne doit pas modifier l’état global du système de façon inattendue.

Exemple de squelette pour MO5 :

```c
void mo5ConsoleOut(void) {
    char ch;
    asm {
        pshs b,x         // préserver B, X (et U automatiquement)
        sta  :ch
    }
    // TODO : envoyer ch à ta routine d'affichage MO5
    asm {
        puls x,b
    }
}

int main(void) {
    setConsoleOutHook(mo5ConsoleOut);
    printf("Hello MO5!\n");
    return 0;
}
```

## Bonnes pratiques sur MO5

- Utilise `sprintf`/`vsprintf` pour formater dans un buffer, puis affiche avec ton propre système **si tu veux rester indépendant du hook global**.
- Si tu rediriges via `setConsoleOutHook`, fais‑le **une fois au début** du programme, avant tout appel à `printf`.

Source: `mo5-docs/cmoc/cmoc_h.md`, `mo5-docs/cmoc/cmoc-manual.md`

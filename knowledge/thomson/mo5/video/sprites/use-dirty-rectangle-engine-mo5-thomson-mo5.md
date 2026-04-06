
# Use dirty rectangle engine (Thomson MO5)

Advanced rendering with background restore.

## Goal

Support overlapping sprites.

## Loop

```c
mo5_actor_dr_restore(&hero);
mo5_actor_dr_move(&hero, x, y);
mo5_actor_dr_save_draw(&hero);
```

## Pros

- perfect visuals

## Cons

- slow
- uses RAM

## When to use

- complex scenes

Source: guide-graphical-development-mo5.md

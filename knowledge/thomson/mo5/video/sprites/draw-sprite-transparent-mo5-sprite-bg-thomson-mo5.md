
# Draw transparent sprite (mo5_sprite_bg)

Default rendering mode.

## Goal

Render sprites over background.

## Example

```c
mo5_actor_move_bg(&hero, x, y);
```

## Behavior

- preserves background
- writes only foreground

## Requirements

- sprite must use --transparent

## Common mistakes

- ❌ mixing engines
- ❌ wrong sprite format

Source: guide-graphical-development-mo5.md

# Detect AABB collision between two rectangles (Thomson MO5)

Detect collisions between sprites/objects using axis-aligned bounding boxes (AABB).

## Steps

1. Represent each box as `(x, y, w, h)`.
2. Two boxes overlap if they overlap on both axes.

## C example

```c
unsigned char aabb_hit(int ax,int ay,int aw,int ah, int bx,int by,int bw,int bh)
{
    if (ax + aw <= bx) return 0;
    if (bx + bw <= ax) return 0;
    if (ay + ah <= by) return 0;
    if (by + bh <= ay) return 0;
    return 1;
}
```

Source: `knowledge/docs/mo5_game_dev_guide.md`

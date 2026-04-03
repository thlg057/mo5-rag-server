# Common performance issues (Thomson MO5)

## Symptoms

- game slows down
- flickering
- input lag

## Causes

- too many collision tests
- full redraw every frame
- no VBL sync

## Fixes

- use object pool
- use dirty rectangle
- throttle updates

# Understand VBL timing 50Hz (Thomson MO5)

The screen refreshes at 50Hz with a short vertical blank period.

## Key facts
- 20ms per frame
- ~1.2ms VBL
- ~20000 CPU cycles per frame

## Why it matters
Use VBL to synchronize rendering and avoid tearing.

Source: vbl_mo5.md

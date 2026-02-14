# Use the default code address $2800 for Thomson MO/TO in CMOC

## Goal
Rely on CMOC’s default code address when targeting Thomson MO/TO.

## Procedure
1. When building for Thomson targets, omit `--org` unless you explicitly need a different address.

## Notes
- The CMOC manual lists the default code address for “Thomson MO/TO” as `$2800`.
- If you need to override the default, use `--org=XXXX` at link time.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Default code addresses”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

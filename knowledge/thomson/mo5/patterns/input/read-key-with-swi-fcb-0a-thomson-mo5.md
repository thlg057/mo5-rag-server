# Read a key with SWI + FCB $0A (Thomson MO5)

Read one keypress using the MO5 system call convention: `SWI` followed by a function code byte (FCB).

## Steps

1. Emit `swi`.
2. Emit the function code byte for the service you need.
3. Read the returned character (commonly in register A).

## CMOC inline assembly example

```c
unsigned char read_key(void)
{
    asm {
        swi
        fcb $0A
        // convention: result returned in A
    }
}
```

## Notes

- Keep this pattern in a single function so you can swap it out if your ROM/distribution uses a different service number.

Source: `knowledge/docs/mo5_game_dev_guide.md`

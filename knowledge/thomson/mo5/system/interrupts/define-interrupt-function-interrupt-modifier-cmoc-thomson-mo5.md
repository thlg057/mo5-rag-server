# Define interrupt handler with the interrupt modifier (CMOC, Thomson MO5)

Define an interrupt service routine (ISR) in C that ends with `RTI` instead of `RTS` using CMOC's `interrupt` function modifier.

## Steps

1. Declare your ISR with the `interrupt` modifier.
2. Keep the ISR self-contained (minimize dependencies).
3. Redirect the relevant interrupt vector to your ISR while interrupts are masked.

## C example

```c
interrupt void my_isr(void)
{
    // optional inline asm + C code
    // ensure last instruction is RTI (CMOC will emit RTI)
}
```

## Notes

- CMOC emits an `RTI` epilogue for `interrupt` functions.
- Using interrupt handlers typically requires vector redirection on the target machine.

Source: `knowledge/docs/cmoc_thomson_mo5.md`, `knowledge/manuals/cmoc-manual.md`

# Tokenize a string with strtok() (CMOC, Thomson MO5)

Split a string into tokens separated by delimiters.

## Steps

1. Include `<cmoc.h>`.
2. Call `strtok(str, delim)` to get the first token.
3. Call `strtok(NULL, delim)` repeatedly to get the next tokens.

## Notes

- `strtok()` modifies the input string.

Source: `knowledge/docs/cmoc_h_mo5.md`

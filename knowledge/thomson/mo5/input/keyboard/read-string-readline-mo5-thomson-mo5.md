# Read string using readline (Thomson MO5)

## Concept

CMOC provides readline().

## Example

```c
char *input = readline();
```

## Pitfalls

- may return NULL
- must copy safely

## Safe copy

```c
strncpy(buffer, input, size-1);
buffer[size-1] = '\0';
```

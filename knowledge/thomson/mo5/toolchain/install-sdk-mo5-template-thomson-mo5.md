
# Install sdk_mo5 using project template (Thomson MO5)

Install the SDK using the mo5_template project.

## Goal

Quickly setup a working development environment with minimal configuration.

## Prerequisites

- mo5_template project
- make installed

## Steps

```bash
make install
```

## Result

The SDK is installed in tools/:

- include/ → headers
- lib/ → static library
- scripts/ → utilities
- docs/ → documentation

## Notes

- The Makefile is preconfigured
- Always declare headers in PROJ_HDR

## Common mistakes

- ❌ Forgetting headers → no recompilation

Source: guide-graphical-development-mo5.md

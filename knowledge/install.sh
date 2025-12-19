#!/usr/bin/env bash
set -euo pipefail

REPO_URL="https://github.com/OlivierP-To8/BootFloppyDisk.git"
TARGET_DIR="tools/BootFloppyDisk"

echo "==> Clonage complet du dépôt BootFloppyDisk (incluant outils)"
rm -rf "${TARGET_DIR}" # on repart propre
mkdir -p tools
git clone "${REPO_URL}" "${TARGET_DIR}"

echo "==> Construction éventuelle des outils (c6809 / fdfs) si règles disponibles"
if grep -q 'tools/c6809-v1.0' "${TARGET_DIR}/Makefile"; then
	(cd "${TARGET_DIR}" && make tools/c6809 || true)
fi
if grep -q 'fdfs' "${TARGET_DIR}/Makefile"; then
	(cd "${TARGET_DIR}" && make fdfs || true)
fi

echo "==> Terminé. Lance: make help"
#!/usr/bin/env bash
# Package an existing linux-x64 publication with executable permissions preserved.
# Usage: bash scripts/package-linux.sh PUBLISH_DIRECTORY OUTPUT.tar.gz
set -euo pipefail

if [[ $# -ne 2 ]]; then
    echo "Usage: $0 PUBLISH_DIRECTORY OUTPUT.tar.gz" >&2
    exit 1
fi

publish_directory=$(realpath -- "$1")
archive=$(realpath -m -- "$2")
if [[ ! -f "$publish_directory/dxt" || ! -f "$publish_directory/Asar/asar" ]]; then
    echo "The publication must contain dxt and Asar/asar." >&2
    exit 1
fi
if [[ "$archive" != *.tar.gz || -e "$archive" ]]; then
    echo "Choose a new output filename ending in .tar.gz." >&2
    exit 1
fi

staging=$(mktemp -d /tmp/dxt-linux-package.XXXXXXXX)
# Only remove the uniquely allocated staging directory owned by this invocation.
trap 'case "$staging" in /tmp/dxt-linux-package.*) rm -r -- "$staging" ;; esac' EXIT
mkdir "$staging/DynamicXtreme" "$staging/check"
cp -a -- "$publish_directory/." "$staging/DynamicXtreme/"
chmod 755 "$staging/DynamicXtreme/dxt" "$staging/DynamicXtreme/Asar/asar"
tar -czf "$staging/package.tar.gz" -C "$staging" DynamicXtreme

# Check extracted permissions on a Linux filesystem before publishing the archive.
tar -xzf "$staging/package.tar.gz" -C "$staging/check"
test -x "$staging/check/DynamicXtreme/dxt"
test -x "$staging/check/DynamicXtreme/Asar/asar"
mkdir -p -- "$(dirname -- "$archive")"
cp --no-clobber -- "$staging/package.tar.gz" "$archive"
echo "Created $archive (dxt and Asar/asar have executable permissions)."

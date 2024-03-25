#!/bin/sh
echo -ne '\033c\033]0;HidyHoe\a'
base_path="$(dirname "$(realpath "$0")")"
"$base_path/HidyHoe.x86_64" "$@"

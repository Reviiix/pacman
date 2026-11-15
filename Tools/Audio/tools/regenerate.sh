#!/bin/sh
# Rebuilds every sound effect from sounds.json into out/ and writes the level report to out/report.txt.
# Usage (from anywhere): sh tools/regenerate.sh        Needs only Node (no packages).
set -e
cd "$(dirname "$0")/.."
node tools/synth.js sounds.json out
node tools/check.js out | tee out/report.txt

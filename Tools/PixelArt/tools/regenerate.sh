#!/bin/sh
# Rebuilds every art output from art.txt: spec.json, the sprite strips and walls.png in out/, the contact sheet,
# both level mockups, the real-size 2x canvas check, the wall stress test and the readability metrics.
# Usage (from anywhere): sh tools/regenerate.sh        Needs only Node (no packages).
set -e
cd "$(dirname "$0")/.."
node tools/build.js art.txt spec.json
node tools/render.js sprites spec.json out 4 > /dev/null
node tools/hud.js spec.json out/hud.png > /dev/null
node tools/sheet.js spec.json out contact_sheet.png 6
node tools/make_mockup.js . > mockup.json
node tools/make_mockup.js . alt > mockup_alt.json
node tools/render.js mockup mockup.json mockup.png 3
node tools/render.js mockup mockup_alt.json mockup_alt_frames.png 3
mkdir -p review
node tools/render.js mockup mockup.json review/mockup_x2.png 2 > /dev/null
node tools/imgutil.js pad review/mockup_x2.png review/canvas_576x784_x2.png 16 '#1e1442'
node tools/render.js mockup review/stress.json review/stress_walls_x2.png 2
node tools/check.js spec.json out > review/metrics.txt
echo "done: see contact_sheet.png, mockup.png, mockup_alt_frames.png and review/"

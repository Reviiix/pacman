#!/usr/bin/env node
// Writes a mockup config for render.js: the HUD strip above the maze, every pellet (a sparse, staggered few on the twinkle
// frame, as the twinkle driver would show them), the bonus pickup, and one of each character on its spawn-area tile.
// Usage: node make_mockup.js <styleDir> [alt] > <styleDir>/mockup.json   (alt: frame 1 of every animated sprite, another twinkle set)
// Paths in the config are relative to <styleDir>, where the config is written (render.js resolves them from the config's folder).
const fs = require('fs');
const path = require('path');
const styleDir = path.resolve(process.argv[2] || '.');
const alt = process.argv[3] === 'alt' ? 1 : 0;
const mapPath = path.join(styleDir, 'maps/level-map.txt');
const rows = fs.readFileSync(mapPath, 'utf8').split('\n').filter(l => /^\s*\d+ /.test(l)).map(l => l.replace(/^\s*\d+ /, ''));
const pellets = rows.flatMap((r, y) => [...r].map((c, x) => (c === '.' ? [x, y] : null)).filter(Boolean));
// About 1 pellet in 12 twinkles at any moment; a different scatter for the alt frame.
const twinkle = ([x, y]) => ((x * 7 + y * 11 + alt * 5) % 12) === 0;
const spec = JSON.parse(fs.readFileSync(path.join(styleDir, 'spec.json'), 'utf8'));
const has = n => !!spec.sprites[n];
const cfg = {
  map: 'maps/level-map.txt', tile: 16, background: spec.palette.N, corridor: null, wallSpec: 'spec.json',
  outOfBoundsIsWall: true, hudTop: 24,
  layers: [
    { png: 'out/hud.png', frame: 0, frameWidth: 272, cells: [[0, 0]], offset: [128, -20] },
    { png: 'out/pellet.png', frame: 0, cells: pellets.filter(p => !twinkle(p)) },
    { png: 'out/pellet.png', frame: 1, cells: pellets.filter(twinkle) },
    { png: `out/${has('pickup_potion') && alt ? 'pickup_potion' : 'pickup_cherry'}.png`, frame: 0, cells: [[8, 8]] },
    { png: 'out/hero_side.png', frame: alt, cells: [[8, 16]] },
    { png: 'out/enemy_chase.png', frame: alt, cells: [[6, 12]] },
    { png: 'out/enemy_cutoff.png', frame: alt, cells: [[7, 12]] },
    { png: 'out/enemy_scared.png', frame: alt, cells: [[9, 12]] },
    { png: 'out/enemy_random.png', frame: alt, cells: [[4, 6]] },
  ].filter(l => l.png === 'out/hud.png' || fs.existsSync(path.join(styleDir, l.png))),
  overlay: { scanlines: 0.15, vignette: 0.25, vignetteKeepTop: 24 },
};
const json = JSON.stringify(cfg, null, 1).replace(/\[\s+(-?\d+),\s+(-?\d+)\s+\]/g, '[$1,$2]');
process.stdout.write(json + '\n');

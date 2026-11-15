#!/usr/bin/env node
// Draws the HUD strip used in mockups (272x24, the maze width): a centred "SCORE" label and six white digits, each with a
// one-pixel drop shadow, and a pellet twinkle on either side. The 5x7 glyphs only stand in for the project's pixel TTF;
// the colours, shadow offsets, spacing and layout are what the style guide specifies.
// Usage: node hud.js <spec.json> <out.png> [score=004260]
const fs = require('fs');
const { encodePng } = require('./render.js');
const [specFile, outFile, score = '004260'] = process.argv.slice(2);
const pal = JSON.parse(fs.readFileSync(specFile, 'utf8')).palette;
const rgb = h => [1, 3, 5].map(i => parseInt(h.slice(i, i + 2), 16));
const G = {
  S: ['.XXXX', 'X....', 'X....', '.XXX.', '....X', '....X', 'XXXX.'],
  C: ['.XXXX', 'X....', 'X....', 'X....', 'X....', 'X....', '.XXXX'],
  O: ['.XXX.', 'X...X', 'X...X', 'X...X', 'X...X', 'X...X', '.XXX.'],
  R: ['XXXX.', 'X...X', 'X...X', 'XXXX.', 'X.X..', 'X..X.', 'X...X'],
  E: ['XXXXX', 'X....', 'X....', 'XXXX.', 'X....', 'X....', 'XXXXX'],
  0: ['.XXX.', 'X...X', 'X..XX', 'X.X.X', 'XX..X', 'X...X', '.XXX.'],
  1: ['..X..', '.XX..', '..X..', '..X..', '..X..', '..X..', '.XXX.'],
  2: ['.XXX.', 'X...X', '....X', '...X.', '..X..', '.X...', 'XXXXX'],
  3: ['XXXX.', '....X', '....X', '.XXX.', '....X', '....X', 'XXXX.'],
  4: ['...X.', '..XX.', '.X.X.', 'X..X.', 'XXXXX', '...X.', '...X.'],
  5: ['XXXXX', 'X....', 'XXXX.', '....X', '....X', 'X...X', '.XXX.'],
  6: ['.XXX.', 'X....', 'X....', 'XXXX.', 'X...X', 'X...X', '.XXX.'],
  7: ['XXXXX', '....X', '...X.', '..X..', '.X...', '.X...', '.X...'],
  8: ['.XXX.', 'X...X', 'X...X', '.XXX.', 'X...X', 'X...X', '.XXX.'],
  9: ['.XXX.', 'X...X', 'X...X', '.XXXX', '....X', '....X', '.XXX.'],
};
const W = 272, H = 24;
const img = { width: W, height: H, data: new Uint8Array(W * H * 4) };
const px = (x, y, c) => { if (x >= 0 && y >= 0 && x < W && y < H) img.data.set([...rgb(c), 255], (y * W + x) * 4); };
function text(str, x, y, face, shadow) {
  for (const pass of [shadow, face]) {
    const dy = pass === shadow ? 1 : 0;
    [...str].forEach((ch, i) => G[ch].forEach((row, gy) => [...row].forEach((v, gx) => { if (v === 'X') px(x + i * 6 + gx, y + gy + dy, pass); })));
  }
}
const label = 'SCORE', gap = 8;
const total = label.length * 6 - 1 + gap + score.length * 6 - 1;
const x0 = Math.floor((W - total) / 2), y0 = 8;
text(label, x0, y0, pal.L, pal.D);
text(score, x0 + label.length * 6 - 1 + gap, y0, pal.W, pal.S);
// Pellet twinkles (the pellet's frame 0, a 4x4 sugar sparkle) either side of the readout, vertically centred on the glyphs.
for (const cx of [x0 - 12, x0 + total + 8]) {
  const rows = ['.YY.', 'YWWY', 'YWWY', '.YY.'];
  rows.forEach((r, yy) => [...r].forEach((k, xx) => { if (k !== '.') px(cx + xx, y0 + 2 + yy, pal[k]); }));
}
fs.writeFileSync(outFile, encodePng(img));
console.log(`hud: ${outFile}`);

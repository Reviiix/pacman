#!/usr/bin/env node
// Contact sheet on the in-game background: every sprite frame at <bigScale> with a 2x copy beside it (the real in-game size),
// then all 47 autotile wall pieces in sheet order with their index, then the palette swatches.
// Usage: node sheet.js <spec.json> <outDir> <sheet.png> [bigScale=6]     (ONLY=name,name limits the sprites)
const fs = require('fs');
const path = require('path');
const { encodePng, decodePng } = require('./render.js');
const [specFile, outDir, outFile, big = '6'] = process.argv.slice(2);
const S = parseInt(big, 10);
const spec = JSON.parse(fs.readFileSync(specFile, 'utf8'));
const rgb = h => [1, 3, 5].map(i => parseInt(h.slice(i, i + 2), 16));
const bg = rgb(spec.palette.N);
const only = process.env.ONLY ? process.env.ONLY.split(',') : null;
const strips = Object.keys(spec.sprites).filter(n => !only || only.includes(n))
  .map(n => ({ n, img: decodePng(fs.readFileSync(path.join(outDir, n + '.png'))) }));
const walls = !only && fs.existsSync(path.join(outDir, 'walls.png')) ? decodePng(fs.readFileSync(path.join(outDir, 'walls.png'))) : null;
const maxFrames = Math.max(...strips.map(s => s.img.width / 16));
const cellBig = 16 * S + 8, cellSmall = 16 * 2 + 6;
const colW = maxFrames * cellBig + 12 + maxFrames * cellSmall + 16;
const columns = strips.length > 6 ? 2 : 1;
const perCol = Math.ceil(strips.length / columns);
const wallCols = 12, wallScale = 3, wallCell = 16 * wallScale + 8;
const wallsH = walls ? Math.ceil(walls.width / 16 / wallCols) * wallCell + 12 : 0;
const swatchH = only ? 0 : 40;
const W = Math.max(8 + columns * colW, walls ? 8 + wallCols * wallCell + 4 : 0);
const H = 8 + perCol * cellBig + wallsH + swatchH;
const out = { width: W, height: H, data: new Uint8Array(W * H * 4) };
for (let i = 0; i < W * H; i++) out.data.set([...bg, 255], i * 4);
function put(img, fx, scale, dx, dy) {
  for (let y = 0; y < 16 * scale; y++) for (let x = 0; x < 16 * scale; x++) {
    const s = (Math.floor(y / scale) * img.width + fx * 16 + Math.floor(x / scale)) * 4;
    const a = img.data[s + 3] / 255; if (!a) continue;
    const d = ((dy + y) * W + dx + x) * 4;
    for (let k = 0; k < 3; k++) out.data[d + k] = Math.round(img.data[s + k] * a + out.data[d + k] * (1 - a));
  }
}
// 3x5 digits for wall tile indices.
const DIG = ['111101101101111', '010110010010111', '111001111100111', '111001111001111', '101101111001001', '111100111001111', '111100111101111', '111001001001001', '111101111101111', '111101111001111'];
function number(n, x, y, c) {
  String(n).split('').forEach((d, i) => DIG[+d].split('').forEach((v, k) => {
    if (v === '1') out.data.set([...c, 255], ((y + Math.floor(k / 3)) * W + x + i * 4 + (k % 3)) * 4);
  }));
}
strips.forEach((s, i) => {
  const ox = 8 + Math.floor(i / perCol) * colW, oy = 8 + (i % perCol) * cellBig;
  const frames = s.img.width / 16;
  for (let f = 0; f < frames; f++) {
    put(s.img, f, S, ox + f * cellBig, oy);
    put(s.img, f, 2, ox + maxFrames * cellBig + 12 + f * cellSmall, oy + (cellBig - 32) / 2);
  }
});
let y = 8 + perCol * cellBig;
if (walls) {
  for (let t = 0; t < walls.width / 16; t++) {
    const x0 = 8 + (t % wallCols) * wallCell, y0 = y + 4 + Math.floor(t / wallCols) * wallCell;
    put(walls, t, wallScale, x0, y0);
    number(t, x0, y0 + 16 * wallScale + 1, rgb(spec.palette.Y));
  }
  y += wallsH;
}
if (!only) Object.values(spec.palette).forEach((hex, i) => {
  for (let yy = -1; yy < 25; yy++) for (let xx = -1; xx < 31; xx++) {
    const border = yy < 0 || xx < 0 || yy > 23 || xx > 29;
    out.data.set([...rgb(border ? spec.palette.K : hex), 255], ((y + 6 + yy) * W + 8 + i * 34 + xx) * 4);
  }
});
fs.writeFileSync(outFile, encodePng(out));
console.log(`sheet: ${outFile} (${W}x${H})`);

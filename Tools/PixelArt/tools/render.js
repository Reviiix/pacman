#!/usr/bin/env node
// Pixel-art toolkit: validates ASCII sprite specs, writes PNG strips, contact sheets, wall tile sheets and a level mockup.
//
// Spec JSON format (one file per sprite group):
// {
//   "palette": { "K": "#101018", "Y": "#ffd75e", ... },   // single-character keys; "." and " " are transparent
//   "sprites": { "hero_side": { "frames": [ ["....16 chars....", ... 16 rows], [...] ] } },
//   "walls": {                                           // optional
//     "mode": "quadrants",                                // NE-oriented 8x8 patterns, mirrored to the other quadrants
//     "quadrants": { "outer": [8 rows], "vertical": [...], "horizontal": [...], "inner": [...], "fill": [...] }
//     // or "mode": "corners", "corners": { "ne": {5 patterns}, "nw": {...}, "se": {...}, "sw": {...} }
//     //    (one NE-authored set per tile corner, flipped into place like "quadrants", so lighting can differ per side;
//     //     same 47-tile Blob47 sheet and order as "quadrants")
//     // or "mode": "cardinal", "base": [16 rows], "armN": [16 rows]   (arm pointing north, rotated for E/S/W)
//   }
// }
// This copy (Candy Haunt style kit) adds the "corners" wall mode and the mockup overlay "vignette" (strength 0..1, with
// "vignetteKeepTop" art rows left untouched for the HUD) to the original toolkit; everything else is unchanged.
//
// Commands:
//   node render.js sprites <spec.json> <outDir> [previewScale=8]
//   node render.js mockup  <mockup.json> <out.png> [scale=3]
const fs = require('fs');
const path = require('path');
const zlib = require('zlib');

// ---------- PNG encoding ----------
const CRC_TABLE = (() => {
  const t = new Uint32Array(256);
  for (let n = 0; n < 256; n++) {
    let c = n;
    for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
    t[n] = c >>> 0;
  }
  return t;
})();
function crc32(buf) {
  let c = 0xffffffff;
  for (let i = 0; i < buf.length; i++) c = CRC_TABLE[(c ^ buf[i]) & 0xff] ^ (c >>> 8);
  return (c ^ 0xffffffff) >>> 0;
}
function chunk(type, data) {
  const len = Buffer.alloc(4); len.writeUInt32BE(data.length);
  const td = Buffer.concat([Buffer.from(type, 'ascii'), data]);
  const crc = Buffer.alloc(4); crc.writeUInt32BE(crc32(td));
  return Buffer.concat([len, td, crc]);
}
function encodePng(img) {
  const { width, height, data } = img; // data: Uint8Array RGBA
  const raw = Buffer.alloc((width * 4 + 1) * height);
  for (let y = 0; y < height; y++) {
    raw[y * (width * 4 + 1)] = 0;
    Buffer.from(data.buffer, data.byteOffset + y * width * 4, width * 4).copy(raw, y * (width * 4 + 1) + 1);
  }
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(width, 0); ihdr.writeUInt32BE(height, 4);
  ihdr[8] = 8; ihdr[9] = 6; ihdr[10] = 0; ihdr[11] = 0; ihdr[12] = 0;
  return Buffer.concat([
    Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
    chunk('IHDR', ihdr), chunk('IDAT', zlib.deflateSync(raw, { level: 9 })), chunk('IEND', Buffer.alloc(0)),
  ]);
}
function decodePng(buf) {
  // Minimal decoder for 8-bit RGBA/RGB non-interlaced PNGs (what this tool writes).
  let pos = 8, width, height, colorType, idat = [];
  while (pos < buf.length) {
    const len = buf.readUInt32BE(pos); const type = buf.toString('ascii', pos + 4, pos + 8);
    const data = buf.slice(pos + 8, pos + 8 + len);
    if (type === 'IHDR') { width = data.readUInt32BE(0); height = data.readUInt32BE(4); colorType = data[9]; }
    if (type === 'IDAT') idat.push(data);
    pos += 12 + len;
  }
  const bpp = colorType === 6 ? 4 : 3;
  const raw = zlib.inflateSync(Buffer.concat(idat));
  const out = newImage(width, height);
  let prev = Buffer.alloc(width * bpp);
  for (let y = 0; y < height; y++) {
    const f = raw[y * (width * bpp + 1)];
    const line = Buffer.from(raw.slice(y * (width * bpp + 1) + 1, (y + 1) * (width * bpp + 1)));
    for (let i = 0; i < line.length; i++) {
      const a = i >= bpp ? line[i - bpp] : 0, b = prev[i], c = i >= bpp ? prev[i - bpp] : 0;
      if (f === 1) line[i] = (line[i] + a) & 255;
      else if (f === 2) line[i] = (line[i] + b) & 255;
      else if (f === 3) line[i] = (line[i] + ((a + b) >> 1)) & 255;
      else if (f === 4) { const p = a + b - c, pa = Math.abs(p - a), pb = Math.abs(p - b), pc = Math.abs(p - c); line[i] = (line[i] + (pa <= pb && pa <= pc ? a : pb <= pc ? b : c)) & 255; }
    }
    for (let x = 0; x < width; x++) {
      const o = (y * width + x) * 4;
      out.data[o] = line[x * bpp]; out.data[o + 1] = line[x * bpp + 1]; out.data[o + 2] = line[x * bpp + 2];
      out.data[o + 3] = bpp === 4 ? line[x * bpp + 3] : 255;
    }
    prev = line;
  }
  return out;
}

// ---------- image helpers ----------
function newImage(width, height, rgba = [0, 0, 0, 0]) {
  const data = new Uint8Array(width * height * 4);
  for (let i = 0; i < width * height; i++) data.set(rgba, i * 4);
  return { width, height, data };
}
function hexToRgba(hex) {
  const h = hex.replace('#', '');
  const v = h.length === 3 ? h.split('').map(c => c + c).join('') : h;
  return [parseInt(v.slice(0, 2), 16), parseInt(v.slice(2, 4), 16), parseInt(v.slice(4, 6), 16), v.length === 8 ? parseInt(v.slice(6, 8), 16) : 255];
}
function blit(dst, src, dx, dy) {
  for (let y = 0; y < src.height; y++) for (let x = 0; x < src.width; x++) {
    const tx = dx + x, ty = dy + y;
    if (tx < 0 || ty < 0 || tx >= dst.width || ty >= dst.height) continue;
    const s = (y * src.width + x) * 4, d = (ty * dst.width + tx) * 4;
    const a = src.data[s + 3] / 255;
    if (a === 0) continue;
    for (let k = 0; k < 3; k++) dst.data[d + k] = Math.round(src.data[s + k] * a + dst.data[d + k] * (1 - a));
    dst.data[d + 3] = Math.max(dst.data[d + 3], src.data[s + 3]);
  }
}
function scaleImage(img, s) {
  const out = newImage(img.width * s, img.height * s);
  for (let y = 0; y < out.height; y++) for (let x = 0; x < out.width; x++) {
    const si = (Math.floor(y / s) * img.width + Math.floor(x / s)) * 4;
    out.data.set(img.data.subarray(si, si + 4), (y * out.width + x) * 4);
  }
  return out;
}
function gridToImage(rows, palette, label) {
  const height = rows.length, width = rows[0].length;
  const img = newImage(width, height);
  const errors = [];
  rows.forEach((row, y) => {
    if (row.length !== width) errors.push(`${label}: row ${y} has ${row.length} chars, expected ${width}`);
    [...row].forEach((ch, x) => {
      if (ch === '.' || ch === ' ') return;
      if (!palette[ch]) { errors.push(`${label}: unknown palette key '${ch}' at row ${y} col ${x}`); return; }
      img.data.set(palette[ch], (y * width + x) * 4);
    });
  });
  return { img, errors };
}
function flipX(rows) { return rows.map(r => [...r].reverse().join('')); }
function flipY(rows) { return [...rows].reverse(); }
function rotateCW(rows) { const n = rows.length; return rows[0].split('').map((_, x) => rows.map((_, y) => rows[n - 1 - y][x]).join('')); }
function overlay(base, top) { return base.map((r, y) => [...r].map((c, x) => (top[y][x] !== '.' && top[y][x] !== ' ' ? top[y][x] : c)).join('')); }

// ---------- wall autotiling (shared rules with the C# side) ----------
// 8-neighbour bits: N=1, NE=2, E=4, SE=8, S=16, SW=32, W=64, NW=128 (bit set when the neighbour is a wall).
const BIT = { N: 1, NE: 2, E: 4, SE: 8, S: 16, SW: 32, W: 64, NW: 128 };
function reduceMask(m) {
  if (!((m & BIT.N) && (m & BIT.E))) m &= ~BIT.NE;
  if (!((m & BIT.S) && (m & BIT.E))) m &= ~BIT.SE;
  if (!((m & BIT.S) && (m & BIT.W))) m &= ~BIT.SW;
  if (!((m & BIT.N) && (m & BIT.W))) m &= ~BIT.NW;
  return m;
}
const BLOB_MASKS = [...new Set([...Array(256).keys()].map(reduceMask))].sort((a, b) => a - b); // 47 entries
function quadrantPattern(q, vert, horiz, diag) {
  if (!vert && !horiz) return q.outer;
  if (vert && !horiz) return q.vertical;
  if (!vert && horiz) return q.horizontal;
  return diag ? q.fill : q.inner;
}
function blobTileRows(walls, mask) {
  // "quadrants": one NE-authored pattern set mirrored into all four corners (symmetric lighting).
  // "corners": one NE-authored pattern set per corner (ne/nw/se/sw), still flipped into place, so lighting can differ per side.
  const sets = walls.mode === 'corners' ? walls.corners : { ne: walls.quadrants, nw: walls.quadrants, se: walls.quadrants, sw: walls.quadrants };
  const has = b => (mask & b) !== 0;
  // NE quadrant uses N (vertical continuation) and E (horizontal continuation).
  const ne = quadrantPattern(sets.ne, has(BIT.N), has(BIT.E), has(BIT.NE));
  const nw = flipX(quadrantPattern(sets.nw, has(BIT.N), has(BIT.W), has(BIT.NW)));
  const se = flipY(quadrantPattern(sets.se, has(BIT.S), has(BIT.E), has(BIT.SE)));
  const sw = flipX(flipY(quadrantPattern(sets.sw, has(BIT.S), has(BIT.W), has(BIT.SW))));
  const half = ne.length;
  const rows = [];
  for (let y = 0; y < half; y++) rows.push(nw[y] + ne[y]);
  for (let y = 0; y < half; y++) rows.push(sw[y] + se[y]);
  return rows;
}
function cardinalTileRows(walls, mask4) {
  let rows = walls.base;
  let arm = walls.armN;
  const order = [1, 2, 4, 8]; // N, E, S, W
  for (const bit of order) {
    if (mask4 & bit) rows = overlay(rows, arm);
    arm = rotateCW(arm);
  }
  return rows;
}
function wallSheet(walls, palette) {
  const errors = [];
  const tiles = walls.mode === 'cardinal'
    ? [...Array(16).keys()].map(m => cardinalTileRows(walls, m))
    : BLOB_MASKS.map(m => blobTileRows(walls, m));
  const size = tiles[0].length;
  const sheet = newImage(size * tiles.length, size);
  const images = tiles.map((rows, i) => { const r = gridToImage(rows, palette, `wall tile ${i}`); errors.push(...r.errors); blit(sheet, r.img, i * size, 0); return r.img; });
  return { sheet, images, errors, size };
}
function wallIndexFor(walls, map, x, y, outOfBoundsIsWall) {
  const isWall = (cx, cy) => (cy < 0 || cy >= map.length || cx < 0 || cx >= map[0].length) ? outOfBoundsIsWall : map[cy][cx] === '#';
  if (walls.mode === 'cardinal') {
    return (isWall(x, y - 1) ? 1 : 0) | (isWall(x + 1, y) ? 2 : 0) | (isWall(x, y + 1) ? 4 : 0) | (isWall(x - 1, y) ? 8 : 0);
  }
  let m = 0;
  if (isWall(x, y - 1)) m |= BIT.N; if (isWall(x + 1, y - 1)) m |= BIT.NE; if (isWall(x + 1, y)) m |= BIT.E; if (isWall(x + 1, y + 1)) m |= BIT.SE;
  if (isWall(x, y + 1)) m |= BIT.S; if (isWall(x - 1, y + 1)) m |= BIT.SW; if (isWall(x - 1, y)) m |= BIT.W; if (isWall(x - 1, y - 1)) m |= BIT.NW;
  return BLOB_MASKS.indexOf(reduceMask(m));
}

// ---------- commands ----------
function loadSpec(file) {
  const spec = JSON.parse(fs.readFileSync(file, 'utf8'));
  const palette = Object.fromEntries(Object.entries(spec.palette || {}).map(([k, v]) => [k, hexToRgba(v)]));
  return { spec, palette };
}
function renderSprites(specFile, outDir, previewScale = 8) {
  const { spec, palette } = loadSpec(specFile);
  fs.mkdirSync(outDir, { recursive: true });
  const errors = [];
  const strips = [];
  for (const [name, sprite] of Object.entries(spec.sprites || {})) {
    const frames = sprite.frames.map((rows, i) => { const r = gridToImage(rows, palette, `${name}[${i}]`); errors.push(...r.errors); return r.img; });
    const w = frames[0].width, h = frames[0].height;
    const strip = newImage(w * frames.length, h);
    frames.forEach((f, i) => blit(strip, f, i * w, 0));
    fs.writeFileSync(path.join(outDir, `${name}.png`), encodePng(strip));
    strips.push({ name, strip, w, h, count: frames.length });
  }
  if (spec.walls) {
    const ws = wallSheet(spec.walls, palette);
    errors.push(...ws.errors);
    fs.writeFileSync(path.join(outDir, `walls.png`), encodePng(ws.sheet));
    strips.push({ name: 'walls', strip: ws.sheet, w: ws.size, h: ws.size, count: ws.images.length });
  }
  // Contact sheet: each strip on its own row, on a dark checker so transparency is visible.
  const pad = 4;
  const sheetW = Math.max(...strips.map(s => s.strip.width + (s.count - 1) * pad)) + pad * 2;
  const sheetH = strips.reduce((a, s) => a + s.h + pad, pad);
  const sheet = newImage(sheetW, sheetH, [24, 24, 32, 255]);
  for (let y = 0; y < sheetH; y++) for (let x = 0; x < sheetW; x++) if (((x >> 2) + (y >> 2)) % 2) sheet.data.set([32, 32, 44, 255], (y * sheetW + x) * 4);
  let cy = pad;
  const legend = [];
  for (const s of strips) {
    for (let i = 0; i < s.count; i++) {
      const frame = newImage(s.w, s.h);
      blit(frame, { width: s.w, height: s.h, data: cropData(s.strip, i * s.w, 0, s.w, s.h) }, 0, 0);
      blit(sheet, frame, pad + i * (s.w + pad), cy);
    }
    legend.push(`row y=${cy}: ${s.name} (${s.count} frame${s.count > 1 ? 's' : ''}, ${s.w}x${s.h})`);
    cy += s.h + pad;
  }
  const previewPath = path.join(outDir, `_preview_x${previewScale}.png`);
  fs.writeFileSync(previewPath, encodePng(scaleImage(sheet, previewScale)));
  console.log(legend.join('\n'));
  console.log(`preview: ${previewPath}`);
  if (errors.length) { console.error('ERRORS:\n' + errors.join('\n')); process.exitCode = 1; }
}
function cropData(img, x0, y0, w, h) {
  const out = new Uint8Array(w * h * 4);
  for (let y = 0; y < h; y++) out.set(img.data.subarray(((y0 + y) * img.width + x0) * 4, ((y0 + y) * img.width + x0 + w) * 4), y * w * 4);
  return out;
}
function loadStripFrame(file, frameIndex, frameWidth) {
  const img = decodePng(fs.readFileSync(file));
  const w = frameWidth || img.height;
  return { width: w, height: img.height, data: cropData(img, frameIndex * w, 0, w, img.height) };
}
// mockup.json: { "map": "path/to/map.txt", "tile": 16, "background": "#000", "corridor": "#000" (optional),
//   "wallSpec": "walls spec.json", "outOfBoundsIsWall": false,
//   "layers": [ { "png": "pellet.png", "frame": 0, "cells": "pellets" | [[x,y],...] , "offset": [0,0] } ],
//   "overlay": { "scanlines": 0.15 } }
function renderMockup(configFile, outFile, scale = 3) {
  const cfg = JSON.parse(fs.readFileSync(configFile, 'utf8'));
  const baseDir = path.dirname(configFile);
  const rel = p => path.resolve(baseDir, p);
  const map = fs.readFileSync(rel(cfg.map), 'utf8').split('\n').filter(l => /^\s*\d+ /.test(l)).map(l => l.replace(/^\s*\d+ /, ''));
  const tile = cfg.tile || 16;
  const hudTop = cfg.hudTop || 0;
  const img = newImage(map[0].length * tile, map.length * tile + hudTop, hexToRgba(cfg.background || '#000000'));
  const { spec: wspec, palette: wpal } = loadSpec(rel(cfg.wallSpec));
  const ws = wallSheet(wspec.walls, wpal);
  if (ws.errors.length) console.error(ws.errors.join('\n'));
  map.forEach((row, y) => [...row].forEach((ch, x) => {
    if (ch === '#') blit(img, ws.images[wallIndexFor(wspec.walls, map, x, y, !!cfg.outOfBoundsIsWall)], x * tile, y * tile + hudTop);
    else if (cfg.corridor) blit(img, newImage(tile, tile, hexToRgba(cfg.corridor)), x * tile, y * tile + hudTop);
  }));
  for (const layer of cfg.layers || []) {
    const sprite = loadStripFrame(rel(layer.png), layer.frame || 0, layer.frameWidth);
    const cells = layer.cells === 'pellets'
      ? map.flatMap((row, y) => [...row].map((ch, x) => (ch === '.' ? [x, y] : null)).filter(Boolean))
      : layer.cells;
    const [ox, oy] = layer.offset || [0, 0];
    for (const [x, y] of cells) blit(img, sprite, x * tile + Math.floor((tile - sprite.width) / 2) + ox, y * tile + hudTop + Math.floor((tile - sprite.height) / 2) + oy);
  }
  let out = scaleImage(img, scale);
  if (cfg.overlay && cfg.overlay.vignette) {
    // Smooth radial darkening (what a bilinear-filtered vignette Image over the maze does); rows above keepTop (the HUD) stay untouched.
    const k = cfg.overlay.vignette, keepTop = (cfg.overlay.vignetteKeepTop || 0) * scale;
    const cx = out.width / 2, cy = keepTop + (out.height - keepTop) / 2;
    for (let y = keepTop; y < out.height; y++) for (let x = 0; x < out.width; x++) {
      const dx = (x + 0.5 - cx) / cx, dy = (y + 0.5 - cy) / (out.height - cy);
      const t = Math.max(0, (Math.min(1, Math.hypot(dx, dy) / Math.SQRT2 * 1.25) - 0.45) / 0.55);
      const f = 1 - k * t * t * (3 - 2 * t);
      const o = (y * out.width + x) * 4;
      for (let c = 0; c < 3; c++) out.data[o + c] = Math.round(out.data[o + c] * f);
    }
  }
  if (cfg.overlay && cfg.overlay.scanlines) {
    const a = cfg.overlay.scanlines;
    for (let y = 0; y < out.height; y++) if (y % scale === scale - 1) for (let x = 0; x < out.width; x++) {
      const o = (y * out.width + x) * 4;
      for (let k = 0; k < 3; k++) out.data[o + k] = Math.round(out.data[o + k] * (1 - a));
    }
  }
  fs.writeFileSync(outFile, encodePng(out));
  console.log(`mockup: ${outFile} (${out.width}x${out.height})`);
}

module.exports = { BLOB_MASKS, reduceMask, encodePng, decodePng };
if (require.main === module) {
  const [cmd, a, b, c] = process.argv.slice(2);
  if (cmd === 'sprites') renderSprites(a, b, c ? parseInt(c, 10) : 8);
  else if (cmd === 'mockup') renderMockup(a, b, c ? parseInt(c, 10) : 3);
  else if (cmd === 'masks') console.log(JSON.stringify(BLOB_MASKS));
  else { console.error('usage: render.js sprites <spec.json> <outDir> [scale] | mockup <mockup.json> <out.png> [scale] | masks'); process.exit(2); }
}

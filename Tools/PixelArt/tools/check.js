#!/usr/bin/env node
// Readability checks for a rendered sprite set:
// WCAG contrast vs the background and wall fill, CIEDE2000 between actors (normal and colour-blind simulations),
// silhouette overlap, and hue echoes of the original arcade ghost colours on the same behaviour role.
//   node check.js <spec.json> <outDir>
const fs = require('fs');
const path = require('path');
const { decodePng } = require('./render.js');

const hex = h => { const v = h.replace('#', ''); return [0, 2, 4].map(i => parseInt(v.slice(i, i + 2), 16)); };
const toHex = c => '#' + c.slice(0, 3).map(v => v.toString(16).padStart(2, '0')).join('');
const lin = v => { v /= 255; return v <= 0.04045 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4); };
const unlin = v => { v = Math.max(0, Math.min(1, v)); return Math.round(255 * (v <= 0.0031308 ? v * 12.92 : 1.055 * Math.pow(v, 1 / 2.4) - 0.055)); };
const relLum = c => 0.2126 * lin(c[0]) + 0.7152 * lin(c[1]) + 0.0722 * lin(c[2]);
const contrast = (a, b) => { const la = relLum(a), lb = relLum(b); return (Math.max(la, lb) + 0.05) / (Math.min(la, lb) + 0.05); };
function toLab(c) {
  const [r, g, b] = c.map(lin);
  let x = (0.4124 * r + 0.3576 * g + 0.1805 * b) / 0.95047, y = 0.2126 * r + 0.7152 * g + 0.0722 * b, z = (0.0193 * r + 0.1192 * g + 0.9505 * b) / 1.08883;
  const f = t => (t > 216 / 24389 ? Math.cbrt(t) : (24389 / 27 * t + 16) / 116);
  x = f(x); y = f(y); z = f(z);
  return [116 * y - 16, 500 * (x - y), 200 * (y - z)];
}
function de2000(c1, c2) {
  const [L1, a1, b1] = toLab(c1), [L2, a2, b2] = toLab(c2), rad = Math.PI / 180;
  const C1 = Math.hypot(a1, b1), C2 = Math.hypot(a2, b2), Cb = (C1 + C2) / 2;
  const G = 0.5 * (1 - Math.sqrt(Math.pow(Cb, 7) / (Math.pow(Cb, 7) + Math.pow(25, 7))));
  const a1p = (1 + G) * a1, a2p = (1 + G) * a2, C1p = Math.hypot(a1p, b1), C2p = Math.hypot(a2p, b2);
  const h = (a, b) => { if (a === 0 && b === 0) return 0; const v = Math.atan2(b, a) / rad; return v < 0 ? v + 360 : v; };
  const h1p = h(a1p, b1), h2p = h(a2p, b2), dLp = L2 - L1, dCp = C2p - C1p;
  let dhp = 0;
  if (C1p * C2p !== 0) { dhp = h2p - h1p; if (dhp > 180) dhp -= 360; else if (dhp < -180) dhp += 360; }
  const dHp = 2 * Math.sqrt(C1p * C2p) * Math.sin(dhp / 2 * rad), Lbp = (L1 + L2) / 2, Cbp = (C1p + C2p) / 2;
  let hbp = h1p + h2p;
  if (C1p * C2p !== 0) { if (Math.abs(h1p - h2p) > 180) hbp += h1p + h2p < 360 ? 360 : -360; hbp /= 2; }
  const T = 1 - 0.17 * Math.cos((hbp - 30) * rad) + 0.24 * Math.cos(2 * hbp * rad) + 0.32 * Math.cos((3 * hbp + 6) * rad) - 0.2 * Math.cos((4 * hbp - 63) * rad);
  const dTh = 30 * Math.exp(-Math.pow((hbp - 275) / 25, 2)), RC = 2 * Math.sqrt(Math.pow(Cbp, 7) / (Math.pow(Cbp, 7) + Math.pow(25, 7)));
  const SL = 1 + (0.015 * Math.pow(Lbp - 50, 2)) / Math.sqrt(20 + Math.pow(Lbp - 50, 2)), SC = 1 + 0.045 * Cbp, SH = 1 + 0.015 * Cbp * T, RT = -Math.sin(2 * dTh * rad) * RC;
  return Math.sqrt(Math.pow(dLp / SL, 2) + Math.pow(dCp / SC, 2) + Math.pow(dHp / SH, 2) + RT * (dCp / SC) * (dHp / SH));
}
function hue(c) {
  const [r, g, b] = c.map(v => v / 255), mx = Math.max(r, g, b), mn = Math.min(r, g, b), d = mx - mn;
  if (d === 0) return null;
  let h = mx === r ? ((g - b) / d) % 6 : mx === g ? (b - r) / d + 2 : (r - g) / d + 4;
  h *= 60; return h < 0 ? h + 360 : h;
}
const sat = c => { const mx = Math.max(...c), mn = Math.min(...c); return mx === 0 ? 0 : (mx - mn) / mx; };
const hueDist = (a, b) => { if (a == null || b == null) return null; const d = Math.abs(a - b) % 360; return d > 180 ? 360 - d : d; };
const CVD = {
  deutan: [[0.367322, 0.860646, -0.227968], [0.280085, 0.672501, 0.047413], [-0.01182, 0.04294, 0.968881]],
  protan: [[0.152286, 1.052583, -0.204868], [0.114503, 0.786281, 0.099216], [-0.003882, -0.048116, 1.051998]],
  tritan: [[1.255528, -0.076749, -0.178779], [-0.078411, 0.930809, 0.147602], [0.004733, 0.691367, 0.3039]],
};
const simulate = (c, m) => { const l = c.slice(0, 3).map(lin); return m.map(row => unlin(row[0] * l[0] + row[1] * l[1] + row[2] * l[2])); };
const GHOSTS = { enemy_chase: ['Blinky red', '#ff0000'], enemy_cutoff: ['Pinky pink', '#ffb8ff'], enemy_scared: ['Clyde orange', '#ffb852'], enemy_random: ['Inky cyan', '#00ffff'] };

function frames(file) {
  const img = decodePng(fs.readFileSync(file));
  const n = Math.max(1, Math.round(img.width / img.height)), w = img.width / n;
  return [...Array(n).keys()].map(i => {
    const px = [];
    for (let y = 0; y < img.height; y++) for (let x = 0; x < w; x++) { const o = (y * img.width + i * w + x) * 4; px.push([img.data[o], img.data[o + 1], img.data[o + 2], img.data[o + 3]]); }
    return { w, h: img.height, px };
  });
}
function bodyColour(f) {
  const counts = new Map();
  for (const p of f.px) if (p[3] >= 128 && relLum(p) > 0.02) { const k = toHex(p); counts.set(k, (counts.get(k) || 0) + 1); }
  const [k] = [...counts.entries()].sort((a, b) => b[1] - a[1])[0] || ['#000000'];
  return hex(k);
}
const mask = f => f.px.map(p => p[3] >= 128);
function iou(a, b) { let i = 0, u = 0; a.forEach((v, k) => { if (v && b[k]) i++; if (v || b[k]) u++; }); return u ? i / u : 0; }
function count(f, pred) { return f.px.filter(pred).length; }
function bbox(f) {
  let x0 = 99, y0 = 99, x1 = -1, y1 = -1;
  f.px.forEach((p, i) => { if (p[3] < 128) return; const x = i % f.w, y = Math.floor(i / f.w); x0 = Math.min(x0, x); y0 = Math.min(y0, y); x1 = Math.max(x1, x); y1 = Math.max(y1, y); });
  return `${x1 - x0 + 1}x${y1 - y0 + 1}`;
}

if (require.main === module) {
  const [specFile, outDir] = process.argv.slice(2);
  const spec = JSON.parse(fs.readFileSync(specFile, 'utf8'));
  const pal = Object.fromEntries(Object.entries(spec.palette).map(([k, v]) => [k, hex(v)]));
  const bg = pal.N, wall = pal.J;
  console.log('palette contrast vs background N and wall core J:');
  for (const [k, c] of Object.entries(pal)) console.log(`  ${k} ${toHex(c)}  vs N ${contrast(c, bg).toFixed(2).padStart(5)}:1  vs J ${contrast(c, wall).toFixed(2).padStart(5)}:1`);
  const actors = ['hero_side', 'hero', 'enemy_chase', 'enemy_cutoff', 'enemy_scared', 'enemy_random'].filter(n => fs.existsSync(path.join(outDir, n + '.png')));
  const others = Object.keys(spec.sprites).filter(n => !actors.includes(n));
  const S = Object.fromEntries([...actors, ...others].map(n => [n, frames(path.join(outDir, n + '.png'))]));
  console.log('\nsprites (frame 0): body colour, contrast vs N and vs J, bbox, opaque px, lit px (non-outline)');
  for (const n of [...actors, ...others]) {
    const f = S[n][0], c = bodyColour(f);
    console.log(`  ${n.padEnd(18)} ${toHex(c)} hue ${hue(c) == null ? '  -' : hue(c).toFixed(0).padStart(3)}  vsN ${contrast(c, bg).toFixed(1).padStart(4)}  vsJ ${contrast(c, wall).toFixed(1).padStart(4)}  bbox ${bbox(f).padEnd(5)} opaque ${String(count(f, p => p[3] >= 128)).padStart(3)} lit ${String(count(f, p => p[3] >= 128 && relLum(p) > 0.02)).padStart(3)}  frames ${S[n].length}`);
  }
  console.log('\nactor pairs: dE2000 | deutan | protan | tritan | silhouette IoU | lum ratio');
  for (let i = 0; i < actors.length; i++) for (let j = i + 1; j < actors.length; j++) {
    const a = S[actors[i]][0], b = S[actors[j]][0], ca = bodyColour(a), cb = bodyColour(b);
    const d = de2000(ca, cb), dd = de2000(simulate(ca, CVD.deutan), simulate(cb, CVD.deutan)), dp = de2000(simulate(ca, CVD.protan), simulate(cb, CVD.protan)), dt = de2000(simulate(ca, CVD.tritan), simulate(cb, CVD.tritan));
    const flag = Math.min(d, dd, dp, dt) < 15 ? '  <-- low colour separation (silhouette must carry it)' : '';
    console.log(`  ${actors[i].replace('enemy_', '').padEnd(7)} vs ${actors[j].replace('enemy_', '').padEnd(7)} ${d.toFixed(1).padStart(5)} | ${dd.toFixed(1).padStart(5)} | ${dp.toFixed(1).padStart(5)} | ${dt.toFixed(1).padStart(5)} | ${iou(mask(a), mask(b)).toFixed(2)} | ${contrast(ca, cb).toFixed(2)}${flag}`);
  }
  if (S.pellet) {
    const pc = bodyColour(S.pellet[0]);
    const bright = f => count(f, p => p[3] >= 128 && relLum(p) > 0.2);
    console.log(`\npellet: body ${toHex(pc)}, frame bright px ${S.pellet.map(bright).join('/')}, bboxes ${S.pellet.map(bbox).join('/')}`);
    for (const n of actors) console.log(`  pellet vs ${n} body dE2000 ${de2000(pc, bodyColour(S[n][0])).toFixed(1)}`);
  }
  console.log('\nrole echo vs original ghost colours (hue distance, < 30 deg with saturation > 0.35 = echo):');
  for (const [role, [name, gh]] of Object.entries(GHOSTS)) {
    if (!S[role]) continue;
    const c = bodyColour(S[role][0]), hd = hueDist(hue(c), hue(hex(gh)));
    const near = Object.values(GHOSTS).map(([nm, g]) => [nm, hueDist(hue(c), hue(hex(g)))]).filter(([, d]) => d != null && d < 30 && sat(c) > 0.35).map(([nm, d]) => `${nm} ${d.toFixed(0)}deg`);
    console.log(`  ${role.padEnd(13)} ${toHex(c)} vs ${name}: ${hd == null ? 'achromatic' : hd.toFixed(0) + ' deg'}${hd != null && hd < 30 && sat(c) > 0.35 ? '  <-- SAME-ROLE ECHO' : ''}${near.length ? '  (near ' + near.join(', ') + ')' : ''}`);
  }
  const heroKey = S.hero_side ? 'hero_side' : 'hero'; if (S[heroKey]) { const hc = bodyColour(S[heroKey][0]), hh = hue(hc); console.log(`  hero body ${toHex(hc)}: ${hh != null && hueDist(hh, 60) < 20 && sat(hc) > 0.5 ? 'YELLOW ECHO' : 'not Pac-Man yellow'}`); }
}
module.exports = { contrast, de2000, simulate, CVD, hex, hue, relLum };

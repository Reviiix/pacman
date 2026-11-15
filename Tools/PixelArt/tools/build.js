#!/usr/bin/env node
// Builds spec.json from the hand-drawn ASCII source art.txt, so sprites and wall quadrants are edited as plain text blocks.
//   @palette              then lines "K #rrggbb role words"
//   @sprite <name>        starts a sprite; the @frame blocks after it are its flipbook frames, left to right
//   @frame [mirror]       16 rows of 16 chars, or 16 rows of 8 chars mirrored left->right when "mirror" is given
//   @walls corners        then @corner ne|nw|se|sw, each followed by @outer/@vertical/@horizontal/@inner/@fill (8 rows x 8 chars)
//   @walls quadrants      then @outer/@vertical/@horizontal/@inner/@fill once (mirrored into every corner)
// Lines starting with "//" are comments. Usage: node build.js art.txt spec.json
const fs = require('fs');
const [src, dst] = process.argv.slice(2);
if (!src || !dst) { console.error('usage: node build.js <art.txt> <spec.json>'); process.exit(2); }
const lines = fs.readFileSync(src, 'utf8').split('\n').map(l => l.replace(/\s+$/, ''));
const spec = { palette: {}, paletteRoles: {}, sprites: {}, walls: null };
const PATTERNS = ['outer', 'vertical', 'horizontal', 'inner', 'fill'];
let section = null, sprite = null, frame = null, mirror = false, corner = null, quadrant = null;
const errors = [];
const target = () => (spec.walls.mode === 'corners' ? spec.walls.corners[corner] : spec.walls.quadrants);
function closeFrame() {
  if (!frame) return;
  const rows = mirror ? frame.map(r => r + [...r].reverse().join('')) : frame;
  if (rows.length !== 16 || rows.some(r => r.length !== 16)) errors.push(`${sprite} frame ${spec.sprites[sprite].frames.length}: ${rows.length} rows, widths ${rows.map(r => r.length).join(',')}`);
  spec.sprites[sprite].frames.push(rows);
  frame = null;
}
function closeQuadrant() {
  if (!quadrant) return;
  const rows = target()[quadrant];
  if (rows.length !== 8 || rows.some(r => r.length !== 8)) errors.push(`walls.${corner ? corner + '.' : ''}${quadrant}: ${rows.length} rows, widths ${rows.map(r => r.length).join(',')}`);
  quadrant = null;
}
for (const raw of lines) {
  const line = raw.trim();
  if (!line || line.startsWith('//')) continue;
  if (line.startsWith('@')) {
    closeFrame(); closeQuadrant();
    const [cmd, arg] = line.slice(1).split(/\s+/);
    if (cmd === 'palette') section = 'palette';
    else if (cmd === 'sprite') { section = 'sprite'; sprite = arg; spec.sprites[sprite] = { frames: [] }; }
    else if (cmd === 'frame') { frame = []; mirror = arg === 'mirror'; }
    else if (cmd === 'walls') {
      section = 'walls';
      spec.walls = arg === 'corners' ? { mode: 'corners', corners: {} } : { mode: 'quadrants', quadrants: {} };
    } else if (cmd === 'corner') { corner = arg; spec.walls.corners[corner] = {}; }
    else if (PATTERNS.includes(cmd)) { quadrant = cmd; target()[cmd] = []; }
    else errors.push(`unknown directive ${line}`);
    continue;
  }
  if (section === 'palette') {
    const [key, hex, ...role] = line.split(/\s+/);
    spec.palette[key] = hex; spec.paletteRoles[key] = role.join(' ');
  } else if (frame) frame.push(line);
  else if (quadrant) target()[quadrant].push(line);
}
closeFrame(); closeQuadrant();
if (spec.walls && spec.walls.mode === 'corners') {
  for (const c of ['ne', 'nw', 'se', 'sw']) for (const p of PATTERNS) if (!spec.walls.corners[c] || !spec.walls.corners[c][p]) errors.push(`walls.corners.${c}.${p} is missing`);
}
if (errors.length) { console.error('BUILD ERRORS:\n' + errors.join('\n')); process.exit(1); }
const fmt = rows => '[\n        ' + rows.map(r => JSON.stringify(r)).join(',\n        ') + '\n      ]';
const fmtSet = (set, indent) => '{\n' + PATTERNS.map(p => `${indent}  "${p}": ${JSON.stringify(set[p])}`).join(',\n') + `\n${indent}}`;
let out = '{\n  "palette": ' + JSON.stringify(spec.palette).replace(/,/g, ', ') + ',\n';
out += '  "paletteRoles": ' + JSON.stringify(spec.paletteRoles, null, 4).replace(/\n/g, '\n  ') + ',\n';
out += '  "sprites": {\n' + Object.entries(spec.sprites).map(([n, s]) => `    "${n}": { "frames": [\n      ` + s.frames.map(fmt).join(',\n      ') + '\n    ] }').join(',\n') + '\n  }';
if (spec.walls && spec.walls.mode === 'corners') {
  out += ',\n  "walls": { "mode": "corners", "corners": {\n' + ['ne', 'nw', 'se', 'sw'].map(c => `    "${c}": ${fmtSet(spec.walls.corners[c], '    ')}`).join(',\n') + '\n  } }';
} else if (spec.walls) {
  out += ',\n  "walls": { "mode": "quadrants", "quadrants": ' + fmtSet(spec.walls.quadrants, '  ') + ' }';
}
out += '\n}\n';
JSON.parse(out);
fs.writeFileSync(dst, out);
console.log(`wrote ${dst}: ${Object.keys(spec.sprites).length} sprites, ${Object.keys(spec.palette).length} colours, walls ${spec.walls ? spec.walls.mode : 'none'}`);

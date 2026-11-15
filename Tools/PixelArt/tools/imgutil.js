#!/usr/bin/env node
// Small PNG helpers for review images: side-by-side stacks and integer-scaled crops.
//   node imgutil.js hstack <out.png> <a.png> <b.png> ...      (12px grey gutter)
//   node imgutil.js crop <in.png> <out.png> <x> <y> <w> <h> [scale]
const fs = require('fs');
const { decodePng, encodePng } = require('./render.js');
function hstack(outFile, files) {
  const imgs = files.map(f => decodePng(fs.readFileSync(f)));
  const gutter = 12, W = imgs.reduce((s, i) => s + i.width, 0) + gutter * (imgs.length - 1), H = Math.max(...imgs.map(i => i.height));
  const out = { width: W, height: H, data: new Uint8Array(W * H * 4).fill(90) };
  let ox = 0;
  for (const im of imgs) {
    for (let y = 0; y < im.height; y++) for (let x = 0; x < im.width; x++) {
      const si = (y * im.width + x) * 4, di = (y * W + ox + x) * 4;
      for (let k = 0; k < 3; k++) out.data[di + k] = im.data[si + k];
    }
    ox += im.width + gutter;
  }
  for (let i = 3; i < out.data.length; i += 4) out.data[i] = 255;
  fs.writeFileSync(outFile, encodePng(out));
  console.log(`${outFile} ${W}x${H}`);
}
function crop(inFile, outFile, x, y, w, h, s = 1) {
  const img = decodePng(fs.readFileSync(inFile));
  const W = Math.min(w, img.width - x), H = Math.min(h, img.height - y);
  const out = { width: W * s, height: H * s, data: new Uint8Array(W * s * H * s * 4) };
  for (let j = 0; j < H * s; j++) for (let i = 0; i < W * s; i++) {
    const si = ((y + Math.floor(j / s)) * img.width + (x + Math.floor(i / s))) * 4, di = (j * W * s + i) * 4;
    for (let k = 0; k < 4; k++) out.data[di + k] = img.data[si + k];
  }
  fs.writeFileSync(outFile, encodePng(out));
  console.log(`${outFile} ${out.width}x${out.height}`);
}
function pad(inFile, outFile, px, hex) {
  const img = decodePng(fs.readFileSync(inFile));
  const c = [1, 3, 5].map(i => parseInt(hex.slice(i, i + 2), 16));
  const W = img.width + px * 2, H = img.height + px * 2;
  const out = { width: W, height: H, data: new Uint8Array(W * H * 4) };
  for (let i = 0; i < W * H; i++) out.data.set([...c, 255], i * 4);
  for (let y = 0; y < img.height; y++) out.data.set(img.data.subarray(y * img.width * 4, (y + 1) * img.width * 4), ((y + px) * W + px) * 4);
  fs.writeFileSync(outFile, encodePng(out));
  console.log(`${outFile} ${W}x${H}`);
}
module.exports = { hstack, crop, pad };
if (require.main === module) {
  const [cmd, ...a] = process.argv.slice(2);
  if (cmd === 'hstack') hstack(a[0], a.slice(1));
  else if (cmd === 'crop') crop(a[0], a[1], +a[2], +a[3], +a[4], +a[5], a[6] ? +a[6] : 1);
  else if (cmd === 'pad') pad(a[0], a[1], +a[2], a[3]);
  else { console.error('usage: imgutil.js hstack <out> <in...> | crop <in> <out> x y w h [scale] | pad <in> <out> <px> <#hex>'); process.exit(2); }
}

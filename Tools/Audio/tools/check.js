#!/usr/bin/env node
// Reports each WAV in a folder: length, peak and RMS level, DC offset and the first and last samples (non-zero edges click).
// Usage: node tools/check.js out
const fs = require('fs');
const path = require('path');
const dir = process.argv[2] || 'out';
const db = v => (v > 0 ? (20 * Math.log10(v)).toFixed(1) : '-inf');
console.log('file                 seconds  peak dBFS  rms dBFS  dc       first  last');
for (const file of fs.readdirSync(dir).filter(f => f.endsWith('.wav')).sort()) {
  const buf = fs.readFileSync(path.join(dir, file));
  const rate = buf.readUInt32LE(24);
  const data = new Int16Array(buf.buffer.slice(buf.byteOffset + 44, buf.byteOffset + buf.length));
  let peak = 0, sum = 0, sq = 0;
  for (const s of data) { const v = s / 32767; peak = Math.max(peak, Math.abs(v)); sum += v; sq += v * v; }
  const n = data.length || 1;
  console.log(`${file.padEnd(20)} ${(data.length / rate).toFixed(3).padStart(7)}  ${db(peak).padStart(9)}  ${db(Math.sqrt(sq / n)).padStart(8)}  ${(sum / n).toFixed(4).padStart(7)}  ${String(data[0]).padStart(5)}  ${String(data[data.length - 1]).padStart(4)}`);
}

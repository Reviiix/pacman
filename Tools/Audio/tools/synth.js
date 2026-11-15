#!/usr/bin/env node
// Renders the game's sound effects from sounds.json into 16-bit mono WAV files, with no packages.
// Each sound mixes voices; a voice is an oscillator (zero-mean square with a duty cycle, triangle or noise) playing a list of notes,
// each with its own length, optional pitch slide and an attack/decay/sustain/release envelope. The mix is normalised to
// the sound's peak level, edged with short fades so it never clicks, and trimmed of trailing silence.
// Usage: node tools/synth.js sounds.json out
const fs = require('fs');
const path = require('path');

const [specFile = 'sounds.json', outDir = 'out'] = process.argv.slice(2);
const spec = JSON.parse(fs.readFileSync(specFile, 'utf8'));
const rate = spec.sampleRate || 22050;
const NOTE = { C: 0, D: 2, E: 4, F: 5, G: 7, A: 9, B: 11 };

/** Converts a note name such as "C#5" or a number of hertz to hertz. */
function hz(note) {
  if (typeof note === 'number') return note;
  const m = /^([A-G])(#|b)?(-?\d)$/.exec(note);
  if (!m) throw new Error(`bad note ${note}`);
  const midi = 12 * (parseInt(m[3], 10) + 1) + NOTE[m[1]] + (m[2] === '#' ? 1 : m[2] === 'b' ? -1 : 0);
  return 440 * Math.pow(2, (midi - 69) / 12);
}

/** PolyBLEP correction that softens the aliasing of a hard waveform edge. */
function polyBlep(t, dt) {
  if (t < dt) { t /= dt; return t + t - t * t - 1; }
  if (t > 1 - dt) { t = (t - 1) / dt; return t * t + t + t + 1; }
  return 0;
}

/** Linear attack/decay/sustain/release envelope for a note of the given length, all in seconds. */
function envelope(time, length, env) {
  const { a = 0.002, d = 0, s = 1, r = 0.01 } = env || {};
  let level;
  if (time < a) level = time / a;
  else if (time < a + d) level = 1 - (1 - s) * ((time - a) / d);
  else level = s;
  const releaseStart = length - r;
  if (time > releaseStart) level *= Math.max(0, (length - time) / r);
  return level;
}

/** Renders one voice into a float buffer. */
function renderVoice(voice, seed) {
  const notes = voice.notes;
  const total = notes.reduce((sum, n) => sum + n.len + (n.gap || 0), 0) + (voice.delay || 0);
  const out = new Float32Array(Math.ceil(total * rate));
  let cursor = Math.round((voice.delay || 0) * rate);
  let phase = 0;
  let noise = seed || 0x5eed;
  let noiseValue = 0;
  let noisePhase = 0;
  for (const n of notes) {
    const count = Math.round(n.len * rate);
    if (n.note !== null && n.note !== undefined) {
      const f0 = hz(n.note), f1 = n.to !== undefined ? hz(n.to) : f0;
      for (let i = 0; i < count && cursor + i < out.length; i++) {
        const t = i / rate;
        const f = f0 * Math.pow(f1 / f0, i / count);
        const dt = f / rate;
        let v;
        if (voice.wave === 'square') {
          const duty = voice.duty || 0.5;
          v = (phase < duty ? 1 : -1) + polyBlep(phase, dt) - polyBlep((phase + 1 - duty) % 1, dt) - (2 * duty - 1);
        } else if (voice.wave === 'triangle') {
          v = 1 - 4 * Math.abs(phase - 0.5);
        } else {
          noisePhase += dt;
          if (noisePhase >= 1) {
            noisePhase -= 1;
            noise ^= noise << 13; noise ^= noise >>> 17; noise ^= noise << 5; noise >>>= 0;
            noiseValue = (noise / 0xffffffff) * 2 - 1;
          }
          v = noiseValue;
        }
        phase = (phase + dt) % 1;
        out[cursor + i] += v * envelope(t, n.len, n.env || voice.env) * (n.vol !== undefined ? n.vol : 1);
      }
    }
    cursor += count + Math.round((n.gap || 0) * rate);
  }
  return out;
}

/** Filters in place with a 12 dB-per-octave Butterworth low-pass (RBJ biquad), which is 3 dB down at the cut-off. */
function lowpass(buf, cutoff) {
  const w0 = 2 * Math.PI * Math.min(cutoff, rate * 0.49) / rate, alpha = Math.sin(w0) / (2 * Math.SQRT1_2), cos = Math.cos(w0);
  const a0 = 1 + alpha, b0 = (1 - cos) / 2 / a0, b1 = (1 - cos) / a0, b2 = b0, a1 = -2 * cos / a0, a2 = (1 - alpha) / a0;
  let x1 = 0, x2 = 0, y1 = 0, y2 = 0;
  for (let i = 0; i < buf.length; i++) {
    const x = buf[i], y = b0 * x + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
    x2 = x1; x1 = x; y2 = y1; y1 = y; buf[i] = y;
  }
}

/** Mixes a sound's voices, low-passes, normalises to its peak, adds edge fades and trims silence. */
function renderSound(sound) {
  const voices = sound.voices.map((v, i) => ({ buf: renderVoice(v, 0x9e3779b9 ^ (i + 1)), vol: v.vol !== undefined ? v.vol : 1 }));
  const length = Math.max(...voices.map(v => v.buf.length));
  const mix = new Float32Array(length);
  for (const v of voices) for (let i = 0; i < v.buf.length; i++) mix[i] += v.buf[i] * v.vol;
  if (sound.lowpass) lowpass(mix, sound.lowpass);
  let mean = 0;
  for (let i = 0; i < length; i++) mean += mix[i];
  mean /= length || 1;
  let peak = 0;
  for (let i = 0; i < length; i++) { mix[i] -= mean; peak = Math.max(peak, Math.abs(mix[i])); }
  const target = Math.pow(10, (sound.peakDb !== undefined ? sound.peakDb : -6) / 20);
  const gain = peak > 0 ? target / peak : 0;
  let end = length;
  while (end > 1 && Math.abs(mix[end - 1] * gain) < 1e-4) end--;
  const fade = Math.min(Math.round(0.003 * rate), Math.floor(end / 4));
  const pcm = new Int16Array(end);
  for (let i = 0; i < end; i++) {
    let edge = 1;
    if (i < fade) edge = i / fade;
    else if (i >= end - fade) edge = (end - 1 - i) / fade;
    pcm[i] = Math.max(-32767, Math.min(32767, Math.round(mix[i] * gain * edge * 32767)));
  }
  return pcm;
}

/** Wraps 16-bit mono samples in a WAV file. */
function wav(pcm) {
  const data = Buffer.from(pcm.buffer, pcm.byteOffset, pcm.byteLength);
  const header = Buffer.alloc(44);
  header.write('RIFF', 0); header.writeUInt32LE(36 + data.length, 4); header.write('WAVE', 8);
  header.write('fmt ', 12); header.writeUInt32LE(16, 16); header.writeUInt16LE(1, 20); header.writeUInt16LE(1, 22);
  header.writeUInt32LE(rate, 24); header.writeUInt32LE(rate * 2, 28); header.writeUInt16LE(2, 32); header.writeUInt16LE(16, 34);
  header.write('data', 36); header.writeUInt32LE(data.length, 40);
  return Buffer.concat([header, data]);
}

fs.mkdirSync(outDir, { recursive: true });
for (const [name, sound] of Object.entries(spec.sounds)) {
  const pcm = renderSound(sound);
  fs.writeFileSync(path.join(outDir, `${name}.wav`), wav(pcm));
  console.log(`${name}.wav ${(pcm.length / rate).toFixed(3)} s`);
}

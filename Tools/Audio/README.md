# Sound effects source

Source and generator for the game's chiptune sound effects. Every sound is synthesised from `sounds.json`, so there are no
recordings or third-party samples. Unity ignores this folder; the imported clips live in `Assets/Audio/SFX/`.

- `sounds.json` is the source of truth. Each sound mixes voices; a voice is an oscillator (`square` with a `duty` cycle,
  `triangle` or `noise`) playing a list of notes. A note has a name (`"C#5"`) or a frequency in hertz, a length in
  seconds, an optional slide target (`to`) and an attack/decay/sustain/release envelope (`env`). Each sound sets its peak
  level (`peakDb`) and an optional low-pass cut-off (`lowpass`, in hertz) to soften the squares.
- `tools/synth.js` renders the sounds to 22.05 kHz, 16-bit mono WAVs. It removes any DC offset, normalises to the peak
  level, fades the first and last 3 ms so nothing clicks and trims trailing silence.
- `tools/check.js` reports each file's length, peak and RMS level, DC offset and edge samples.

Rebuild everything (needs only Node):

```sh
sh tools/regenerate.sh
```

Then copy the changed `out/*.wav` files into `Assets/Audio/SFX/`, keeping their names so the scene's references
survive. `SoundEffectImporter` gives new clips mono, preload, decompress-on-load and Vorbis settings.

## The sounds

| File | When it plays | Notes |
|---|---|---|
| `sfx_pellet_0` … `sfx_pellet_3` | Each pellet, in turn | A rising C–D–E–G pentatonic trickle of 50 ms chirps, kept quiet (-15 dBFS) because it repeats constantly |
| `sfx_bonus` | A bonus pickup | A bright A-major arpeggio over a triangle bass |
| `sfx_pause` / `sfx_resume` | P pauses / resumes | Three falling or rising square tones |
| `sfx_start` | The level starts | A short D-major jingle; in a browser it is only heard once the page has had a click, tap or key press |
| `sfx_win` | The last pellet is eaten | A G-major fanfare with a noise sparkle on the final chord |

All melodies are original. Keep it that way: do not copy themes or sound effects from existing games.

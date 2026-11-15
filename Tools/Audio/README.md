# Sound effects source

Source and generator for the game's chiptune sound effects. Every sound is synthesised from `sounds.json`, so there are no
recordings or third-party samples. Unity ignores this folder; the imported clips live in `Assets/Audio/SFX/`.

- `sounds.json` is the source of truth (`sampleRate` sets the output rate). Each sound mixes voices; a voice is an
  oscillator (`square` with a `duty` cycle, `triangle` or `noise`) playing a list of notes, with an optional mix level
  (`vol`), start offset in seconds (`delay`) and default envelope for its notes (`env`). A note has a name (`"C#5"`) or a
  frequency in hertz, a length in seconds (`len`), an optional slide target (`to`), its own envelope (`env`, an
  attack/decay/sustain/release), level (`vol`) and silence after it (`gap`); a `null` note is a rest. Each sound sets its
  peak level (`peakDb`) and an optional low-pass cut-off (`lowpass`, in hertz, 3 dB down there) to soften the squares.
- `tools/synth.js` renders the sounds to 16-bit mono WAVs. In order it mixes the voices (squares are zero-mean, so their
  envelopes never thump), applies the low-pass, removes the remaining DC offset, trims trailing silence and normalises to
  the peak level, fading the first and last 3 ms so nothing clicks.
- `tools/check.js` reports each file's length, peak and RMS level, DC offset and edge samples.

Rebuild everything (needs only Node):

```sh
sh tools/regenerate.sh
```

Then copy the changed `out/*.wav` files into `Assets/Audio/SFX/`, keeping their names so the scene's references
survive. `SoundEffectImporter` gives new clips mono, preload, decompress-on-load and Vorbis settings (web builds re-encode
clips for the browser, which decodes them itself).

## The sounds

| File | When it plays | Notes |
|---|---|---|
| `sfx_pellet_0` … `sfx_pellet_3` | Each pellet, in turn | A rising C–D–E–G pentatonic trickle of 50 ms chirps, kept quiet (-15 dBFS) because it repeats constantly |
| `sfx_bonus` | A bonus pickup | A bright A-major arpeggio over a triangle bass |
| `sfx_pause` / `sfx_resume` | P pauses / resumes | Three falling or rising square tones |
| `sfx_start` | The level starts | A short D-major jingle. A browser only lets audio play after a click, tap or key press in the page, so `SoundEffects` holds the jingle until then and skips it if that takes over four seconds |
| `sfx_win` | The last pellet is eaten | A G-major fanfare with a noise sparkle on the final chord |

All melodies are original. Keep it that way: do not copy themes or sound effects from existing games.

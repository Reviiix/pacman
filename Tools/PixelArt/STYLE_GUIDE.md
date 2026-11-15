# Cobalt Cabinet: production style guide

An early-1980s arcade cabinet screen in blue. Solid cobalt bars, bevelled from the top-left with an ice edge, an azure band
and a navy shade, sit on a calm near-black night. Small white-core pellets glow azure, and the score bar is ice and white.
Mallow the white cat, in a red neckerchief, collects the pellets while four flat, chunky creatures hunt her. Every character
is drawn like a hardware sprite: no outlines and at most three colours. Every shape and colour is original.

This guide describes the art and how it is wired into Unity. The source of truth is `art.txt`; everything else is generated
from it. The generated files below are not committed: run `sh tools/regenerate.sh` to produce them.

| File | What it is |
|---|---|
| `art.txt` | Editable ASCII source: palette, every sprite frame, wall quadrant patterns |
| `spec.json` | Built from `art.txt` (`node tools/build.js art.txt spec.json`) |
| `out/*.png` (generated) | Production PNG strips (frames left to right) and `walls.png` (47-tile autotile sheet) |
| `contact_sheet.png` (generated) | Every frame at 6x with a 2x copy (the real in-game size), all 47 wall tiles with indices, palette |
| `mockup.png` / `mockup_alt_frames.png` (generated) | The real level at 3x, frame 0 and frame 1 of every animated sprite |
| `review/canvas_576x784_x2.png` (generated) | The real WebGL canvas (288x392 reference at 2x, 8 px margins) |
| `review/stress_walls_x2.png` (generated) | Wall stress test: pillars, checkerboards, rings, masses, crosses, concave corners |
| `review/metrics.txt` (generated) | Contrast, CIEDE2000 and colour-blind separation between actors, ghost-colour echo check |
| `tools/` | Node toolchain with no dependencies (see [Art pipeline](#14-art-pipeline)) |

---

## 1. Ground rules

- **Grid:** every sprite frame is 16x16 art pixels, one maze cell. The level is 17x22 cells (272x352). The canvas reference is
  288x392, shown at whole-number scales only (2x on the 576x784 WebGL canvas).
- **Pixels:** palette colours only, all fully opaque. No anti-aliasing, no dithering, no semi-transparent pixels. Alpha
  fades happen in code (FX), never in the art.
- **No outlines:** every character and pickup is a flat, solid silhouette with no ink outline, like a hardware sprite. Ink
  (`K`) is almost the background colour, so it is used only inside a body: pupils, the hero's eyes and nose, mouths, Jitters'
  face, Swirlpop's swirl groove and melon seeds. A pupil is always enclosed by white or body colour, so it never reads as
  a notch against the background.
- **Colours per sprite:** a character uses at most 3 colours: one body colour plus white and/or ink details (the hero is
  white with a red neckerchief and an ink face, and her back view uses only 2). A pickup uses at most 4.
- **Chunky strokes:** legs, tails and sticks are 2 px thick. The exceptions are Peeper's jointed legs and silk thread,
  which are 1 px, the hero's tail, which tapers to a 1 px tip, and the fruit stems.
- **Light:** comes from the top-left everywhere. Highlights (`W`, `L`) go on top and left edges, shading on the bottom and
  right (`E` on walls, `S` on the pellet glow).
- **White** is a large fill only on the hero, so the player is always the brightest figure. Enemies use white only for
  small details (at most 18 px a frame): eyes, fangs, Peeper's thread, Jitters' gloss corner and sweat drop, and Swirlpop's
  eyes and stick. Pickups use it only for shines, sticks, pith and rind lines and the potion's glass.
- **Faces:** every enemy has a face (white eyes with ink pupils, except Jitters' squeezed-shut ink `><`). No pickup has eyes
  or a face, so creatures are never mistaken for pickups.
- **One hue per actor:** each enemy owns one hue that no other actor uses as a body colour.
- **Originality (hard rule):** no yellow disc or wedge-mouthed hero. No dome-headed or wavy-skirted ghosts. No
  red/pink/cyan/orange enemy set, no blue "scared" enemy, no zigzag "frightened" mouth. No thin double-line maze outline:
  walls are filled bars. No copied arcade fruit layouts (for example a cherry pair on a V stem). No READY! banner styling.
  Every sprite is an original drawing.

## 2. Palette (16 colours, all used)

| Key | Hex | Name | Used for | Contrast vs `N` |
|---|---|---|---|---|
| `N` | `#060a1c` | Night | Background, corridors, camera clear colour. Never drawn inside sprites | 1.0:1 |
| `J` | `#173596` | Cobalt | Wall core, always flat | 1.9:1 |
| `S` | `#4378f0` | Azure | Wall bevel band, pellet glow (shaded side), HUD digit shadow, points popup shadow, burst sparks | 4.9:1 |
| `L` | `#b8e4ff` | Ice | Wall lit edge, pellet glint, HUD label, burst rays and centre, potion brew | 14.6:1 |
| `D` | `#2756d8` | Deep blue | HUD label shadow only | 3.2:1 |
| `E` | `#0c1a52` | Navy | Wall shade, 2 px on south and east faces | 1.2:1 |
| `W` | `#ffffff` | White | Hero, eyes, fangs, thread, sticks, shines, wall corner glint, pellet core and twinkle, burst core, HUD digits, points popup | 19.7:1 |
| `K` | `#020410` | Ink | Pupils, the hero's eyes and nose, Fangbat's mouth, Jitters' face, Swirlpop's swirl groove, melon seeds, debug-target rim, vignette tint. Never an outline | 1.0:1 |
| `V` | `#c294ff` | Violet | Fangbat (Chase) only | 8.4:1 |
| `G` | `#bcf43a` | Lime | Peeper (CutOff) only | 15.1:1 |
| `M` | `#da609d` | Raspberry | Jitters (Scared) only | 5.8:1 |
| `T` | `#1fa858` | Jade | Swirlpop (Random); also pickup stems, leaves and melon rind | 6.4:1 |
| `R` | `#ff3b3b` | Red | Hero neckerchief, cherry, strawberry, apple, melon flesh, bell bow | 5.6:1 |
| `O` | `#ff8a1f` | Orange | Orange rind, toffee caramel, bell rim and clapper, key shading, potion cork | 8.3:1 |
| `A` | `#ffb347` | Tangerine | Orange flesh | 11.0:1 |
| `Y` | `#ffcf3a` | Gold | Bell, key, strawberry seeds, caramel highlight | 13.3:1 |

`M` keeps a hue of exactly 330 degrees (`B = (R + G) / 2`), the same 30 degrees from arcade red and arcade pink, and is
desaturated to read raspberry rather than hot pink. Keep `T` at a hue of 145 degrees or less, never towards teal. Do not add
a 17th colour (16 is the format's limit); change a slot only after updating this table and re-running `tools/check.js`.

## 3. Background, corridors and layout

- **Background and corridors:** one flat colour, `N #060a1c`, over the whole screen, with no corridor texture or tint.
  Pellets and characters always sit on the darkest, calmest value.
- **Canvas (288x392 reference):** 8 px margin at the top, then the 272x24 HUD band, then the 272x352 maze, then an 8 px
  margin at the bottom. The left and right margins are 8 px each, all in `N`.
- **Unity:** set the camera clear colour to `#060a1c`, and set `MazeSkin.corridorColor` to fully transparent so the
  background shows through.

## 4. Walls: solid cobalt bars lit from the top-left

Run `sh tools/regenerate.sh`, then open `contact_sheet.png` to see all 47 numbered wall tiles.

- **Autotile:** Blob47, the 8-neighbour mask that `MazeSkin` supports. The sheet holds 47 tiles in ascending
  reduced-mask order (N=1, NE=2, E=4, SE=8, S=16, SW=32, W=64, NW=128), the same order as `MazeSkin.BuildBlobLookup`.
  `walls.png` (752x16) is sliced by the importer into `walls_0` … `walls_46`, which go into `MazeSkin.wallTiles` in that
  order.
- **Wall mode `corners`:** the patterns are 8x8 quadrants authored as the north-east quadrant, with one set per tile corner.
  That lets north- and west-facing edges be lit while south- and east-facing edges sit in shade. `tools/render.js` renders
  this mode.
- **Bands** (from the exposed edge inwards):
  - Lit faces (north, west): `L` ice 1 px, `S` azure 1 px, then the core.
  - Shade faces (south, east): `E` navy 2 px, then the core.
  - Core: flat `J`, with no specks or texture, so large wall masses stay calm.
- **Shape:** wall bodies are inset 2 px from every exposed cell edge. Corridors read 20 px wide and single walls 12 px
  thick (1 px ice, 1 px azure, 8 px core, 2 px navy). Convex corners are rounded (radius 3 px), with a mitre where a lit
  face meets a shade face. Concave corners are tight: both bands wrap the corner, so no core pixel shows through.
- **Glint:** one `W` pixel on each north-west (light-facing) convex corner.
- **Border:** cells outside the grid count as wall (`MazeSkin.treatOutsideAsWall = true`, `outOfBoundsIsWall: true` in
  mockups). The border becomes one solid band with only its inner edge drawn: no double outline. The playfield reads as a
  tray recessed into the frame.
- **Generator:** `node tools/walls.js LS EE J 3 2 nw:outer:4:2:W nw:inner:4:3:S ne:inner:4:3:S sw:inner:4:3:S se:inner:4:3:E`
  (lit bands, shade bands, core, radius, inset, then single-pixel overrides). `nw:outer:4:2:W` is the corner glint. The
  four `inner:4:3` overrides keep the concave corners tight: `walls.js` rounds inner corners, which drops the second band's
  corner pixel. Without them, every lit concave corner (inside rings, the ghost-house U, the lit side of T-junctions and
  stubs) shows a cobalt bite in its azure band. The overrides restore `S` at every concave corner that touches a lit face
  (the lit face wins the mitre) and `E` where both faces are shaded. The command is recorded in `art.txt`. Never
  hand-edit the `@walls` block or `walls.png`. Change the command, paste its output over the block, rebuild, then check
  the 47-tile grid on the contact sheet and `review/stress_walls_x2.png`. If you change the number of bands, move or drop
  the overrides to match.
- **Keep the azure band 1 px:** Fangbat's violet stays clear of it for red-green colour-blind players (10.9 dE in protan
  and 16.8 in deutan vision, with a 1.73 luminance ratio). A 2 px azure band would camouflage the Chase enemy there.
- **Level design:** any layout works. Pillars, checkerboards, rings, thick masses, crosses and concave corners are all
  verified on the stress map. Diagonal-only neighbours deliberately do not connect.
- **Contrast:**

  | Band | vs corridor |
  |---|---|
  | Lit edge (`L`) | 14.6:1 |
  | Azure band (`S`) | 4.9:1 |
  | Navy shade (`E`) | 1.2:1 (1.55:1 against the core) |
  | Core (`J`) | 1.9:1 |

  The maze reads bright at its top-left edges, the navy shade gives every bar a raised bevel on its bottom-right, and the
  dark core keeps the border and side-tunnel masses calm across the roughly 54% of the screen the walls cover.

## 5. The hero: Mallow, the white cat

| File | View | Frame 0 | Frame 1 |
|---|---|---|---|
| `hero_side.png` | Side view facing right (mirrored in code for left) | Stride: front and back legs apart | Passing: whole body 1 px higher, legs straight under hips and shoulders, tail flicks back |
| `hero_up.png` | Back view (walking up) | Left hind paw down | Body bobs 1 px, right paw down |
| `hero_down.png` | Front view (walking down) | Left front paw down | Body bobs 1 px, right paw down |

- **Look:** a flat white silhouette with no outline or shading, in 3 colours: `W` body, `R` neckerchief and `K` eyes and
  nose. Two pointed ears, 1x2 ink eyes (set towards the front in the side view) and an ink nose. A 2 px tail, tapering to
  a 1 px tip, curls up behind her. Legs are 2 px wide, with feet on row 14 in every frame, so she never floats.
- **Neckerchief:** in the side view the red band wraps the neck and its point hangs over the chest (2 rows deep). In the
  front and back views the band is exactly as wide as the body (10 px), so the cheeks overhang it by 1 px, which leaves a
  small neck notch on each side. The front view shows the band with its loose end hanging just left of centre and the tail
  tip curling up on the right, clear of the body. The back view has no face; it shows the knot with two tails spreading
  down the back and the tail curling up on the right round one small gap, so the back view never reads as the front.
- **Reads as:** the brightest and largest figure on screen (134 opaque px, 19.7:1, a luminous mass of 117-128 in every
  view against at most 73 for any enemy). It shares no shape with any enemy: not round, no mouth wedge, no yellow.
- **Animation:** 8 fps (0.125 s per frame), advancing only while moving: the Player's `CharacterAnimator` has
  `animateOnlyWhileMoving` on, so she holds frame 0 when a wall blocks her.
- **Facing:**
  - Left uses the side frames mirrored (`scale.x = -1`).
  - Up and Down use `hero_up` / `hero_down` through `CharacterAnimator.upFrames` / `downFrames`.

## 6. Enemies

All four are front-facing, 2-frame idle loops that run continuously, each drawn in one body hue with white and ink details.
Every body is centred on the tile's middle line, so the animator's left-mirroring never shifts it. Only Peeper's
frame-1 pupil and thread and Swirlpop's swirl and pupils change sides. Jitters is never mirrored, so its gloss stays
top-left (see section 12). They need no up or down frames.

| Behaviour (code) | Character | File | Colour | Silhouette and behaviour cue | Frame 0 / frame 1 | fps |
|---|---|---|---|---|---|---|
| **Chase**: heads straight for the player | Fangbat | `enemy_chase.png` | `V` violet | A bat with broad membrane wings, scalloped trailing edges, small close-set ears, glaring white eyes and two fangs: a direct hunter, the heaviest wingspan on screen | Wings spread / wings beat up into a V. Only the wings move (42 px, all silhouette) | 8 |
| **CutOff**: aims 3 tiles ahead | Peeper | `enemy_cutoff.png` | `G` lime | A round spider hanging on a white silk thread from the top edge of the tile, one big 4x4 eye, three 1 px jointed legs a side: a trap-setter that drops in ahead of you | Hanging with legs reaching, pupil centred / body drops 1 px, legs dangle, pupil glances ahead to the eye's front edge (follows the mirror). The eye keeps its 4x4 shape in both frames; only the pupil moves | 6 |
| **Scared**: chases until within 9 tiles, then retreats | Jitters | `enemy_scared.png` | `M` raspberry | Jelly cube, the only square silhouette. Squeezed-shut `><` eyes and a frown in 2 px ink strokes (never a zigzag), a white gloss corner and a sweat drop: nervous | Tall (10x12) / squashed wide (12x10, about the same area) while the sweat drop flies up | 5 |
| **Random**: wanders at random | Swirlpop | `enemy_random.png` | `T` jade | A round lolly on a white stick with an ink swirl groove and two white googly eyes peeking over its rim: dizzy | Eyes look outwards / bobs 1 px down its stick, the swirl turns exactly 180 degrees and the eyes cross | 6 |

Rules for new enemy art:

- Keep each enemy inside its own hue, flat and outline-free, in at most 3 colours.
- Keep eyes white with ink pupils, each pupil enclosed by white or body colour.
- Keep the body centred on the tile so mirroring never makes it hop.
- Keep every enemy at least 15 CIEDE2000 from every other actor in normal vision and in the deutan, protan and tritan
  simulations, or give it a clearly different silhouette. Run `tools/check.js`.
- No enemy may sit within 30 degrees of hue of its original arcade counterpart on the same behaviour role (the check
  prints this).

## 7. Pellets: glowing dots

- `pellet.png`: a 16x16 frame with a 4x4 dot centred in it (columns 6-9, rows 6-9), rounded at the corners. A 2x2 `W` core,
  a 3 px ice (`L`) glint on its top-left and a 5 px azure (`S`) glow round its shaded lower and right sides: 12 bright
  pixels, with the core at 19.7:1 and the glint at 14.6:1 on the corridor.
- **Frame 1 (twinkle):** the whole dot flashes white (12 `W`) in the same 4x4 footprint. The azure-to-white jump is clearly
  visible at 2x, and the dot is never larger than 4x4, so it never reads as a power pellet (the game has none).
- **Twinkle behaviour (sparse):** one `PelletTwinkle` driver (not 142 animators) lights about 1 in 12 uneaten pellets (at
  least 1) on frame 1 every 0.25 s tick, re-picking at random and preferring pellets that were not lit last tick. Pellets
  live on their own sub-canvas (`GRP_Pickups`), so twinkles never rebuild the maze.

## 8. Bonus pickups

Static and faceless, drawn from scratch as flat shapes with no outline. Each is 13-16 px in its longest dimension (the
orange slice is 16x8 and the key 16x6), at most 4 colours, and centred on the cell. The even-height pickups span rows
1-14 (cherry, strawberry, apple, bell), 4-11 (orange) and 5-10 (key), and the two 13 px tall ones (melon, potion) sit
within half a pixel of the centre. The order matches `BonusItem.BonusItemType`, and so
`ScoreManager.bonusPickUpSprites[0..7]`.

| # | Enum | Points | File | Design |
|---|---|---|---|---|
| 0 | `Cherry` | 10 | `pickup_cherry.png` | One plump red cherry (a 10x8 body) with a white shine on its upper left, on a curved jade stem with a leaf. A single cherry, never the arcade pair |
| 1 | `Strawberry` | 20 | `pickup_strawberry.png` | Red strawberry with gold seeds, one white shine and a jade leaf cap |
| 2 | `Orange` | 30 | `pickup_orange.png` | Orange half-slice: orange rind, tangerine flesh, white pith lines fanning from the centre |
| 3 | `Apple` | 40 | `pickup_apple.png` | Toffee apple: red apple under an orange caramel cap with drips and a gold highlight, a white stick and a white shine. No ink inside the body, so it never looks like a face |
| 4 | `Melon` | 50 | `pickup_melon.png` | Watermelon wedge: red flesh with ink seeds, a white rind line and jade rind |
| 5 | `Potion` | 60 | `pickup_potion.png` | Round flask of ice-blue brew with a white glass neck and highlights, and an orange cork |
| 6 | `Bell` | 70 | `pickup_bell.png` | Gold bell with a white shine, an orange rim and clapper, tied with a red bow |
| 7 | `Key` | 80 | `pickup_key.png` | Horizontal gold key: round bow with a white shine, orange-shaded shaft, two teeth |

Pickups sit on the bonus cell, here (8, 8). An optional 1 px bob at 2 Hz is allowed. Avoid making any pickup's main
colour an enemy hue (`V`, `G`, `M`, `T`).

## 9. HUD

`tools/regenerate.sh` writes a mockup of the HUD to `out/hud.png`.

- **Layout:** a 272x24 band at the top of the canvas, on the same `N` background, with no panel or frame. The content is
  centred as one group: twinkle, 8 px gap, `SCORE`, 8 px gap, six digits, 8 px gap, twinkle.
- **Label `SCORE`:** ice `L #b8e4ff`, drop shadow deep blue `D #2756d8` (3.2:1 against the background, so the shadow shows).
- **Score:** six digits, zero-padded, white `W #ffffff`, drop shadow azure `S #4378f0`. Digits sit in 7 px monospace
  cells so the readout never shifts when a digit changes width.
- **Shadows:** offset exactly 1 art pixel straight down, with no blur or softness.
- **Type:** the project's TTF (`Assets/Fonts/PixelFont`), regular weight (never Bold, which dilates the SDF strokes), at
  size 9.2647 (`7 x 90 / 68`, from the font's cap line), so the cap height is 7 art px. The font's outlines are not drawn on
  an exact pixel grid, so some glyph edges are slightly soft; a true bitmap pixel font would be fully crisp.
- **Twinkles:** two 16x16 Images using `pellet_0` (the white-core dot with its azure glow), vertically centred on the
  glyphs.
- **Banners:** none (no READY!). If a win message is added later, use the same `L`-plus-`D` and `W`-plus-`S` pairs,
  centred over the middle corridor, never yellow text.
- **In Unity:** `TXT_ScoreLabel` and `TXT_ScoreLabelShadow` are static texts. `TXT_ScoreValue` carries `ScoreDisplay`,
  which writes the digits into it and into `TXT_ScoreValueShadow` without allocating.

## 10. FX and overlays

All effects are sprites or text, with no shaders, post-processing or particles.

| Effect | Spec |
|---|---|
| Pellet burst (pellet eaten) | `fx_sugar_burst.png`, 2 frames, both centred on the pellet's 4x4 footprint (rows and columns 6-9): a rounded 4x4 white core, the twinkle's shape, with 8 ice rays (four 2 px bars and four diagonal sparks, 6x6 in all), then a pinwheel of 8 azure sparks with 90-degree symmetry (10x10) round a 2x2 ice centre. Frame 0 shows for 0.06 s and frame 1 for 0.09 s, while alpha fades from 1 to 0 over the 0.15 s. `PickupEffects` pools instances of the `FX_SugarBurst` prefab |
| Bonus pickup | The same burst on the pickup's cell, plus a `+NN` popup (the `FX_PointsPopup` prefab, pooled): the TTF in `W #ffffff` with a 1 px `S #4378f0` shadow, rising 8 art px a whole pixel at a time over 0.6 s and fading over the last 0.2 s |
| Scanlines | `ScanlineOverlay` on `FX_Scanlines`, darkness **0.15**. It darkens the bottom screen row of every art pixel (phased to the centre-anchored art grid) and hides itself at 1x |
| Vignette | `VignetteOverlay` on `FX_Vignette`: alpha **0.25** of `K #020410` at the corners, smoothstep from 45% of the half-diagonal outwards, over the maze rect (272x352) only. The 68x88 gradient is generated at runtime (bilinear), never a PNG under `Assets/Art/Sprites`, where the importer would point-filter it and the gradient would band |

Both overlay RawImages are saved fully transparent; each component turns its RawImage on once it has generated its texture,
so the scene stays visible in edit mode.

Draw order, back to front:

1. Camera clear colour `N`
2. Walls
3. Pellets
4. Bonus pickup
5. Enemies
6. Hero
7. FX
8. Vignette
9. HUD
10. Scanlines

Debug target markers sit above the enemies and show whenever `ProjectSettings.DebugSettings.DebugEnemyTargets` is on (it
is a compile-time setting, so turn it off before shipping). The look must also hold with no vignette; it does, and the
vignette only deepens the corners.

## 11. Animation timings

| Sprite | Frames | `secondsPerFrame` | Notes |
|---|---|---|---|
| `hero_side`, `hero_up`, `hero_down` | 2 each | 0.125 (8 fps) | Advance while moving; hold frame 0 when blocked; side frames mirrored for Left |
| `enemy_chase` | 2 | 0.125 (8 fps) | Wing beat (spread, then up), always looping |
| `enemy_cutoff` | 2 | 0.167 (6 fps) | Thread drop, leg swing and glance, always looping |
| `enemy_scared` | 2 | 0.2 (5 fps) | Tall and squash wobble with the sweat drop flying up, always looping |
| `enemy_random` | 2 | 0.167 (6 fps) | Bob, swirl turn and eye cross, always looping |
| `pellet` twinkle | 2 | 0.25 s tick | About 1 in 12 uneaten pellets per tick flash white, re-picked at random |
| `fx_sugar_burst` | 2 | 0.06 then 0.09, once | Alpha fades from 1 to 0 |
| `+NN` popup | text | 0.6 s | Rises 8 art px, fades over the last 0.2 s |
| Pickups, walls, debug target | 1 | – | Static (pickups may bob 1 px at 2 Hz) |

All pivots are centred, and every frame is 16x16. Frames and moving characters land on whole art pixels: `MovingObject`
moves an unrounded logical position and draws the character at it rounded to whole canvas units.

## 12. How the art is wired into Unity

- **Import:** PNGs go under `Assets/Art/Sprites/<Folder>/`. `PixelArtImporter` gives new textures Point filtering, no
  mipmaps, no compression, 16 pixels per unit and FullRect, raises Max Size on every import to the next power of two above
  the largest side (at least 32: 1024 for `walls.png`, 32 for everything else), and slices strips whose width is a whole
  multiple of their height into `<file>_<n>`, keeping sprite IDs when a strip is re-sliced.
- **Scaling and layout:** `PixelPerfectCanvasScaler` on both canvases picks the largest whole-number scale for the 288x392
  reference. Grid cells are fixed 16x16 (the layout groups do not size cells from their content).
- **Canvases:**
  - `GRP_Grid`: nested canvas for the static maze.
  - `GRP_Pickups`: nested canvas for pellets and the bonus item. `ProjectInitializer` moves them here from their cells at
    start-up, so the `Blank` tile's pickup tools work in edit mode only.
  - `GRP_Game`, the root: characters, FX, debug markers and the vignette.
  - `GRP_UserInterface`: the HUD and the scanlines.
- **Prefabs:** `Assets/Prefabs/Characters/Player`, an `Enemy` base with `Enemy_Chase`, `Enemy_CutOff`, `Enemy_Scared` and
  `Enemy_Random` variants (frames and timing only), `Assets/Prefabs/FX/FX_SugarBurst` and `FX_PointsPopup`, and the existing
  `Pellet` and `BonusPickUp` (all level pellets are instances).
- **Image colours:** every art Image is white (no tint).
- **Debug targets:** `SeekerEnemyTarget` (Chase, `#c294ff`), `CutOffEnemyTarget` (`#bcf43a`) and `ScaredEnemyTarget`
  (`#da609d`) use `debug_target` tinted with their enemy's colour. Random has no target.
- **`MazeSkin`** (`Assets/Art/MazeSkin.asset`): layout `Blob47`, `wallTiles` = `walls_0` … `walls_46` in order (use the
  **Fill Wall Tiles From Sheet** context menu after changing `walls.png`), `wallTint` white, `corridorColor` transparent,
  `treatOutsideAsWall` true. `GridManager` applies it at start-up; its **Apply Maze Skin** context menu previews it in edit
  mode.
- **`CharacterAnimator`:** the Player uses `hero_side_0/1`, `hero_up_0/1` and `hero_down_0/1` at 0.125 s with
  `animateOnlyWhileMoving`; each enemy variant uses its `sideFrames` with the timings in section 11. Side frames are
  mirrored for Left unless `mirrorWhenFacingLeft` is off, which it is only on `Enemy_Scared`: Jitters is symmetric, and
  mirroring would move its gloss to the top-right, against the top-left light rule.
- **Score:** `ScoreManager.bonusPickUpSprites` = cherry, strawberry, orange, apple, melon, potion, bell, key, in
  `BonusItemType` order.
- **Colours set in Unity** (everything else comes from the sprites): camera clear colour `#060a1c`; HUD label `#b8e4ff`
  with shadow `#2756d8`; HUD digits `#ffffff` with shadow `#4378f0`; points popup `#ffffff` with shadow `#4378f0`;
  vignette tint `#020410`; the three debug-target tints above.
- **Atlas:** no Sprite Atlas is used. Add one (Point filtering, no compression, padding 2, no rotation, no tight packing) if
  WebGL draw calls become a concern.

## 13. Naming conventions

- **Case:** lower snake_case, `<category>_<subject>[_<view>].png`. The categories are `hero`, `enemy`, `pickup`, `fx` and
  `debug`, plus the two singletons `pellet.png` and `walls.png`.
- **Enemies:** named by behaviour role to match the code (`enemy_chase`, `enemy_cutoff`, `enemy_scared`, `enemy_random`).
  Character names (Fangbat, Peeper, Jitters, Swirlpop) appear only in docs.
- **Pickups:** `pickup_` plus the lower-case `BonusItemType` name. The sixth slot is `pickup_potion`.
- **Views:** `_side` faces right, `_up` is the back view, `_down` is the front view. Never ship `_left` copies; the code
  mirrors.
- **Strips:** one strip per animation, frames left to right, 16x16, no padding or gutters. Unity slice names are
  `<file>_<index>` (`enemy_chase_0`, `walls_46`).
- **Folders:**

  | `Assets/Art/Sprites/...` | Files |
  |---|---|
  | `Characters/` | `hero_*` |
  | `Enemies/` | `enemy_*` |
  | `Pickups/` | `pellet` and `pickup_*` |
  | `Maze/` | `walls` |
  | `FX/` | `fx_*` |
  | `Debug/` | `debug_*` |

- **Importer trap:** never put a non-strip image whose width is a whole multiple of its height under `Assets/Art/Sprites`
  (a HUD divider, for example); the importer would slice it.
- **Mockup-only outputs:** `out/hud.png` and `out/_preview_x4.png` are not game assets.
- **Art source:** lives outside `Assets` in `Tools/PixelArt/` (`art.txt`, `spec.json`, `maps/`, `tools/`), so Unity ignores
  it.

## 13a. Complete sprite list

Every frame is 16x16. "Drawn" means final Cobalt Cabinet art is in `spec.json` and `out/`. File names, sizes and slice names
are unchanged from the previous look, so every Unity reference survives a copy-over.

| File | Size | Frames | Unity slices | Used by | Status |
|---|---|---|---|---|---|
| `hero_side.png` | 32x16 | 2 | `hero_side_0..1` | Player `CharacterAnimator.sideFrames` | Drawn |
| `hero_up.png` | 32x16 | 2 | `hero_up_0..1` | Player `upFrames` | Drawn |
| `hero_down.png` | 32x16 | 2 | `hero_down_0..1` | Player `downFrames` | Drawn |
| `enemy_chase.png` | 32x16 | 2 | `enemy_chase_0..1` | `Enemy_Chase` prefab `sideFrames` | Drawn |
| `enemy_cutoff.png` | 32x16 | 2 | `enemy_cutoff_0..1` | `Enemy_CutOff` prefab `sideFrames` | Drawn |
| `enemy_scared.png` | 32x16 | 2 | `enemy_scared_0..1` | `Enemy_Scared` prefab `sideFrames` | Drawn |
| `enemy_random.png` | 32x16 | 2 | `enemy_random_0..1` | `Enemy_Random` prefab `sideFrames` | Drawn |
| `pellet.png` | 32x16 | 2 | `pellet_0..1` | Pellet prefab (frame 0), twinkle driver (frame 1), HUD twinkles | Drawn |
| `pickup_cherry.png` | 16x16 | 1 | single | `bonusPickUpSprites[0]` | Drawn |
| `pickup_strawberry.png` | 16x16 | 1 | single | `bonusPickUpSprites[1]` | Drawn |
| `pickup_orange.png` | 16x16 | 1 | single | `bonusPickUpSprites[2]` | Drawn |
| `pickup_apple.png` | 16x16 | 1 | single | `bonusPickUpSprites[3]` | Drawn |
| `pickup_melon.png` | 16x16 | 1 | single | `bonusPickUpSprites[4]` | Drawn |
| `pickup_potion.png` | 16x16 | 1 | single | `bonusPickUpSprites[5]` | Drawn |
| `pickup_bell.png` | 16x16 | 1 | single | `bonusPickUpSprites[6]` | Drawn |
| `pickup_key.png` | 16x16 | 1 | single | `bonusPickUpSprites[7]` | Drawn |
| `walls.png` | 752x16 | 47 | `walls_0..46` | `MazeSkin.wallTiles` (Blob47) | Generated |
| `fx_sugar_burst.png` | 32x16 | 2 | `fx_sugar_burst_0..1` | `PickupEffects` burst frames (`FX_SugarBurst` prefab) | Drawn |
| `debug_target.png` | 16x16 | 1 | single | The three `*EnemyTarget` markers, tinted per enemy | Drawn |
| Scanline overlay tile | 1 x scale | – | – | `ScanlineOverlay` (generated at runtime, darkness 0.15) | No file needed |
| Vignette | 68x88 | – | – | `VignetteOverlay` (generated at runtime, bilinear, tint `K #020410`) | No file needed |
| HUD text, `+NN` popup | – | – | – | The project TTF with the colours in sections 9 and 10 | No sprite |

Optional extras, not needed by the current loop: a WebGL loading or favicon icon (crop `hero_down_0`) and a hero "caught"
pose (2 frames) if a lose state is added. Enemies need no frightened, eaten or up/down sprites: there are no power-ups and
they face the camera.

## 14. Art pipeline

Node only, with no packages. From `Tools/PixelArt/`:

```sh
sh tools/regenerate.sh
```

This builds `spec.json`, then writes `out/*.png`, the contact sheet, both mockups, the 2x canvas check, the wall stress test
and `review/metrics.txt`.

| Tool | Job |
|---|---|
| `tools/build.js` | `art.txt` to `spec.json`. Handles `@palette`, `@sprite`, `@frame [mirror]` and `@walls corners` with `@corner ne/nw/se/sw` blocks |
| `tools/walls.js` | Generates the four corner pattern sets from a distance field (bands, core, radius, inset, glints) |
| `tools/render.js` | Writes sprite strips, the 47-tile wall sheet (including the `corners` wall mode) and level mockups (with a vignette preview). Commands: `sprites <spec> <outDir> [scale]`, `mockup <config> <out.png> [scale]` |
| `tools/sheet.js`, `tools/hud.js`, `tools/make_mockup.js`, `tools/imgutil.js` | Review images |
| `tools/check.js` | Readability and originality metrics: contrast, CIEDE2000 separation in normal and colour-blind vision, and hue distance from the original arcade enemy colours |

**Adding or changing a sprite:**

1. Edit the `@sprite` block in `art.txt`, using palette keys only.
2. Keep the no-outline, colour-count, stroke, lighting and white rules (section 1).
3. Run `sh tools/regenerate.sh`.
4. Look at the 2x column of the contact sheet and at `review/canvas_576x784_x2.png`.
5. Read `review/metrics.txt`.
6. Copy the changed `out/*.png` into `Assets/Art/Sprites/<Folder>/`. Strip widths must stay whole multiples of 16, or the
   slice names change. After changing `walls.png`, run **Fill Wall Tiles From Sheet** on `Assets/Art/MazeSkin.asset`.

**Review checklist:**

- [ ] Reads at 2x: you can name it from the small copy alone.
- [ ] No orphan pixels (the burst's flying sparks excepted) and no enclosed 1 px pinholes; no off-palette colours, no
  outline; every pupil enclosed by white or body colour.
- [ ] At most 3 colours on a character, 4 on a pickup.
- [ ] Light from the top-left.
- [ ] Enemy separation is at least 15 dE in every simulation, or the silhouette clearly differs.
- [ ] No ghost-colour echo on its role.
- [ ] Pellet twinkle stays inside 4x4.
- [ ] All 47 wall tiles checked on the contact sheet and the stress map.
- [ ] Nothing in the originality hard rule.

## 15. Readability results (`review/metrics.txt`)

- **Separation:** every actor pair is at least 15 dE in normal vision and in the deutan, protan and tritan simulations. The
  closest pair is Fangbat vs Jitters (worst simulation 18.2, protan), and those two never spawn together. Chase, CutOff and
  Random start on one tile, and that trio is at least 23.3 apart (Peeper vs Swirlpop, protan). The tritan floor is 17.4
  (Mallow vs Peeper).
- **Ghost colours:** no echo on any behaviour role and no "near" note. Jitters' raspberry is exactly 30 degrees from both
  arcade red and arcade pink, on the orange role (65 degrees away). Swirlpop's jade is 35 degrees from cyan on the cyan role.
  The hero is white, not Pac-Man yellow.
- **Hero:** the brightest figure in every view, with a luminous mass (the sum of every pixel's relative luminance) of
  117-128 against 71-73 for Peeper, the brightest enemy. Enemies carry at most 18 white pixels a frame.
- **Animation:** every loop changes its silhouette: Mallow 50 px (side), 32-34 px (up and down); Fangbat 42 px; Peeper 75 px;
  Jitters 40 px; Swirlpop 22 px plus the swirl and eyes (72 px changed in all).
- **Contrast on the corridor:**

  | Sprite | Contrast | Note |
  |---|---|---|
  | Hero | 19.7:1 | |
  | Bat | 8.4:1 | 8.2:1 in protan |
  | Spider | 15.1:1 | |
  | Jelly | 5.8:1 | 4.8:1 in protan, the lowest of any enemy in any simulation |
  | Lolly | 6.4:1 | Its white eyes and stick are 19.7:1 |
  | Pellet | 19.7:1 core, 14.6:1 glint, 4.9:1 glow | 12 bright px, 48 screen px at 2x; 43.4 dE from the hero |

## 16. Known limits

- **HUD font:** the project TTF's outlines are not on an exact pixel grid, so some HUD and popup glyph edges are slightly
  soft. A true bitmap pixel font (with a raster, point-filtered TMP atlas) would make them fully crisp.
- **Debug markers:** shown whenever `ProjectSettings.DebugSettings.DebugEnemyTargets` is on, including in release builds.
- **Spawn:** Chase, CutOff and Random start on the same tile and travel together for the first few seconds, so they overlap
  until their targets diverge. With no outlines, overlapping sprites meet edge to edge (the white hero over the lime spider
  is only 1.30:1 at the edge). If the clump reads muddy in Unity, a 1 px `K` outline on those three enemies is the fallback:
  `K` is 1.04:1 on the corridor, so it shows only where sprites overlap, at the cost of about a quarter of each body.
- **Peeper's thread:** mirroring moves Peeper's 1 px thread between columns 7 and 8 on each turn, which is acceptable.
- **Atlas:** none yet (see section 12).
- **Testing scene:** `PacmanTesting.unity` uses the new maze but keeps its original placeholder characters, and it cannot enter Play mode: it predates the current managers (it has no PelletManager or ScoreManager, and stale enemy data). Open it on its own rather than alongside `Pacman.unity`.

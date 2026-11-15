# Candy Haunt: production style guide

A haunted sweet shop at midnight. Glossy strawberry-jelly walls, lit from the top-left, sit on a deep grape-navy night.
Mallow the marshmallow cat collects sugar sparkles while four candy creatures hunt her. At a glance it reads as a dark-screen
1980s maze chaser (bright maze edges, small glowing dots, chunky characters, centred score bar). Every shape and colour is
original.

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
- **Outline:** every character and pickup has a 1 px ink (`K`) outline. `K` is almost the background colour, so silhouettes
  read from their fills; the outline only crisps edges against walls. Walls have no outline.
- **Light:** comes from the top-left everywhere. Highlights (`W`, `L`) go on top and left edges, shading (`S`, `D`, `E`) on
  the bottom, right and underside.
- **White** is a large fill only on the hero, so the player is always the brightest figure. Enemies use white only for
  small details: eyes, fangs, Peeper's thread, Jitters' gloss corner and sweat drop, and Swirlpop's paper stick. Pickups use
  it only for highlights and sugar.
- **Faces:** every enemy has a face (white eyes with ink pupils, except Jitters' squeezed-shut ink `><` and Peeper's tiny
  ink side eyes). No pickup has eyes or a face, so candy enemies are never mistaken for candy pickups.
- **One hue per actor:** each enemy owns one hue that no other actor uses as a body colour.
- **Originality (hard rule):** no yellow disc or wedge-mouthed hero. No dome-headed or wavy-skirted ghosts. No
  red/pink/cyan/orange enemy set, no blue "scared" enemy, no zigzag "frightened" mouth. No thin double-line maze outline. No
  arcade fruit layouts (for example a cherry pair on a V stem). No READY! banner styling.

## 2. Palette (16 colours, all used)

| Key | Hex | Name | Used for | Contrast vs `N` |
|---|---|---|---|---|
| `K` | `#140b26` | Ink | 1 px outline and pupils on every sprite | 1.1:1 |
| `N` | `#1e1442` | Midnight grape | Background, corridors, camera clear colour. Never drawn inside sprites | 1.0:1 |
| `W` | `#ffffff` | Sugar white | Hero fur, eyes, fangs, thread, glints, wall corner glint, pellet core, HUD digits | 17.0:1 |
| `S` | `#b3a6ea` | Lilac | Hero shading and far legs, paper cup, lolly stick, apple stick, potion glass, HUD digit shadow | 7.8:1 |
| `L` | `#ffcbe8` | Gloss pink | Wall lit edge, hero inner ears and cheeks, HUD label | 12.1:1 |
| `P` | `#ff7cc0` | Strawberry | Wall lit band, hero scarf, Swirlpop swirl, blush, melon flesh, potion brew | 7.2:1 |
| `D` | `#a8358a` | Raspberry | Wall shade edge and inner lit band, red-fruit shading, HUD label shadow | 2.9:1 |
| `J` | `#6e2468` | Berry core | Wall interior, always flat | 1.7:1 |
| `V` | `#c4a6ff` | Lavender grape | Fangbat (Chase) only | 8.3:1 |
| `G` | `#a2f25a` | Sour apple | Peeper (CutOff), leaf accents on pickups | 12.5:1 |
| `T` | `#34d9ac` | Spearmint | Jitters (Scared), stems and leaves, melon rind | 9.5:1 |
| `B` | `#3a74f0` | Blue raspberry | Swirlpop (Random) only | 4.0:1 |
| `Y` | `#ffe55e` | Lemon | Pellet halo, bell, key, citrus highlights, sugar-burst sparks | 13.5:1 |
| `R` | `#ff3b5c` | Cherry red | Cherry, strawberry, apple, key heart, bell bow | 4.9:1 |
| `O` | `#ff9b45` | Tangerine | Orange slice, caramel, potion cork, bell shading | 8.1:1 |
| `E` | `#3a2878` | Dusk | Shared enemy underside shade (wing edges, spider belly, jelly base) | 1.4:1 |

Do not add a 17th colour without updating this table and re-running `tools/check.js`.

## 3. Background, corridors and layout

- **Background and corridors:** one flat colour, `N #1e1442`, over the whole screen, with no corridor texture or tint.
  Pellets and characters always sit on the darkest, calmest value.
- **Canvas (288x392 reference):** 8 px margin at the top, then the 272x24 HUD band, then the 272x352 maze, then an 8 px
  margin at the bottom. The left and right margins are 8 px each, all in `N`.
- **Unity:** set the camera clear colour to `#1e1442`, and set `MazeSkin.corridorColor` to fully transparent so the
  background shows through.

## 4. Walls: strawberry jelly bars lit from the top-left

Run `sh tools/regenerate.sh`, then open `contact_sheet.png` to see all 47 numbered wall tiles.

- **Autotile:** Blob47, the 8-neighbour mask that `MazeSkin` supports. The sheet holds 47 tiles in ascending
  reduced-mask order (N=1, NE=2, E=4, SE=8, S=16, SW=32, W=64, NW=128), the same order as `MazeSkin.BuildBlobLookup`.
  `walls.png` (752x16) is sliced by the importer into `walls_0` … `walls_46`, which go into `MazeSkin.wallTiles` in that
  order.
- **Wall mode `corners`:** the patterns are 8x8 quadrants authored as the north-east quadrant, with one set per tile corner.
  That lets north- and west-facing edges be lit while south- and east-facing edges sit in shade. `tools/render.js` renders
  this mode.
- **Bands** (from the exposed edge inwards):
  - Lit faces (north, west): `L` gloss 1 px, `P` strawberry 2 px, `D` raspberry 1 px, then the core.
  - Shade faces (south, east): `D` raspberry 1 px, then the core.
  - Core: flat `J`, with no specks or texture, so large wall masses stay calm.
- **Shape:** wall bodies are inset 2 px from every exposed cell edge. Corridors read 20 px wide and single walls 12 px
  thick. Convex corners are rounded (radius 4 px), with a mitre where a lit face meets a shade face. Concave corners are
  tight.
- **Glint:** one `W` pixel on each north-west (light-facing) convex corner.
- **Border:** cells outside the grid count as wall (`MazeSkin.treatOutsideAsWall = true`, `outOfBoundsIsWall: true` in
  mockups). The border becomes one solid band with only its inner edge drawn: no double outline. The playfield reads as a
  tray recessed into the frame.
- **Generator:** `node tools/walls.js LPPD D J 4 2 nw:outer:4:3:W` (lit bands, shade bands, core, radius, inset, glint). The
  command is recorded in `art.txt`. Never hand-edit `walls.png`. Change the command, rebuild, then check the 47-tile grid on
  the contact sheet and `review/stress_walls_x2.png`.
- **Level design:** any layout works. Pillars, checkerboards, rings, thick masses, crosses and concave corners are all
  verified on the stress map. Diagonal-only neighbours deliberately do not connect.
- **Contrast:**

  | Band | vs corridor |
  |---|---|
  | Lit edge (`L`) | 12:1 |
  | Strawberry band (`P`) | 7:1 |
  | Shade edge (`D`) | 2.9:1 |
  | Core (`J`) | 1.7:1 |

  The maze reads bright at its edges and stays calm across the roughly 54% of the screen it covers.

## 5. The hero: Mallow, the marshmallow cat

| File | View | Frame 0 | Frame 1 |
|---|---|---|---|
| `hero_side.png` | Side view facing right (mirrored in code for left) | Stride: near legs out, far legs (lilac) in | Passing: whole body 1 px higher, legs straight under hips and shoulders, scarf end flicks 2 px back |
| `hero_up.png` | Back view (walking up) | Left hind paw down | Body bobs 1 px, right paw down |
| `hero_down.png` | Front view (walking down) | Left front paw down | Body bobs 1 px, right paw down |

- **Look:** chunky white body (`W`, shaded `S`). Two ears with pink (`L`) inner. 1x2 bean eyes set right of centre, pink
  cheeks and a dot mouth. A strawberry (`P`) scarf whose loose end trails back. A thick 2 px tail hooked up behind. Legs are
  2 px wide and fully outlined, with feet on row 14 and the outline on row 15 in every frame, so she never floats.
- **Up and down views:** the back view shows white ears with no face, the scarf band with its knot ends hanging, and the
  tail curling up on the right. The front view shows the full face and the scarf end hanging on the screen-left side, with the
  tail peeking out on the screen-right side.
- **Reads as:** the strongest silhouette on screen (179 opaque px, 17:1). It shares no shape with any enemy: not round, no
  mouth wedge, no yellow.
- **Animation:** 8 fps (0.125 s per frame), advancing only while moving: the Player's `CharacterAnimator` has
  `animateOnlyWhileMoving` on, so she holds frame 0 when a wall blocks her.
- **Facing:**
  - Left uses the side frames mirrored (`scale.x = -1`).
  - Up and Down use `hero_up` / `hero_down` through `CharacterAnimator.upFrames` / `downFrames`.

## 6. Enemies

All four are front-facing, 2-frame idle loops that run continuously. They are mostly symmetric (Peeper's glance, Jitters'
sweat drop and Swirlpop's eyes and spiral aside), so the animator's left-mirroring is harmless and they need no up or down
frames.

| Behaviour (code) | Character | File | Colour | Silhouette and behaviour cue | Frame 0 / frame 1 | fps |
|---|---|---|---|---|---|---|
| **Chase**: heads straight for the player | Fangbat | `enemy_chase.png` | `V` lavender, `E` wing edges | Round bat, the widest silhouette. Heavy angled brows, slanted white glaring eyes, two fangs: a direct hunter | Wings raised / wings drooped | 8 |
| **CutOff**: aims 3 tiles ahead | Peeper | `enemy_cutoff.png` | `G` sour apple, `E` belly | Spider hanging on a white silk thread (top edge of the tile). One big scouting eye, two tiny side eyes, white mandibles, three chunky 2 px legs per side: a trap-setter that drops in ahead of you | Hanging / body drops 1 px, legs swing flat, pupil glances ahead (follows the mirror) | 6 |
| **Scared**: chases until within 9 tiles, then retreats | Jitters | `enemy_scared.png` | `T` spearmint, `W` gloss corner, `P` blush, `E` base | Jelly cube, the only square silhouette. Squeezed-shut `><` eyes, a small frown (never a zigzag), blush and a sweat drop: nervous | Tall / squashed wide (constant wobble) | 5 |
| **Random**: wanders at random | Swirlpop | `enemy_random.png` | `B` blue raspberry, `P` swirl | Disc on a paper stick (`W`/`S`) with two googly eyes on top. The spiral turns and the pupils point different ways: dizzy | Spiral rotated exactly 180 degrees, pupils swap | 6 |

Rules for new enemy art:

- Keep each enemy inside its own hue.
- Keep the shared `E` underside.
- Keep eyes white with ink pupils.
- Keep every enemy at least 15 CIEDE2000 from every other actor in normal vision and in the deutan, protan and tritan
  simulations, or give it a clearly different silhouette. Run `tools/check.js`.
- No enemy may sit within 30 degrees of hue of its original arcade counterpart on the same behaviour role (the check
  prints this).

## 7. Pellets: sugar sparkles

- `pellet.png`: a 16x16 frame with a 4x4 sparkle centred in it (columns 6-9, rows 6-9). A 2x2 `W` core with a lemon `Y` halo
  and rounded corners: 12 bright pixels, 13.5:1 on the corridor.
- **Frame 1 (twinkle):** the same 4x4 footprint with a brighter diagonal `W` glint (8 `W`, 4 `Y`). It is never larger than
  4x4, so it never reads as a power pellet (the game has none).
- **Twinkle behaviour (sparse):** one `PelletTwinkle` driver (not 142 animators) lights about 1 in 12 uneaten pellets (at
  least 1) on frame 1 every 0.25 s tick, re-picking at random and preferring pellets that were not lit last tick. Pellets
  live on their own sub-canvas (`GRP_Pickups`), so twinkles never rebuild the maze.

## 8. Bonus pickups

Static and faceless, drawn from scratch, each about 12-16 px tall with a 1 px outline. The order matches
`BonusItem.BonusItemType`, and so `ScoreManager.bonusPickUpSprites[0..7]`.

| # | Enum | Points | File | Design |
|---|---|---|---|---|
| 0 | `Cherry` | 10 | `pickup_cherry.png` | One glossy candied cherry (`R`, `W` highlight, `D` shade) with a mint stem and sour-apple leaf, in a pleated lilac-and-white paper cup. A single cherry, never the arcade pair |
| 1 | `Strawberry` | 20 | `pickup_strawberry.png` | Sugar-dusted strawberry: `R` body with `W` sugar specks and a `D` shade, spearmint leaf cap and stalk |
| 2 | `Orange` | 30 | `pickup_orange.png` | Jelly orange half-slice: tangerine peel, white pith, orange segments with white segment lines and lemon highlights |
| 3 | `Apple` | 40 | `pickup_apple.png` | Toffee apple: `R` apple with tangerine caramel drips, a lilac stick and a mint-and-sour-apple leaf. The drips never use ink inside the body, so it never looks like a face |
| 4 | `Melon` | 50 | `pickup_melon.png` | Watermelon wedge: strawberry flesh with ink seeds, white rind line, spearmint rind |
| 5 | `Potion` | 60 | `pickup_potion.png` | Round-bottomed flask: lilac glass, strawberry brew with white bubbles, tangerine cork, a lemon twinkle beside it |
| 6 | `Bell` | 70 | `pickup_bell.png` | Lemon candy bell with tangerine shading, a white shine, a cherry-red bow and an ink clapper |
| 7 | `Key` | 80 | `pickup_key.png` | Sweetheart key: cherry-red heart bow with a white shine, 2 px lemon and tangerine shaft, two teeth |

Pickups sit on the bonus cell, here (8, 8). An optional 1 px bob at 2 Hz is allowed. Avoid making any pickup's main
colour an enemy hue (`V`, `G`, `T`, `B`).

## 9. HUD

`tools/regenerate.sh` writes a mockup of the HUD to `out/hud.png`.

- **Layout:** a 272x24 band at the top of the canvas, on the same `N` background, with no panel or frame. The content is
  centred as one group: twinkle, 8 px gap, `SCORE`, 8 px gap, six digits, 8 px gap, twinkle.
- **Label `SCORE`:** gloss pink `L #ffcbe8`, drop shadow raspberry `D #a8358a`.
- **Score:** six digits, zero-padded, sugar white `W #ffffff`, drop shadow lilac `S #b3a6ea`. Digits sit in 7 px monospace
  cells so the readout never shifts when a digit changes width.
- **Shadows:** offset exactly 1 art pixel straight down, with no blur or softness.
- **Type:** the project's TTF (`Assets/Fonts/PixelFont`), regular weight (never Bold, which dilates the SDF strokes), at
  size 9.2647 (`7 x 90 / 68`, from the font's cap line), so the cap height is 7 art px. The font's outlines are not drawn on
  an exact pixel grid, so some glyph edges are slightly soft; a true bitmap pixel font would be fully crisp.
- **Twinkles:** two 16x16 Images using `pellet_0` (the 4x4 sparkle), vertically centred on the glyphs.
- **Banners:** none (no READY!). If a win message is added later, use the same `L`-plus-shadow and `W`-plus-shadow pairs,
  centred over the middle corridor, never yellow text.
- **In Unity:** `TXT_ScoreLabel` and `TXT_ScoreLabelShadow` are static texts. `TXT_ScoreValue` carries `ScoreDisplay`,
  which writes the digits into it and into `TXT_ScoreValueShadow` without allocating.

## 10. FX and overlays

All effects are sprites or text, with no shaders, post-processing or particles.

| Effect | Spec |
|---|---|
| Sugar burst (pellet eaten) | `fx_sugar_burst.png`, 2 frames: a white core with lemon rays, then a ring of lemon sparks. Frame 0 shows for 0.06 s and frame 1 for 0.09 s, while alpha fades from 1 to 0 over the 0.15 s. `PickupEffects` pools instances of the `FX_SugarBurst` prefab |
| Bonus pickup | The same burst on the pickup's cell, plus a `+NN` popup (the `FX_PointsPopup` prefab, pooled): the TTF in `W` with a 1 px `P` shadow, rising 8 art px a whole pixel at a time over 0.6 s and fading over the last 0.2 s |
| Scanlines | `ScanlineOverlay` on `FX_Scanlines`, darkness **0.15**. It darkens the bottom screen row of every art pixel (phased to the centre-anchored art grid) and hides itself at 1x |
| Vignette | `VignetteOverlay` on `FX_Vignette`: alpha **0.25** of `K` at the corners, smoothstep from 45% of the half-diagonal outwards, over the maze rect (272x352) only. The 68x88 gradient is generated at runtime (bilinear), never a PNG under `Assets/Art/Sprites`, where the importer would point-filter it and the gradient would band |

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
| `enemy_chase` | 2 | 0.125 (8 fps) | Wing flap, always looping |
| `enemy_cutoff` | 2 | 0.167 (6 fps) | Thread bob and glance, always looping |
| `enemy_scared` | 2 | 0.2 (5 fps) | Tall and squash wobble, always looping |
| `enemy_random` | 2 | 0.167 (6 fps) | Spiral turn and pupil swap, always looping |
| `pellet` twinkle | 2 | 0.25 s tick | About 1 in 12 uneaten pellets per tick, re-picked at random |
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
    start-up.
  - `GRP_Game`, the root: characters, FX, debug markers and the vignette.
  - `GRP_UserInterface`: the HUD and the scanlines.
- **Prefabs:** `Assets/Prefabs/Characters/Player`, an `Enemy` base with `Enemy_Chase`, `Enemy_CutOff`, `Enemy_Scared` and
  `Enemy_Random` variants (frames and timing only), `Assets/Prefabs/FX/FX_SugarBurst` and `FX_PointsPopup`, and the existing
  `Pellet` and `BonusPickUp` (all level pellets are instances).
- **Image colours:** every art Image is white (no tint).
- **Debug targets:** `SeekerEnemyTarget` (Chase, `#c4a6ff`), `CutOffEnemyTarget` (`#a2f25a`) and `ScaredEnemyTarget`
  (`#34d9ac`) use `debug_target` tinted with their enemy's colour. Random has no target.
- **`MazeSkin`** (`Assets/Art/MazeSkin.asset`): layout `Blob47`, `wallTiles` = `walls_0` … `walls_46` in order (use the
  **Fill Wall Tiles From Sheet** context menu after changing `walls.png`), `wallTint` white, `corridorColor` transparent,
  `treatOutsideAsWall` true. `GridManager` applies it at start-up; its **Apply Maze Skin** context menu previews it in edit
  mode.
- **`CharacterAnimator`:** the Player uses `hero_side_0/1`, `hero_up_0/1` and `hero_down_0/1` at 0.125 s with
  `animateOnlyWhileMoving`; each enemy variant uses its `sideFrames` with the timings in section 11.
- **Score:** `ScoreManager.bonusPickUpSprites` = cherry, strawberry, orange, apple, melon, potion, bell, key, in
  `BonusItemType` order.
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

Every frame is 16x16. "Drawn" means final art is in `spec.json` and `out/`.

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
| Vignette | 68x88 | – | – | `VignetteOverlay` (generated at runtime, bilinear) | No file needed |
| HUD text, `+NN` popup | – | – | – | The project TTF with the colours above | No sprite |

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
2. Keep the outline, lighting and white rules.
3. Run `sh tools/regenerate.sh`.
4. Look at the 2x column of the contact sheet and at `review/canvas_576x784_x2.png`.
5. Read `review/metrics.txt`.
6. Copy the changed `out/*.png` into `Assets/Art/Sprites/<Folder>/`. Strip widths must stay whole multiples of 16, or the
   slice names change. After changing `walls.png`, run **Fill Wall Tiles From Sheet** on `Assets/Art/MazeSkin.asset`.

**Review checklist:**

- [ ] Reads at 2x: you can name it from the small copy alone.
- [ ] No orphan pixels, no off-palette colours, outline closed.
- [ ] Light from the top-left.
- [ ] Enemy separation is at least 15 dE in every simulation, or the silhouette clearly differs.
- [ ] No ghost-colour echo on its role.
- [ ] Pellet twinkle stays inside 4x4.
- [ ] All 47 wall tiles checked on the contact sheet and the stress map.
- [ ] Nothing in the originality hard rule.

## 15. Readability results (`review/metrics.txt`)

- **Separation:** every actor pair is at least 15 dE in normal vision and the deutan and protan simulations. The closest
  pairs are bat vs lolly (worst simulation 16.4) and hero vs jelly (16.5).
- **One exception:** Peeper vs Jitters under tritanopia (11.5). The silhouettes carry it: a spider on a thread against a
  solid cube, with 0.53 silhouette overlap. Tritanopia is very rare.
- **Ghost colours:** no echo on any behaviour role. Jitters' spearmint is 16 degrees from cyan, but cyan's role is Random,
  not Scared. The hero is white, not Pac-Man yellow.
- **Contrast on the corridor:**

  | Sprite | Contrast | Note |
  |---|---|---|
  | Hero | 17:1 | |
  | Bat | 8.3:1 | |
  | Spider | 12.5:1 | |
  | Jelly | 9.5:1 | |
  | Lolly body | 4.0:1 | Its white eyes and pink swirl are 17:1 and 7.2:1 |
  | Pellet | 13.5:1 | 12 bright px, 48 screen px at 2x |

## 16. Known limits

- **HUD font:** the project TTF's outlines are not on an exact pixel grid, so some HUD and popup glyph edges are slightly
  soft. A true bitmap pixel font (with a raster, point-filtered TMP atlas) would make them fully crisp.
- **Debug markers:** shown whenever `ProjectSettings.DebugSettings.DebugEnemyTargets` is on, including in release builds.
- **Spawn:** Chase, CutOff and Random start on the same tile and travel together for the first few seconds, so they overlap
  until their targets diverge.
- **Atlas:** none yet (see section 12).
- **Testing scene:** `PacmanTesting.unity` uses the new maze but keeps its original placeholder characters, and it cannot enter Play mode: it predates the current managers (it has no PelletManager or ScoreManager, and stale enemy data). Open it on its own rather than alongside `Pacman.unity`.

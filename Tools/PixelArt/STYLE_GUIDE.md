# Candy Haunt: production style guide

A haunted sweet shop at midnight. Glossy strawberry-jelly walls, lit from the top-left, sit on a deep grape-navy night.
Mallow the marshmallow cat collects sugar sparkles while four candy creatures hunt her. At a glance it reads as a dark-screen
1980s maze chaser (bright maze edges, small glowing dots, chunky characters, centred score bar). Every shape and colour is
original.

This guide is the reference for producing and integrating every sprite. The source of truth is `art.txt`. Everything else
is generated from it.

| File | What it is |
|---|---|
| `art.txt` | Editable ASCII source: palette, every sprite frame, wall quadrant patterns |
| `spec.json` | Built from `art.txt` (`node tools/build.js art.txt spec.json`) |
| `out/*.png` | Production PNG strips (frames left to right) and `walls.png` (47-tile autotile sheet) |
| `contact_sheet.png` | Every frame at 6x with a 2x copy (the real in-game size), all 47 wall tiles with indices, palette |
| `mockup.png` / `mockup_alt_frames.png` | The real level at 3x, frame 0 and frame 1 of every animated sprite |
| `review/canvas_576x784_x2.png` | The real WebGL canvas (288x392 reference at 2x, 8 px margins) |
| `review/stress_walls_x2.png` | Wall stress test: pillars, checkerboards, rings, masses, crosses, concave corners |
| `review/metrics.txt` | Contrast, CIEDE2000 and colour-blind separation between actors, ghost-colour echo check |
| `review/before_after_x3.png` | The judged proposal next to this revision |
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
  eyes, fangs and Peeper's thread. Pickups use it only for highlights and sugar.
- **Faces:** every enemy has white eyes with ink pupils. No pickup has eyes or a face, so candy enemies are never mistaken
  for candy pickups.
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

Changes from the judged proposal: `D` brightened from `#7a2270` and `J` deepened from `#8f2c80` (the wall bevel). `V`
lightened from `#a97cff` (the bat was the weakest actor). `B` deepened from `#4c8bff` and the lolly swirl moved from lilac to
strawberry (the bat and lolly were too close). The Scared jelly moved from lemon to spearmint (`T`, slightly deepened from
`#3fe6b8`). Cocoa `C` became tangerine `O`, so the Orange pickup can be a real orange. Do not add a 17th colour without
updating this table and re-running `tools/check.js`.

## 3. Background, corridors and layout

- **Background and corridors:** one flat colour, `N #1e1442`, over the whole screen, with no corridor texture or tint.
  Pellets and characters always sit on the darkest, calmest value.
- **Canvas (288x392 reference):** 8 px margin at the top, then the 272x24 HUD band, then the 272x352 maze, then an 8 px
  margin at the bottom. The left and right margins are 8 px each, all in `N`.
- **Unity:** set the camera clear colour to `#1e1442`, and set `MazeSkin.corridorColor` to fully transparent so the
  background shows through.

## 4. Walls: strawberry jelly bars lit from the top-left

![walls](contact_sheet.png)

- **Autotile:** Blob47, the 8-neighbour mask that `MazeSkin` already supports. The sheet holds 47 tiles in ascending
  reduced-mask order (N=1, NE=2, E=4, SE=8, S=16, SW=32, W=64, NW=128), the same order as `MazeSkin.BuildBlobLookup`.
  `walls.png` (752x16) is sliced by the importer into `walls_0` … `walls_46`, which go into `MazeSkin.wallTiles` in that
  order.
- **Wall mode `corners`:** the patterns are 8x8 quadrants authored as the north-east quadrant, as before, but with one set
  per tile corner. That lets north- and west-facing edges be lit while south- and east-facing edges sit in shade. The
  original renderer mirrors one set into every corner, so the lighting comes out symmetric. Use `tools/render.js`, a superset
  of the original toolkit. The shared original writes the sprite strips but throws when it reaches the walls.
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
- **Animation:** 8 fps (0.125 s per frame), advancing only while moving. Ideally hold frame 0 when blocked by a wall (this
  needs a small optional change in `CharacterAnimator`).
- **Facing:**
  - Left uses the side frames mirrored (`scale.x = -1`).
  - Up and Down use `hero_up` / `hero_down` through `CharacterAnimator.upFrames` / `downFrames`, so she no longer snaps to
    face right on vertical moves.

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
- **Twinkle behaviour (optional, sparse):**
  - One driver, not 142 animators, with pellets on their own sub-canvas.
  - Every 0.25 s, about 1 in 12 uneaten pellets (at least 1) show frame 1 for one tick. They are re-picked at random each
    tick and never the same pellet twice in a row.
  - Shipping every pellet static on frame 0 is fine for the first version.

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
| 5 | `Galaxian` → **`Potion`** | 60 | `pickup_potion.png` | Round-bottomed flask: lilac glass, strawberry brew with white bubbles, tangerine cork, a lemon twinkle beside it. Replaces the trademarked spaceship; rename the enum value to `Potion` |
| 6 | `Bell` | 70 | `pickup_bell.png` | Lemon candy bell with tangerine shading, a white shine, a cherry-red bow and an ink clapper |
| 7 | `Key` | 80 | `pickup_key.png` | Sweetheart key: cherry-red heart bow with a white shine, 2 px lemon and tangerine shaft, two teeth |

Pickups sit on the bonus cell, here (8, 8). An optional 1 px bob at 2 Hz is allowed. Avoid making any pickup's main
colour an enemy hue (`V`, `G`, `T`, `B`).

## 9. HUD

![hud](out/hud.png)

- **Layout:** a 272x24 band at the top of the canvas, on the same `N` background, with no panel or frame. The content is
  centred as one group: twinkle, 8 px gap, `SCORE`, 8 px gap, six digits, 8 px gap, twinkle.
- **Label `SCORE`:** gloss pink `L #ffcbe8`, drop shadow raspberry `D #a8358a`.
- **Score:** six digits, zero-padded (`ScoreDisplay.digits = 6`), sugar white `W #ffffff`, drop shadow lilac
  `S #b3a6ea`.
- **Shadows:** offset exactly 1 art pixel straight down, with no blur or softness.
- **Type:** the project's pixel TTF (`Assets/Fonts/PixelFont`), sized so the cap height is 7 art px (one font pixel = one
  art pixel at the canvas scale), with the glyph tops 8 px below the top of the band. The 5x7 glyphs in the mockup only
  stand in for the TTF.
- **Twinkles:** two 16x16 Images using `pellet_0` (the 4x4 sparkle), vertically centred on the glyphs. They may join the
  pellet twinkle driver.
- **Banners:** none (no READY!). If a win message is added later, use the same `L`-plus-shadow and `W`-plus-shadow pairs,
  centred over the middle corridor, never yellow text.
- **Implementation note:** TMP underlay has one colour per material. Use two TMP texts (label and value), each with its own
  underlay material, or a shadow copy of the text 1 art px lower that `ScoreDisplay` updates. The current single text with
  a rich-text label colour cannot give the two shadow colours.

## 10. FX and overlays

All effects are sprites or text, with no shaders, post-processing or particles.

| Effect | Spec |
|---|---|
| Sugar burst (pellet eaten) | `fx_sugar_burst.png`, 2 frames: a white core with lemon rays, then a ring of lemon sparks. Frame 0 shows for 0.06 s and frame 1 for 0.09 s, while alpha fades from 1 to 0 over the 0.15 s. Pooled; never instantiate per pellet |
| Bonus pickup | The same burst on the pickup's cell, plus a `+NN` popup: pixel TTF in `W` with a 1 px `P` shadow, rising 8 art px over 0.6 s and fading over the last 0.2 s. Pooled |
| Scanlines | The existing `ScanlineOverlay`, with darkness **0.15**. The component defaults to 0.25, so set it on the scene instance. It darkens one screen row per art pixel and hides itself at 1x |
| Vignette (optional) | Radial: alpha **0.25** of `K` at the corners, smoothstep from 45% of the half-diagonal outwards. It covers the maze rect (272x352) only, not the HUD. Generate it at runtime like `ScanlineOverlay` (for example a 68x88 RGBA texture, bilinear, clamp, `raycastTarget` off). Never a PNG under `Assets/Art/Sprites`, where the importer would force Point filtering and the gradient would band |

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

Debug target markers (debug builds only) sit above the enemies. The look must also hold with no vignette; it does, and the
vignette only deepens the corners.

## 11. Animation timings

| Sprite | Frames | `secondsPerFrame` | Notes |
|---|---|---|---|
| `hero_side`, `hero_up`, `hero_down` | 2 each | 0.125 (8 fps) | Advance while moving; hold frame 0 when blocked (optional); side frames mirrored for Left |
| `enemy_chase` | 2 | 0.125 (8 fps) | Wing flap, always looping |
| `enemy_cutoff` | 2 | 0.167 (6 fps) | Thread bob and glance, always looping |
| `enemy_scared` | 2 | 0.2 (5 fps) | Tall and squash wobble, always looping |
| `enemy_random` | 2 | 0.167 (6 fps) | Spiral turn and pupil swap, always looping |
| `pellet` twinkle | 2 | 0.25 s tick | About 1 in 12 uneaten pellets per tick, re-picked at random |
| `fx_sugar_burst` | 2 | 0.06 then 0.09, once | Alpha fades from 1 to 0 |
| `+NN` popup | text | 0.6 s | Rises 8 art px, fades over the last 0.2 s |
| Pickups, walls, debug target | 1 | – | Static (pickups may bob 1 px at 2 Hz) |

All pivots are centred, and every frame is 16x16. Movement and frames land on whole art pixels.

## 12. Unity integration notes (for the integration step)

- **Import:** PNGs go under `Assets/Art/Sprites/<Folder>/`. `PixelArtImporter` applies Point filtering, no mipmaps, no
  compression, 16 pixels per unit and FullRect on first import, and slices strips whose width is a whole multiple of their
  height into `<file>_<n>`. Set Max Size to 1024 for `walls.png` (752 px wide) and 64 for everything else.
- **Atlas:** pack all game sprites into one Sprite Atlas: Point filtering, no compression, padding 2, no rotation, no tight
  packing, mipmaps off.
- **Sub-canvases:** static maze; pellets (rebuilt on eat or twinkle); actors (moving every frame); HUD; overlays. Moving
  characters must not re-batch the 374 grid cells and 142 pellets.
- **Image colours:** every art Image uses white (no tint). Clear the current placeholder tints: Player red, enemies
  blue/pink/orange/green, and the Pellet prefab's yellow, which would turn the white core yellow.
- **Debug targets:** the four `*EnemyTarget` Images use `debug_target` tinted with the owning enemy's colour:

  | Target | Tint |
  |---|---|
  | `SeekerEnemyTarget` (Chase) | `#c4a6ff` |
  | `CutOffEnemyTarget` | `#a2f25a` |
  | `ScaredEnemyTarget` | `#34d9ac` |
  | `RandomEnemyTarget` | `#3a74f0` |

- **`MazeSkin`:** layout `Blob47`, `wallTiles` = `walls_0` … `walls_46` in order, `wallTint` white, `corridorColor`
  transparent, `treatOutsideAsWall` true. `MazeSkin` only logs an error when tiles are missing, not when they are out of
  order, so a "fill wall tiles from texture" editor helper (sorted by the `_N` suffix) is worth adding. Check the result with
  the grid's edit-mode apply.
- **`CharacterAnimator`:**
  - Player: `sideFrames` = `hero_side_0/1`, `upFrames` = `hero_up_0/1`, `downFrames` = `hero_down_0/1`, 0.125 s.
  - Each enemy: `sideFrames` only, with the timings in section 11.
- **Score:** `ScoreManager.bonusPickUpSprites` = cherry, strawberry, orange, apple, melon, potion, bell, key. Rename
  `BonusItemType.Galaxian` to `Potion`. Keep its position (value 5) so serialised data survives, and rename
  `ChangeToGalaxian` to `ChangeToPotion`. `ScoreDisplay.labelColor` = `#ffcbe8`, value text white, shadows as in section 9.
- **Unused assets:** once nothing references them, remove `Assets/Images/icons8-x-50.png` (a third-party icon, replaced by
  `debug_target`) and `Assets/Images/pickups.png`.

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
| `enemy_chase.png` | 32x16 | 2 | `enemy_chase_0..1` | SeekerEnemy (Chase) `sideFrames` | Drawn |
| `enemy_cutoff.png` | 32x16 | 2 | `enemy_cutoff_0..1` | SeekerEnemy (Cutoff) `sideFrames` | Drawn |
| `enemy_scared.png` | 32x16 | 2 | `enemy_scared_0..1` | SeekerEnemy (Scared) `sideFrames` | Drawn |
| `enemy_random.png` | 32x16 | 2 | `enemy_random_0..1` | RandomMovementEnemy `sideFrames` | Drawn |
| `pellet.png` | 32x16 | 2 | `pellet_0..1` | Pellet prefab (frame 0), twinkle driver (frame 1), HUD twinkles | Drawn |
| `pickup_cherry.png` | 16x16 | 1 | single | `bonusPickUpSprites[0]` | Drawn |
| `pickup_strawberry.png` | 16x16 | 1 | single | `bonusPickUpSprites[1]` | Drawn |
| `pickup_orange.png` | 16x16 | 1 | single | `bonusPickUpSprites[2]` | Drawn |
| `pickup_apple.png` | 16x16 | 1 | single | `bonusPickUpSprites[3]` | Drawn |
| `pickup_melon.png` | 16x16 | 1 | single | `bonusPickUpSprites[4]` | Drawn |
| `pickup_potion.png` | 16x16 | 1 | single | `bonusPickUpSprites[5]` (was Galaxian) | Drawn |
| `pickup_bell.png` | 16x16 | 1 | single | `bonusPickUpSprites[6]` | Drawn |
| `pickup_key.png` | 16x16 | 1 | single | `bonusPickUpSprites[7]` | Drawn |
| `walls.png` | 752x16 | 47 | `walls_0..46` | `MazeSkin.wallTiles` (Blob47) | Generated |
| `fx_sugar_burst.png` | 32x16 | 2 | `fx_sugar_burst_0..1` | Pooled pellet and pickup burst | Drawn (optional feature) |
| `debug_target.png` | 16x16 | 1 | single | The four `*EnemyTarget` markers, tinted per enemy | Drawn |
| Scanline overlay tile | 1 x scale | – | – | `ScanlineOverlay` (generated at runtime, darkness 0.15) | No file needed |
| Vignette | about 68x88 | – | – | New runtime overlay (bilinear, outside the sprites folder) | No file needed |
| HUD text, `+NN` popup | – | – | – | Pixel TTF with the colours above | No sprite |

Optional extras, not needed by the current loop: a WebGL loading or favicon icon (crop `hero_down_0`) and a hero "caught"
pose (2 frames) if a lose state is added. Enemies need no frightened, eaten or up/down sprites: there are no power-ups and
they face the camera.

## 14. Art pipeline

Node only, with no packages. From the style folder (or `Tools/PixelArt/` once committed):

```sh
sh tools/regenerate.sh
```

This builds `spec.json`, then writes `out/*.png`, the contact sheet, both mockups, the 2x canvas check, the wall stress test
and `review/metrics.txt`.

| Tool | Job |
|---|---|
| `tools/build.js` | `art.txt` to `spec.json`. Handles `@palette`, `@sprite`, `@frame [mirror]` and `@walls corners` with `@corner ne/nw/se/sw` blocks |
| `tools/walls.js` | Generates the four corner pattern sets from a distance field (bands, core, radius, inset, glints) |
| `tools/render.js` | The original toolkit plus the `corners` wall mode and the mockup vignette preview. Commands: `sprites <spec> <outDir> [scale]`, `mockup <config> <out.png> [scale]` |
| `tools/sheet.js`, `tools/hud.js`, `tools/make_mockup.js`, `tools/imgutil.js` | Review images |
| `tools/check.js` | Readability and originality metrics, using the same maths as the readability judge |

**Adding or changing a sprite:**

1. Edit the `@sprite` block in `art.txt`, using palette keys only.
2. Keep the outline, lighting and white rules.
3. Run `sh tools/regenerate.sh`.
4. Look at the 2x column of the contact sheet and at `review/canvas_576x784_x2.png`.
5. Read `review/metrics.txt`.
6. Copy the changed `out/*.png` into `Assets/Art/Sprites/<Folder>/`. Strip widths must stay whole multiples of 16, or the
   slice names change.

**Review checklist:**

- [ ] Reads at 2x: you can name it from the small copy alone.
- [ ] No orphan pixels, no off-palette colours, outline closed.
- [ ] Light from the top-left.
- [ ] Enemy separation is at least 15 dE in every simulation, or the silhouette clearly differs.
- [ ] No ghost-colour echo on its role.
- [ ] Pellet twinkle stays inside 4x4.
- [ ] All 47 wall tiles checked on the contact sheet and the stress map.
- [ ] Nothing in the originality hard rule.

## 15. Must-fix compliance

| Judge | Must-fix | Done |
|---|---|---|
| Beauty | Quiet the walls (deeper `J`, inset 1-2 px) | `J` deepened to `#6e2468` (1.7:1), flat core, inset raised to 2 px, so corridors read 20 px |
| Beauty | Sparse, small twinkle | The twinkle stays inside the 4x4 footprint, about 1 in 12 pellets per 0.25 s tick, staggered at random |
| Beauty | Beef up the cat (2 px legs, thick tail, strong outline); 2 px spider legs | Cat has fully outlined 2 px legs, a 2 px hooked tail and a closed outline (179 px, the strongest silhouette). Peeper has three chunky 2 px legs per side |
| Beauty | Brighten the bat | `V` lightened to `#c4a6ff` (5.7:1 to 8.3:1 on the corridor, 4.8:1 on the wall core) |
| Readability | Scared jelly off lemon | Spearmint `T #34d9ac`, with no Clyde or Pac-Man-yellow echo and well apart from the pellets |
| Readability | Lolly away from the bat | Deeper blue body with a strawberry swirl. Bat vs lolly went from 12.8 to 24.6 dE (worst simulation 16.4, was about 11) |
| Readability | Twinkle within the footprint | Yes, 4x4 in both frames |
| Readability | Border as one band; top-left bevel, not an all-round rim | `outOfBoundsIsWall` true; four-corner wall mode with lit north and west faces and shaded south and east faces |
| Feasibility | Wall-tile order helper or documented order | Order documented (section 4) with a helper recommended (section 12); the contact sheet numbers every tile |
| Feasibility | Commit the pipeline with a one-line regenerate | `tools/` plus `sh tools/regenerate.sh`, ready to drop into `Tools/PixelArt/` |
| Feasibility | Ship pellets static, or as a cached swap | Static frame 0 is the baseline; the twinkle is one optional cached driver |
| Feasibility | Draw the 6 remaining pickups; a true orange | All 8 drawn; cocoa became tangerine `O` for a real orange |
| Feasibility | Optional facing change for the cat | Solved in art: `hero_up` and `hero_down` plug into the existing `upFrames`/`downFrames` |

Ideas grafted from the runner-ups:

- **Circuit:** the standing 47-tile review grid and stress map; the runtime vignette, kept low; scanlines kept at 0.15.
- **Console-bricks:** connectivity changes only the rims while the core stays flat; everything checked at the real 2x; the
  eaten-pellet sparkle FX.
- **Neon:** each actor's hue kept exclusive, and pellets in a hue no actor uses as a body colour.
- **Candy:** its own centred HUD with flanking twinkles.
- **Feasibility groundwork:** one atlas and character sub-canvases.

## 16. Readability results (`review/metrics.txt`)

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

## 17. Open points

- **Renderer:** the `corners` wall mode needs `tools/render.js` here. The shared original renderer throws on this
  `spec.json` when it reaches the walls. Adopt this copy as the project renderer under `Tools/PixelArt/`.
- **Pixel TTF:** its real metrics are unknown. Confirm a size that gives a 7 art px cap height on the 16 px grid.
- **Code changes implied by the art:** none is required to show the art. They are hero hold-frame-when-stopped, the pellet
  twinkle driver, pooled FX and `+NN` popups, the vignette component, the HUD shadow objects and the `Galaxian` to `Potion`
  rename. Decide which ship in the first pass.

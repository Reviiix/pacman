# Pixel art source

Source and toolchain for the game's "Candy Haunt" pixel art. Unity ignores this folder; the imported sprites live in
`Assets/Art/Sprites/`.

- `art.txt` is the source of truth: the 16-colour palette, every sprite frame as ASCII and the wall generator settings.
- `STYLE_GUIDE.md` covers the look, the palette roles, the animation timings, the naming rules and how the art is wired into Unity.
- `tools/` is a Node toolchain with no package dependencies.

## Change a sprite

1. Edit its `@sprite` block in `art.txt`, using palette keys only. Never hand-edit `walls.png`; change the `@walls` settings instead.
2. Run `sh tools/regenerate.sh`. It needs Node only.
3. Check `contact_sheet.png` (frames at 6x and at the real 2x size), `review/canvas_576x784_x2.png` and `review/metrics.txt`.
4. Copy the changed `out/*.png` over the matching file in `Assets/Art/Sprites/<Folder>/`. Keep strip widths a whole multiple
   of 16, so Unity keeps the slice names (`enemy_chase_0`, `walls_46`...) and every reference survives.

You can also edit the PNGs in `Assets/Art/Sprites/` directly in any pixel editor. If you do, port the change back to `art.txt`,
or the next regenerate will overwrite it.

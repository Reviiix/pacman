# pacman
A recreation of the original pacman arcade game made with Unity.

Grid System
The grid system generates works in the editor to generate a grid layout for you to create levels with. Grid items can either be blank or walls.

Movement System
MovementManager moves the player and enemies every frame. Each MovingObject advances an unrounded logical position a fixed number of tiles per second and draws the character on the nearest whole art pixel.

Game over
Each enemy has a small trigger collider (EnemyContact); when one touches the player the game ends with a GAME OVER, tap to retry dialog, which reloads the level.

Power pellets
The four corner pellets are power pellets (the PowerPellet prefab, a Pellet variant). Eating one starts PowerMode (in GRP_GameManagers) for 5 seconds: the player and enemies swap colours (each CharacterAnimator's powered frames) and an enemy that touches the player is defeated instead. A defeated enemy leaves the board and MovementManager brings it back on its start tile 5 seconds later. The looks flash for the last second, then return to normal. The durations are on PowerMode and MovementManager.

Visuals
The pixel-art presentation (integer canvas scaling, autotiled walls, sprite flipbooks, pickup effects, CRT overlays and the stepped fade from black when a level starts) lives in Assets/Scripts/Visuals. The art source, toolchain and style guide are in Tools/PixelArt.

Audio
Sound effects are original chiptune sounds synthesised by Tools/Audio (see its README) into Assets/Audio/SFX. SoundEffects in Assets/Scripts/Audio plays them from the pellet, pickup, power-mode start and end, pause and win events and the level start.

Project Initialiser
Handles the order of game initialisation. Allows for better debugging of race conditions by giving granular control over order or initialisation. Also gives an easy way to inject dependencies.

# Abstractions
Singleton
A type singleton abstract class. Inheritors will follow the singleton pattern.
# Web build
The game is built for WebGL to run inside the portfolio site's player (an iframe whose shape changes with the page and full screen).
- Template: Assets/WebGLTemplates/Portfolio fills the frame, shows a loading percentage and loads nothing from other sites (the site's Content-Security-Policy blocks that). Player Settings select it.
- Settings: Brotli compression, no decompression fallback, data caching on, explicitly thrown exceptions only, engine code stripping and Medium managed stripping, hardware statistics off.
- Fit: PixelPerfectCanvasScaler keeps whole-number scales for crisp pixels and shrinks to fit exactly when the frame is smaller than 288x392, so the whole maze is always visible. On touch screens the D-pad keeps a strip clear below the game (unless it fits beside it, as in landscape): the scalers fit the game above the strip and raise GRP_Playfield and GRP_HUD to sit centred there. In a short frame this can drop the game a whole scale step.
- Controls: arrow keys or WASD, swipe or the on-screen D-pad (bottom-right, shown only on touch screens) on phones and tablets, P to pause, M or the speaker button in the HUD's top-right to mute (never Esc, which the site uses to leave the game). The game also pauses when it loses focus, and the game-complete and game-over dialogs restart the level.
- Sound: browsers only let audio play after a click, tap or key press in the game's page, so the start jingle waits for that (and is skipped after four seconds) and earlier effects are silent.
- Build: File > Build Profiles > Web, scene Assets/Scenes/Pacman.unity, output Builds/WebGL (ignored by git). Copy index.html and Build/ into the site's public/games/pac-man/ folder.

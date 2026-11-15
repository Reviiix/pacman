# pacman
A recreation of the original pacman arcade game made with Unity.

Grid System
The grid system generates works in the editor to generate a grid layout for you to create levels with. Grid items can either be blank or walls.

Movement System
MovementManager moves the player and enemies every frame. Each MovingObject advances an unrounded logical position a fixed number of tiles per second and draws the character on the nearest whole art pixel.

Visuals
The pixel-art presentation (integer canvas scaling, autotiled walls, sprite flipbooks, pickup effects and CRT overlays) lives in Assets/Scripts/Visuals. The art source, toolchain and style guide are in Tools/PixelArt.

Project Initialiser
Handles the order of game initialisation. Allows for better debugging of race conditions by giving granular control over order or initialisation. Also gives an easy way to inject dependencies.

# Abstractions
Singleton
A type singleton abstract class. Inheritors will follow the singleton pattern.
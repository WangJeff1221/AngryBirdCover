# angry bird space cover

A Unity 2D classroom cover of Angry Birds Space, with simple geometric characters and four scenes based on Pig Bang 1-1, 1-3, 1-10 and 1-14.

[Play the Web version on itch.io](https://xiashuozsr.itch.io/angrybirdspacecoverfinal)

## Open the project

1. Clone or download this repository.
2. In Unity Hub, add the repository folder as a project.
3. Open it with **Unity 6000.3.7f1**.
4. Open `Assets/Cover1/Scenes/01_PigBang_1-1.unity` and press Play.

The repository contains Assets, Packages and ProjectSettings. Unity generates Library and other caches when the project opens.

## Controls

| Input | Action |
| --- | --- |
| Hold the left mouse button on the bird and drag | Aim |
| Release | Launch |
| Left click during flight | Split a blue bird or detonate a black bird |
| R | Retry the current level |
| 1–4 | Select a level |
| V | Switch between overview and following the current shot |
| N | Load the next bird when available |
| Enter after winning | Continue; the last level returns to the first |

Levels advance automatically two seconds after winning, except the last level. This version has no audio or text HUD.

## Implementation

- `GravityField` calculates constant acceleration toward a planet inside its range. `WorldPhysics` applies force using mass × acceleration.
- `TrajectoryPredictor` updates velocity and position every physics step and stops the preview at an obstacle. It does not use an extra prediction scene.
- Bird abilities, block durability, debris, pig fading and camera following use small scripts under `Assets/Cover1/Scripts`.
- Seven prefabs store birds, pigs, impact rings, fragments and aiming lines. Buildings, planets and the slingshot support are authored scene objects.
- Runtime spawning copies prefabs rather than assembling GameObjects and components in code.

Use the **Cover** editor menu to open the first level or build Windows / WebGL.

## Reference and process

Original game: **Angry Birds Space**, by Rovio. Level references were studied using AngryBirdsNest gameplay videos for [1-1](https://www.youtube.com/watch?v=Woybs2pA_Mw), [1-3](https://www.youtube.com/watch?v=TSNDHfwC81w), [1-10](https://www.youtube.com/watch?v=tCO-FcClkCY) and [1-14](https://www.youtube.com/watch?v=3If-tUiIb-A).

AI assistance included discussing implementation approaches, explaining physics and existing code/API usage, and helping implement, simplify and debug the code. The cover uses simple shapes and has remaining differences in geometry, collision response and timing.

Validation: 33 rule checks and four automatic level replays passed. The silent Web build was checked in Chrome for loading, scene selection, aiming, launch and retry. These checks are not a student playtest.

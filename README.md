# Reversi Game

A clean, scene-driven Reversi project in Unity where you battle a minimax AI through a polished start-to-finish game flow.

![Reversi Game splash screenshot](Assets/ReversiGame/Splash/Texture/ReversiSplashScreen.png)

## About

This is my first published game project.

It is also my first fully vibe-coded game: I used AI collaboration heavily while learning and building. My personal starting point in the project was defining the game states and the AI rules, then growing the project from there. I am happy to collaborate, keep improving it, and learn in public.

## Features

- Full playable Reversi match flow: `Bootstrap -> Splash -> Instructions -> Settings -> Gameplay -> EndGame`
- Human vs AI gameplay with legal move detection, disc flipping, pass handling, and match result resolution
- Difficulty options: `Easy`, `Medium`, and `Hard`
- Player color selection: `Black`, `White`, or `Random`
- AI built with minimax and alpha-beta pruning
- WebGL-friendly AI path that spreads work across frames to avoid browser freezing
- Additive endgame overlay that keeps the final board visible behind the result UI
- Scene-based architecture with Zenject dependency injection

## Tech Stack

- Engine: `Unity 6` (`6000.3.8f1`)
- Language: `C#`
- Rendering: `Universal Render Pipeline`
- Dependency injection: `Extenject / Zenject`
- Async flow: `UniTask`
- Input: `Unity Input System`
- UI animation/support: `DOTween`
- Target included in repo: `WebGL`

### Key Packages

- `com.cysharp.unitask`
- `com.unity.inputsystem` `1.18.0`
- `com.unity.render-pipelines.universal` `17.3.0`
- `com.unity.addressables` `2.8.1`
- `com.unity.cinemachine` `2.10.6`
- `com.unity.test-framework` `1.6.0`
- `Extenject` (`Assets/Plugins/Zenject`)
- `DOTween` (`Assets/Plugins/Demigiant/DOTween`)

## How To Run

### In Unity

1. Open the project in `Unity 6000.3.8f1`.
2. Let Unity import packages and compile scripts.
3. Open the startup scene: `Assets/ReversiGame/BootStrap/Scene/Bootstrap.unity`.
4. Press Play.

### Included WebGL Build

This repo already includes a built WebGL version in [`WebGL/`](WebGL).

To run it locally:

1. Serve the `WebGL` folder with a local static server.
2. Open the served `index.html` in your browser.

Example:

```powershell
cd WebGL
python -m http.server 8080
```

Then open `http://localhost:8080`.

## Controls

- `Left Mouse Click`: interact with UI and place a disc
- `Enter`: submit in supported UI input paths
- Touch input also works through the Unity UI input setup

Gameplay is primarily point-and-click.

## Roadmap

- Add stronger AI heuristics and more tuning
- Improve board feedback, piece animations, and overall juice
- Add better audio and visual polish
- Package desktop builds in GitHub Releases
- Add more screenshots and a gameplay GIF
- Expand menus and onboarding UX

## Download Latest Build

- Future packaged builds: [GitHub Releases](https://github.com/githubwizard2023/ReversiGame/releases)
- Current playable build in repo: [`WebGL/`](WebGL)

## Collaboration

If you want to contribute ideas, fixes, polish, or teaching feedback, check [`CONTRIBUTING.md`](CONTRIBUTING.md).

## Credits

Project-specific credits and third-party notes are in [`CREDITS.md`](CREDITS.md).

## License

This project is released under the MIT License. See [`LICENSE.md`](LICENSE.md).

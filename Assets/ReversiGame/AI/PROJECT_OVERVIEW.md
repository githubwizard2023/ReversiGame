# Project Overview

## Summary

This project is a Unity-based Reversi game in early production.
The startup flow is already implemented with Zenject and scene-based orchestration.
The gameplay scene is connected into the global flow and now includes a working Reversi gameplay domain, UI layer, and AI move selection.

## Current Architecture

- Engine: Unity
- Language: C#
- Dependency injection: Zenject
- Runtime configuration: `ScriptableObject` via `GlobalGameSettings`
- Startup flow style: table-driven state machine using strategies
- Scene loading model: additive stage-scene transitions over a persistent bootstrap scene
- Target gameplay style: pure C# domain logic with thin MonoBehaviour presentation

## Implemented Flow

The current high-level game flow is:

1. `Bootstrap` scene starts.
2. `BootstrapInitializeNotifier` notifies `GameManager` that startup is complete.
3. `GameManager` enters `GameState.Init`.
4. `InitializeGameStateStrategy` transitions to `SplashScene`.
5. `SplashGameStateStrategy` waits for the configured splash duration.
6. Flow transitions to `InstructionsScene`.
7. `InstructionsGameStateStrategy` waits for continue input and transitions to `GameSettingsScene`.
8. `SettingsGameStateStrategy` waits for the setup screen to report completion.
9. `GameManager` transitions to `GameplayScene`.

This means the global application flow now reaches gameplay through the manager-owned state pipeline.

## Main Systems In Place

### Bootstrap And Global Flow

- `Assets/ReversiGame/BootStrap/Scene/Bootstrap.unity`
- `BootstrapInitializeNotifier` is the bridge from scene startup into code-driven flow.
- `GameManager` orchestrates high-level states.
- `IGameFlowStateStrategy` defines one strategy per game state.
- `GameSceneTransitionService` handles additive scene loading and unloading.
- `GameFlowInstaller` binds shared flow services, input/audio services, and strategies through Zenject.

### Settings

- `GlobalGameSettings` currently exposes `splash_duration_seconds` and a shared `button_click_clip`.
- The settings asset is loaded from `Resources/GlobalGameSettings`.
- `GameSetupSession` stores the resolved setup data that gameplay can read after the settings scene unloads.
- `GameSetupCompletionAwaiter` lets the settings scene report completion without owning scene transitions.

### Audio And Input

- `AudioPoolService` provides pooled one-shot playback through `IAudioService`.
- `MouseClickAudioTrigger` uses the new Unity Input System to play the shared button click clip on left mouse clicks.
- Shared audio clips are bound from `GlobalGameSettings` into the root Zenject container.

### Existing Content

- `SplashScene` exists and includes splash artwork.
- `InstructionsScene` exists and includes an instructions image.
- `GameSettingsScene` exists and includes the setup UI for player color and difficulty selection.
- `GameplayScene` exists with gameplay logic, UI bindings, and turn-driven match flow.

## Gameplay Module Status

The `Assets/ReversiGame/Gameplay` module currently contains:

- `Scenes/GameplayScene.unity`
- `LOGIC`
- `UI`

Current observations:

- The gameplay scene already contains a `SceneContext`.
- The scene contains gameplay presentation objects under the canvas, including board and HUD views.
- The gameplay module now includes board state, legal move finding, disc flipping, move execution, scoring, and game-over evaluation.
- Match flow is implemented and coordinates human turns, AI turns, pass handling, and result resolution.
- `AIMoveChooser` uses minimax with alpha-beta pruning and Burst-backed board evaluation jobs.
- The gameplay code is no longer just a scaffold, though scene-level wiring can still be expanded.

This means the gameplay module is functional and already covers the core Reversi match loop.

## Existing Game States

The `GameState` enum already anticipates a broader game:

- `Init`
- `Splash`
- `MainMenu`
- `Settings`
- `Instructions`
- `Gameplay`
- `EndGame`
- `Quit`

`Init`, `Splash`, `Instructions`, and `Settings` currently have active strategy implementations.
`Gameplay` is now backed by dedicated gameplay flow logic. `EndGame` is now the dedicated end-of-match flow state and scene.

## Recommended Gameplay Direction

The intended gameplay architecture should stay specialized for classic 8x8 Reversi and avoid generic board-game abstractions.

Recommended separation:

- Pure domain logic in plain C# classes
- Thin gameplay orchestration layer for match flow and AI turn handling
- Thin UI MonoBehaviours for board rendering, input forwarding, and HUD updates
- Zenject used for scene-level composition, not for unnecessary abstraction
- Burst used selectively for native-friendly AI evaluation hotspots rather than forcing the whole gameplay stack into jobs

Recommended core gameplay systems:

- Board state
- Move validation
- Disc flip resolution
- Legal move finder
- Turn management
- Match flow controller
- Score calculation
- Game over evaluation
- AI move chooser
- Board view
- Cell view
- HUD view

## Board Model Direction

The clean board logic model for Reversi should be:

- `Empty`
- `Black`
- `White`

Legal move hints should remain UI-only state and must not be stored in the core board model.
Grey optional move markers are presentation details, not gameplay facts.

## UI Direction

For the gameplay board:

- The board root should remain named `Board`.
- The current `RawImage` setup is not the ideal long-term component choice.
- A `RectTransform` root with `Image`-based child cells is a better fit than a single `RawImage`.
- An 8x8 `GridLayoutGroup` is a pragmatic choice for version 1.
- Each cell should be its own child UI element.
- A full board refresh after each move is acceptable and matches the project goals.

## Folder Snapshot

- `Assets/ReversiGame/AI`
  - AI coding rules and project overview documents.
- `Assets/ReversiGame/BootStrap`
  - Persistent startup scene and bootstrap notifier.
- `Assets/ReversiGame/GameManager`
  - High-level game flow state machine, transition service, interfaces, and installer.
- `Assets/ReversiGame/GameSettings`
  - Setup scene, selection logic, setup session storage, and related enums.
- `Assets/ReversiGame/Instructions`
  - Instructions scene and artwork.
- `Assets/ReversiGame/Audio`
  - Shared audio service interfaces, pooled playback runtime, and global mouse-click audio trigger.
- `Assets/ReversiGame/Splash`
  - Splash scene and artwork.
- `Assets/ReversiGame/Gameplay`
  - Active gameplay module containing domain logic, AI, match flow, and UI views.
- `Assets/ReversiGame/Docs`
  - Present as a module folder, but currently unused from the scanned project content.

## What Is Still Missing

The project still has follow-up work around polish and expansion:

- Gameplay installer and any remaining scene-level composition cleanup
- Verification that Burst evaluation paths compile and perform correctly in the Unity editor and player builds
- Additional AI tuning beyond the current minimax depth and positional-weight heuristic
- Gameplay polish, feedback, and UX refinement
- Broader state handling inside the dedicated `EndGame` scene or later endgame extensions

## Practical Assessment

The project now has two strong foundations:

- A clean global startup flow
- A manager-driven path from settings into gameplay
- A functional gameplay module with domain logic, UI, and AI integration

The main remaining work is refinement rather than first-pass implementation.
The next major milestone is to harden the gameplay scene wiring, validate the Burst-backed AI path in Unity, and continue polishing the board and HUD experience.

Use only the new Unity Input System.

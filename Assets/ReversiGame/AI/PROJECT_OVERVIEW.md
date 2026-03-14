# Project Overview

## Summary

This project is a Unity-based Reversi game with a working end-to-end flow:

`Bootstrap -> Splash -> Instructions -> Settings -> Gameplay -> EndGame -> Quit/Menu/Replay`

The project already has playable match logic, scene-based orchestration through Zenject, an additive endgame overlay flow, and AI move selection based on minimax with alpha-beta pruning.

## Core Architecture

- Engine: Unity
- Language: C#
- Dependency injection: Zenject
- Shared configuration: `GlobalGameSettings` `ScriptableObject`
- High-level flow: manager-owned state machine using `IGameFlowStateStrategy`
- Scene transitions: additive loading through `GameSceneTransitionService`
- Gameplay design: plain C# domain logic with thin MonoBehaviour presentation

## Implemented Game Flow

1. `Bootstrap.unity` starts and notifies the root flow.
2. `GameManager` enters `GameState.Init`.
3. `InitializeGameStateStrategy` transitions to `SplashScene`.
4. `SplashGameStateStrategy` waits for the configured splash duration.
5. Flow transitions to `InstructionsScene`.
6. `InstructionsGameStateStrategy` waits for continue input.
7. Flow transitions to `GameSettingsScene`.
8. `SettingsGameStateStrategy` waits for the setup screen to submit the selected options.
9. Flow transitions to `GameplayScene`.
10. `GameplayGameStateStrategy` waits for the match to finish, then loads `EndGameScene` additively on top of `GameplayScene`.
11. `EndGameGameStateStrategy` routes the next action:
    - `Menu` -> unload `EndGameScene` and `GameplayScene`, then load `GameSettingsScene`
    - `PlayAgain` -> unload `EndGameScene`, keep `GameplayScene` loaded, and restart the match in place
    - `Quit` -> `GameState.Quit`

This means the full scene loop from startup to replay or quit already exists, and the end screen can appear without hiding the finished board state underneath.

## Main Systems In Place

### Bootstrap And Global Flow

- `BootStrap/Scene/Bootstrap.unity` is the persistent startup scene.
- `BootstrapInitializeNotifier` bridges scene startup into code-driven flow.
- `GameManager` orchestrates state execution and scene transitions.
- `GameFlowInstaller` binds global services, awaiters, sessions, audio, and all current flow strategies.
- `GameSceneTransitionService` performs additive scene loading and unloading.

### Shared Settings And Platform Resolution

- `GlobalGameSettings` currently exposes:
  - `splash_duration_seconds`
  - `platform`
  - `button_click_clip`
- The shared settings asset is loaded from `Resources/GlobalGameSettings`.
- `PlatformSelectionResolver` resolves either the configured platform override or the runtime platform.

### Setup Phase

- `GameSetupScreen` and `GameSetupScreenController` drive setup UI.
- `GameSetupResolver` converts UI selection into runtime-ready `GameSetupData`.
- `GameSetupSession` persists the resolved setup after the settings scene unloads.
- `GameSetupCompletionAwaiter` lets the settings scene report completion without owning scene flow.
- Default setup selection currently starts with:
  - player color: `Random`
  - difficulty: `Easy`

### Gameplay Phase

- `GameplayInstaller` binds gameplay-local services inside the gameplay scene container.
- `ReversiBoard` is the shared runtime board state.
- `ReversiLegalMoveFinder`, `ReversiDiscFlipper`, and `ReversiMoveExecutor` handle the core rules.
- `ReversiBoardEvaluator` handles scoring and game-over evaluation.
- `MatchState` and `MatchFlowController` run the turn loop, pass handling, and match completion.
- `GameplayCompletionAwaiter` lets the gameplay scene signal flow completion back to `GameManager`.
- `GameplaySceneInitializer` now acts as a reusable gameplay scene controller that can start a fresh match without reloading the gameplay scene.
- `GameplaySceneRuntime` lets the root flow start or restart the currently loaded gameplay scene on demand.

### AI

- `AIMoveChooser` selects AI moves with minimax and alpha-beta pruning.
- Editor and non-WebGL builds use synchronous minimax with search depth:
  - `Easy`: depth 1
  - `Medium`: depth 3
  - `Hard`: depth 5
- WebGL uses a coroutine-based search spread across frames with search depth:
  - `Easy`: depth 1
  - `Medium`: depth 2
  - `Hard`: depth 3
- Non-WebGL leaf board evaluation is offloaded to Burst-compiled jobs over a flattened native board snapshot.
- WebGL falls back to managed evaluation and yields regularly to avoid blocking the browser.
- The heuristic is positional-weight based and tuned for standard 8x8 Reversi.

### EndGame Phase

- `MatchResultSession` stores the last completed match result between scene transitions.
- `EndGameResultView` renders the final outcome.
- `EndGameButtonsView` reports menu, replay, and quit actions through `IEndGameChoiceAwaiter`.
- `EndGameQuitStrategySelector` resolves quit behavior by effective platform.
- `EndGameScene` is loaded additively over `GameplayScene` so the final board can remain visible behind the endgame UI.
- The endgame state disables gameplay post-processing blur once a choice is made and control returns to the flow manager.
- Choosing `Menu` unloads both gameplay and endgame before returning to settings.
- Choosing `PlayAgain` unloads only the endgame overlay and starts a new match inside the already-loaded gameplay scene.
- Current quit strategies exist for:
  - `EDITOR`
  - `PC`
  - `WebGL`
  - `Secondlife`

### Audio And Input

- `AudioPoolService` provides pooled one-shot playback through `IAudioService`.
- `MouseClickAudioTrigger` uses the new Unity Input System to play the shared click sound.
- The project should continue using only the new Unity Input System.

## Gameplay Module Snapshot

`Assets/ReversiGame/Gameplay` currently contains:

- `Installer`
- `Interfaces`
- `LOGIC`
- `Scenes`
- `Textures`
- `UI`

Implemented gameplay responsibilities:

- Board initialization
- Legal move detection
- Disc flipping
- Move execution
- Turn ownership
- Human input submission
- AI turn execution
- Pass handling
- Match end detection
- Match outcome resolution
- Board and HUD refresh events

This is a functional gameplay module, not a placeholder scaffold.

## Domain Model Direction

The board model is specialized for classic 8x8 Reversi and should stay that way.

Core cell states:

- `Empty`
- `Black`
- `White`

UI-only concepts such as legal-move hints must stay out of the board domain model.

## Existing Game States

The current `GameState` enum includes:

- `Init`
- `Splash`
- `MainMenu`
- `Settings`
- `Instructions`
- `Gameplay`
- `EndGame`
- `Quit`

Active implemented runtime flow currently uses:

- `Init`
- `Splash`
- `Instructions`
- `Settings`
- `Gameplay`
- `EndGame`
- `Quit`

`MainMenu` exists in the enum but is not part of the active scene pipeline shown in the scanned project structure.

## Folder Snapshot

- `Assets/ReversiGame/AI`
  - Project documentation and AI-specific guidance.
- `Assets/ReversiGame/Audio`
  - Shared audio interfaces, pooled playback runtime, and click trigger.
- `Assets/ReversiGame/BootStrap`
  - Persistent startup scene and bootstrap notifier.
- `Assets/ReversiGame/EndGame`
  - Endgame scene, result rendering, choice awaiter, match-result session, and quit strategies.
- `Assets/ReversiGame/GameManager`
  - Global state machine, scene transition service, installers, and flow strategies.
- `Assets/ReversiGame/GameSettings`
  - Setup scene, setup UI, selection resolution, and session storage.
- `Assets/ReversiGame/Gameplay`
  - Gameplay installer, domain logic, AI, match flow, and UI views.
- `Assets/ReversiGame/GlobalSettings`
  - Shared runtime settings, audio asset references, and platform enums/resolution.
- `Assets/ReversiGame/Instructions`
  - Instructions scene and continue flow.
- `Assets/ReversiGame/Splash`
  - Splash scene and artwork.

## Practical Assessment

The project already has three solid foundations:

- A manager-driven global flow that reaches gameplay and endgame cleanly
- A working gameplay module with dedicated domain, orchestration, and UI layers
- An additive endgame overlay model with in-place replay and platform-aware quit behavior

The main remaining work is refinement and validation rather than first-pass feature creation.

## Follow-Up Work

The most likely next tasks are:

- Validate Burst-backed AI behavior and performance in player builds, not only in-editor
- Continue AI tuning beyond the current positional heuristic and search-depth mapping
- Polish gameplay feedback, board presentation, and endgame UX
- Clean up any remaining scene wiring assumptions that still rely on object-name lookup
- Decide whether `MainMenu` should be implemented or removed from the state enum

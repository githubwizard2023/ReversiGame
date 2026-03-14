using Zenject;

namespace Game
{
    // This installer wires the persistent game-flow services into the Zenject root container.
    // It keeps dependency setup in one place instead of scattering bindings across runtime code.
    // This matches the project's DI approach without forcing unnecessary abstractions.
    public class GameFlowInstaller : MonoInstaller
    {
        private const bool IS_MISSING_GLOBAL_GAME_SETTINGS_LOGGING_ENABLED = true;
        private const string GLOBAL_GAME_SETTINGS_RESOURCE_PATH = "GlobalGameSettings";

        // Registers the manager, scene transition helper, and the initial state strategies.
        // These bindings create the first runnable version of the high-level game flow.
        public override void InstallBindings()
        {
            BindGlobalGameSettings();

            Container.Bind<IGameManager>().To<GameManager>().AsSingle().NonLazy();
            Container.Bind<IGameSceneTransitionService>().To<GameSceneTransitionService>().AsSingle();
            Container.Bind<ISplashContinueAwaiter>().To<SplashContinueAwaiter>().AsSingle();
            Container.Bind<IInstructionsContinueAwaiter>().To<InstructionsContinueAwaiter>().AsSingle();
            Container.Bind<IGameSetupSession>().To<GameSetupSession>().AsSingle();
            Container.Bind<IGameSetupCompletionAwaiter>().To<GameSetupCompletionAwaiter>().AsSingle();
            Container.Bind<IMatchResultSession>().To<MatchResultSession>().AsSingle();
            Container.Bind<IEndGameChoiceAwaiter>().To<EndGameChoiceAwaiter>().AsSingle();
            Container.Bind<IPlatformSelectionResolver>().To<PlatformSelectionResolver>().AsSingle();
            Container.Bind<IAudioService>().To<AudioPoolService>().AsSingle();
            Container.Bind<IGameplaySceneRuntime>().To<GameplaySceneRuntime>().AsSingle();
            Container.Bind<IGameplayPostProcessingService>().To<GameplayPostProcessingService>().AsSingle();
            Container.BindInterfacesTo<MouseClickAudioTrigger>().AsSingle().NonLazy();

            Container.Bind<IGameFlowStateStrategy>().To<InitializeGameStateStrategy>().AsSingle();
            Container.Bind<IGameFlowStateStrategy>().To<SplashGameStateStrategy>().AsSingle();
            Container.Bind<IGameFlowStateStrategy>().To<InstructionsGameStateStrategy>().AsSingle();
            Container.Bind<IGameFlowStateStrategy>().To<SettingsGameStateStrategy>().AsSingle();
            Container.Bind<IGameplayCompletionAwaiter>().To<GameplayCompletionAwaiter>().AsSingle();
            Container.Bind<IGameFlowStateStrategy>().To<GameplayGameStateStrategy>().AsSingle();
            Container.Bind<IGameFlowStateStrategy>().To<EndGameGameStateStrategy>().AsSingle();
            Container.Bind<IGameFlowStateStrategy>().To<QuitGameStateStrategy>().AsSingle();
        }

        // Loads the shared runtime settings asset once and exposes it through the root container.
        // A missing asset is logged clearly so startup failures are diagnosable from the editor.
        private void BindGlobalGameSettings()
        {
            GlobalGameSettings global_game_settings =
                UnityEngine.Resources.Load<GlobalGameSettings>(GLOBAL_GAME_SETTINGS_RESOURCE_PATH);

            if (global_game_settings == null)
            {
                GameDebugLogger.LogError(
                    "GlobalGameSettings asset was not found at Resources/GlobalGameSettings.",
                    IS_MISSING_GLOBAL_GAME_SETTINGS_LOGGING_ENABLED);
                global_game_settings = UnityEngine.ScriptableObject.CreateInstance<GlobalGameSettings>();
            }

            Container.Bind<GlobalGameSettings>().FromInstance(global_game_settings).AsSingle();
            Container.Bind<UnityEngine.AudioClip>()
                .WithId(AudioClipIds.BUTTON_CLICK)
                .FromInstance(global_game_settings.button_click_clip)
                .AsSingle();
        }
    }
}

using Zenject;

namespace Game
{
    // This installer wires all gameplay-local services into the scene's Zenject sub-container.
    // It keeps board logic, AI, match flow, and move execution scoped to the gameplay scene.
    // This follows the same scene-level installer pattern established by GameSetupInstaller.
    public class GameplayInstaller : MonoInstaller
    {
        // Registers domain services, match orchestration, and AI into the gameplay scene container.
        // The board is a single shared instance so all services operate on the same game state.
        public override void InstallBindings()
        {
            Container.Bind<ReversiBoard>().AsSingle();
            Container.Bind<ReversiDiscFlipper>().AsSingle();
            Container.Bind<ReversiLegalMoveFinder>().AsSingle();
            Container.Bind<ReversiBoardEvaluator>().AsSingle();
            Container.Bind<ReversiMoveExecutor>().AsSingle();
            Container.Bind<AIMoveChooser>().AsSingle();
            Container.Bind<MatchState>().AsSingle();
            Container.Bind<MatchFlowController>().AsSingle();
        }
    }
}

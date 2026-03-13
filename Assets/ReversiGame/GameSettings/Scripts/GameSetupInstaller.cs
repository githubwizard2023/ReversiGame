using Zenject;

namespace Game
{
    // This installer wires the settings-scene setup feature into that scene's Zenject container.
    // It uses a scene-level installer so UI-facing setup services stay local while persistent match data stays in the root flow container.
    // This fits the new scene split because the settings scene owns selection logic but not the application's long-lived state.
    public class GameSetupInstaller : MonoInstaller
    {
        // Registers the local services the settings screen needs to resolve selections and start gameplay.
        public override void InstallBindings()
        {
            Container.Bind<IGameSetupResolver>().To<GameSetupResolver>().AsSingle();
            Container.Bind<IGameSetupScreenController>().To<GameSetupScreenController>().AsSingle();
        }
    }
}

using System.Threading.Tasks;

namespace Game
{
    // Coordinates access to the currently loaded gameplay scene from the root game flow.
    public interface IGameplaySceneRuntime
    {
        void RegisterController(IGameplaySceneController gameplay_scene_controller);

        void UnregisterController(IGameplaySceneController gameplay_scene_controller);

        Task BeginMatchAsync();
    }
}

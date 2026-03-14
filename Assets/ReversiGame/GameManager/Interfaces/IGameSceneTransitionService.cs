using System.Threading.Tasks;

namespace Game
{
    // This interface abstracts additive stage-scene transitions away from the manager.
    // It isolates scene loading and unloading from the game flow orchestration logic.
    // This keeps scene handling replaceable without bloating the manager.
    public interface IGameSceneTransitionService
    {
        // Unloads the current stage scene, if one exists, and loads the requested stage scene additively.
        // The bootstrap scene stays alive while stage scenes are swapped under it.
        Task TransitionToStageSceneAsync(string target_scene_name);

        // Loads a scene additively without unloading the currently tracked stage scene.
        Task LoadSceneAdditivelyAsync(string target_scene_name);

        // Unloads a specific loaded scene by name if it is currently present.
        Task UnloadSceneAsync(string target_scene_name);
    }
}

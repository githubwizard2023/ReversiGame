using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    // This service owns additive stage-scene transitions for the persistent game flow.
    // It keeps scene swapping out of the manager so orchestration stays clean and focused.
    // This is the boundary between game flow decisions and Unity scene operations.
    public class GameSceneTransitionService : IGameSceneTransitionService
    {
        private string _current_stage_scene_name;

        // Unloads the previously active stage scene and loads the next stage scene additively.
        // The bootstrap scene is not tracked here, so it remains loaded for the full session.
        public async Task TransitionToStageSceneAsync(string target_scene_name)
        {
            if (string.IsNullOrWhiteSpace(target_scene_name))
            {
                return;
            }

            if (_current_stage_scene_name == target_scene_name)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_current_stage_scene_name) == false)
            {
                Scene current_stage_scene = SceneManager.GetSceneByName(_current_stage_scene_name);

                if (current_stage_scene.IsValid() && current_stage_scene.isLoaded)
                {
                    AsyncOperation unload_operation = SceneManager.UnloadSceneAsync(current_stage_scene);

                    if (unload_operation != null)
                    {
                        await AwaitAsyncOperationAsync(unload_operation);
                    }
                }
            }

            AsyncOperation load_operation = SceneManager.LoadSceneAsync(target_scene_name, LoadSceneMode.Additive);

            if (load_operation == null)
            {
                return;
            }

            await AwaitAsyncOperationAsync(load_operation);

            Scene loaded_stage_scene = SceneManager.GetSceneByName(target_scene_name);

            if (loaded_stage_scene.IsValid() && loaded_stage_scene.isLoaded)
            {
                SceneManager.SetActiveScene(loaded_stage_scene);
                _current_stage_scene_name = target_scene_name;
            }
        }

        // Waits for a Unity async operation without introducing engine-specific dependencies into the manager.
        // This keeps scene transition timing self-contained inside the scene transition service.
        private static async Task AwaitAsyncOperationAsync(AsyncOperation async_operation)
        {
            while (async_operation.isDone == false)
            {
                await Task.Yield();
            }
        }
    }
}

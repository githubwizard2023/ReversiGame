using System.Threading.Tasks;

namespace Game
{
    // This strategy handles quit requests when the effective platform is the Unity editor.
    // It keeps editor-only quit behavior separate from browser feedback and desktop application exits.
    // This fits the project because editor quit is a distinct behavior even when the same button is pressed.
    public class EditorEndGameQuitStrategy : IEndGameQuitStrategy
    {
        public Platforms target_platform => Platforms.EDITOR;

        public bool keeps_end_game_open => false;

        public Task ExecuteAsync()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            return Task.CompletedTask;
        }
    }
}

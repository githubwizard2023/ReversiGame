using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    // This strategy handles quit requests for standalone desktop builds.
    // It keeps direct application termination separate from the endgame view and platform resolver.
    // This fits the project because desktop quit is immediate and does not need any scene feedback.
    public class PcEndGameQuitStrategy : IEndGameQuitStrategy
    {
        public Platforms target_platform => Platforms.PC;

        public bool keeps_end_game_open => false;

        public Task ExecuteAsync()
        {
            Application.Quit();
            return Task.CompletedTask;
        }
    }
}

using System.Collections;
using UnityEngine;

namespace Game
{
    // Represents the live gameplay scene entry point that can begin a fresh match on demand.
    public interface IGameplaySceneController
    {
        void BeginMatch();
    }

    public interface IGameplayCoroutineRunner
    {
        Coroutine StartGameplayCoroutine(IEnumerator routine);
    }
}

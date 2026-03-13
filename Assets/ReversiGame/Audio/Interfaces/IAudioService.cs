using UnityEngine;

namespace Game
{
    // This interface defines the shared entry point for one-shot sound playback across scenes.
    // It creates a small boundary so UI and gameplay code request audio without owning AudioSource objects directly.
    // This fits the project because pooled playback should stay centralized and globally reusable.
    public interface IAudioService
    {
        void Play(AudioClip clip);
    }
}

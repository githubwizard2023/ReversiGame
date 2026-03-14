using UnityEngine;

namespace Game
{
    // This asset centralizes runtime settings that multiple systems can consume through DI.
    // It uses a ScriptableObject so designers can tweak startup values without touching code.
    // This fits the project's Zenject setup by providing one shared configuration source.
    [CreateAssetMenu(fileName = "GlobalGameSettings", menuName = "Reversi/Game Settings")]
    public class GlobalGameSettings : ScriptableObject
    {
        [Min(0f)]
        public float splash_duration_seconds = 5f;

        public Platforms platform = Platforms.AUTO;

        public AudioClip button_click_clip;
    }
}

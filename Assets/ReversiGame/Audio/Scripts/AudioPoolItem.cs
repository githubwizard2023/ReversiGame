using System;
using System.Collections;
using UnityEngine;

namespace Game
{
    // This class owns one pooled AudioSource and its return-to-pool lifecycle.
    // It follows the pooled-object pattern so clip playback timing stays attached to the spawned AudioSource instance.
    // This fits the solution because each sound player needs a local coroutine and a clean reset path.
    public class AudioPoolItem : MonoBehaviour
    {
        private AudioSource _audio_source;
        private Action<AudioPoolItem> _on_playback_completed;
        private Coroutine _playback_coroutine;

        public AudioSource audio_source => _audio_source;

        // Receives the AudioSource created by the pool service so the item can manage playback locally.
        public void Initialize(AudioSource audio_source)
        {
            _audio_source = audio_source;
        }

        // Starts clip playback on this pooled object and schedules the return callback when it finishes.
        public void Play(AudioClip clip, Action<AudioPoolItem> on_playback_completed)
        {
            if (_audio_source == null)
            {
                return;
            }

            _on_playback_completed = on_playback_completed;
            gameObject.SetActive(true);

            _audio_source.clip = clip;
            _audio_source.Play();

            if (_playback_coroutine != null)
            {
                StopCoroutine(_playback_coroutine);
            }

            _playback_coroutine = StartCoroutine(WaitForPlaybackCompleted());
        }

        // Stops any active playback and clears state before the item returns to the available pool.
        public void ResetState()
        {
            if (_playback_coroutine != null)
            {
                StopCoroutine(_playback_coroutine);
                _playback_coroutine = null;
            }

            if (_audio_source != null)
            {
                _audio_source.Stop();
                _audio_source.clip = null;
            }

            _on_playback_completed = null;
            gameObject.SetActive(false);
        }

        // Waits until the AudioSource finishes so the pool service can reclaim this object automatically.
        private IEnumerator WaitForPlaybackCompleted()
        {
            yield return null;

            while (_audio_source != null && _audio_source.isPlaying)
            {
                yield return null;
            }

            _playback_coroutine = null;
            _on_playback_completed?.Invoke(this);
        }
    }
}

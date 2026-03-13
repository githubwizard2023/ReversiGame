using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    // This class provides global one-shot audio playback through a reusable pool of AudioSource objects.
    // It follows the service-plus-object-pool pattern so callers depend on one interface while runtime instances are recycled.
    // This fits the solution because button clicks and short effects should not allocate or manage throwaway audio objects.
    public class AudioPoolService : IAudioService
    {
        private const string AUDIO_POOL_ROOT_NAME = "GlobalAudioPool";
        private const string AUDIO_POOL_ITEM_NAME_PREFIX = "PooledAudioSource";
        private const int INITIAL_POOL_SIZE = 4;

        private readonly Queue<AudioPoolItem> _available_items = new Queue<AudioPoolItem>();
        private readonly HashSet<AudioPoolItem> _active_items = new HashSet<AudioPoolItem>();

        private Transform _audio_pool_root;
        private int _created_item_count;

        // Plays a clip on the next available pooled AudioSource and expands the pool when needed.
        public void Play(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            EnsurePoolCreated();

            AudioPoolItem audio_pool_item = GetAvailableItem();
            _active_items.Add(audio_pool_item);
            audio_pool_item.Play(clip, ReturnToPool);
        }

        // Creates the persistent pool root the first time audio is requested and prewarms a few reusable players.
        private void EnsurePoolCreated()
        {
            if (_audio_pool_root != null)
            {
                return;
            }

            GameObject audio_pool_root_game_object = new GameObject(AUDIO_POOL_ROOT_NAME);
            Object.DontDestroyOnLoad(audio_pool_root_game_object);
            _audio_pool_root = audio_pool_root_game_object.transform;

            for (int index = 0; index < INITIAL_POOL_SIZE; index++)
            {
                AudioPoolItem audio_pool_item = CreatePoolItem();
                _available_items.Enqueue(audio_pool_item);
            }
        }

        // Returns an idle pooled item when possible and creates a new one only when the pool is exhausted.
        private AudioPoolItem GetAvailableItem()
        {
            if (_available_items.Count > 0)
            {
                return _available_items.Dequeue();
            }

            return CreatePoolItem();
        }

        // Creates one pooled GameObject with an AudioSource so the service can recycle it for future clips.
        private AudioPoolItem CreatePoolItem()
        {
            _created_item_count++;

            GameObject audio_pool_item_game_object =
                new GameObject($"{AUDIO_POOL_ITEM_NAME_PREFIX}_{_created_item_count}");

            audio_pool_item_game_object.transform.SetParent(_audio_pool_root, false);

            AudioSource audio_source = audio_pool_item_game_object.AddComponent<AudioSource>();
            audio_source.playOnAwake = false;
            audio_source.loop = false;

            AudioPoolItem audio_pool_item = audio_pool_item_game_object.AddComponent<AudioPoolItem>();
            audio_pool_item.Initialize(audio_source);
            audio_pool_item.ResetState();

            return audio_pool_item;
        }

        // Reclaims a finished player, clears its state, and makes it available for the next sound request.
        private void ReturnToPool(AudioPoolItem audio_pool_item)
        {
            if (audio_pool_item == null)
            {
                return;
            }

            if (_active_items.Remove(audio_pool_item) == false)
            {
                return;
            }

            audio_pool_item.transform.SetParent(_audio_pool_root, false);
            audio_pool_item.ResetState();
            _available_items.Enqueue(audio_pool_item);
        }
    }
}

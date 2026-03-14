using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game
{
    [RequireComponent(typeof(RawImage))]
    public class EndGameResultView : MonoBehaviour
    {
        private const bool IS_MISSING_RESULT_LOGGING_ENABLED = true;
        private const float OVERLAY_FADE_IN_DURATION_SECONDS = 0.5f;
        private const float OVERLAY_HOLD_DURATION_SECONDS = 5f;
        private const float OVERLAY_FADE_OUT_DURATION_SECONDS = 1f;

        [SerializeField]
        private Texture _human_win_texture;

        [SerializeField]
        private Texture _ai_win_texture;

        [SerializeField]
        private Texture _draw_texture;

        [SerializeField]
        private Texture _close_tab_texture;

        [SerializeField]
        private Texture _secondlife_endgame_texture;

        private IMatchResultSession _match_result_session;
        private RawImage _raw_image;
        private Texture _result_texture;
        private Tween _active_overlay_tween;

        [Inject]
        private void Construct(IMatchResultSession match_result_session)
        {
            _match_result_session = match_result_session;
        }

        private void Awake()
        {
            _raw_image = GetComponent<RawImage>();
        }

        private void OnDestroy()
        {
            _active_overlay_tween?.Kill();

            if (_raw_image != null)
            {
                _raw_image.DOKill();
            }
        }

        private void Start()
        {
            if (_match_result_session.TryGetMatchOutcome(out MatchOutcome match_outcome) == false)
            {
                GameDebugLogger.LogError(
                    "EndGame scene started without a stored match result.",
                    IS_MISSING_RESULT_LOGGING_ENABLED);
                return;
            }

            _result_texture = ResolveTexture(match_outcome);
            _raw_image.texture = _result_texture;
            SetRawImageAlpha(1f);
        }

        public Task ShowCloseTabTextureAsync()
        {
            return ShowTemporaryTextureAsync(_close_tab_texture);
        }

        public Task ShowSecondlifeTextureAsync()
        {
            return ShowTemporaryTextureAsync(_secondlife_endgame_texture);
        }

        private Texture ResolveTexture(MatchOutcome match_outcome)
        {
            switch (match_outcome)
            {
                case MatchOutcome.HumanWin:
                    return _human_win_texture;

                case MatchOutcome.AIWin:
                    return _ai_win_texture;

                default:
                    return _draw_texture;
            }
        }

        private async Task ShowTemporaryTextureAsync(Texture overlay_texture)
        {
            if (_raw_image == null || overlay_texture == null)
            {
                return;
            }

            _active_overlay_tween?.Kill();
            _raw_image.DOKill();

            _raw_image.texture = overlay_texture;
            SetRawImageAlpha(0f);

            Sequence overlay_sequence = DOTween.Sequence();
            overlay_sequence.Append(
                DOTween.ToAlpha(
                    () => _raw_image.color,
                    value => _raw_image.color = value,
                    1f,
                    OVERLAY_FADE_IN_DURATION_SECONDS));
            overlay_sequence.AppendInterval(OVERLAY_HOLD_DURATION_SECONDS);
            overlay_sequence.Append(
                DOTween.ToAlpha(
                    () => _raw_image.color,
                    value => _raw_image.color = value,
                    0f,
                    OVERLAY_FADE_OUT_DURATION_SECONDS));

            _active_overlay_tween = overlay_sequence;
            await WaitForTweenCompletionAsync(overlay_sequence);

            _raw_image.texture = _result_texture;
            SetRawImageAlpha(1f);
            _active_overlay_tween = null;
        }

        private void SetRawImageAlpha(float alpha)
        {
            Color image_color = _raw_image.color;
            image_color.a = alpha;
            _raw_image.color = image_color;
        }

        private static Task WaitForTweenCompletionAsync(Tween tween)
        {
            if (tween == null || tween.IsActive() == false)
            {
                return Task.CompletedTask;
            }

            TaskCompletionSource<bool> completion_source =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            tween.OnComplete(() => completion_source.TrySetResult(true));
            tween.OnKill(() => completion_source.TrySetResult(true));
            return completion_source.Task;
        }
    }
}

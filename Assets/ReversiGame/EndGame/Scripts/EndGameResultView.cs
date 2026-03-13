using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game
{
    [RequireComponent(typeof(RawImage))]
    public class EndGameResultView : MonoBehaviour
    {
        private const bool IS_MISSING_RESULT_LOGGING_ENABLED = true;

        [SerializeField]
        private Texture _human_win_texture;

        [SerializeField]
        private Texture _ai_win_texture;

        [SerializeField]
        private Texture _draw_texture;

        private IMatchResultSession _match_result_session;
        private RawImage _raw_image;

        [Inject]
        private void Construct(IMatchResultSession match_result_session)
        {
            _match_result_session = match_result_session;
        }

        private void Awake()
        {
            _raw_image = GetComponent<RawImage>();
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

            _raw_image.texture = ResolveTexture(match_outcome);
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
    }
}

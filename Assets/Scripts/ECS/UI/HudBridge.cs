using Match3.ECS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Match3.ECS.UI
{
    public class HudBridge : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text timerText;

        private bool _isPaused;

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }
        }

        public void UpdateTimer(int remainingSeconds)
        {
            if (timerText != null)
            {
                timerText.text = remainingSeconds.ToString();
            }
        }

        public void OnPauseClicked()
        {
            _isPaused = !_isPaused;
            GameWorldCommand.Send(new GamePauseCommand { IsPaused = _isPaused });
        }
    }
}

using Match3.ECS.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Match3.ECS.UI
{
    public class GameOverBridge : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text scoreText;
        [SerializeField] private Image[] stars;

        private void Awake()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public void Show(int finalScore, int starCount)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }

            if (scoreText != null)
            {
                scoreText.text = finalScore.ToString();
            }

            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].enabled = i < starCount;
            }
        }

        public void OnRetryClicked()
        {
            GameWorldCommand.Send(new GameRetryCommand());
        }

        public void OnHomeClicked()
        {
            SceneManager.LoadScene("Home");
        }
    }
}

using Match3.ECS.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Match3.ECS.UI
{
    public class ReadyBridge : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "InGame";

        public void OnGameStartClicked()
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
}

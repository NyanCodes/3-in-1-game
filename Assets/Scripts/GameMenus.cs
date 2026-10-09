using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ThreeInOneGame
{
    public class GameMenus : MonoBehaviour
    {
        [SerializeField] private bool isMainMenu;
        [SerializeField] private GameObject pausePanel;

        private void Update()
        {
            if (!isMainMenu && Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
                SetPaused(pausePanel != null && !pausePanel.activeSelf);
        }

        public void StartDriving() => LoadScene("Driving");
        public void StartFlying() => LoadScene("Flying");
        public void StartSumoBall() => LoadScene("SumoBall");
        public void OpenPauseMenu() => SetPaused(true);
        public void Resume() => SetPaused(false);
        public void Restart() => LoadScene(SceneManager.GetActiveScene().name);
        public void BackToMainMenu() => LoadScene("MainMenu");

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void SetPaused(bool paused)
        {
            if (pausePanel == null)
                return;

            pausePanel.SetActive(paused);
            Time.timeScale = paused ? 0f : 1f;
        }

        private static void LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }

        private void OnDisable()
        {
            if (!isMainMenu && pausePanel != null && pausePanel.activeSelf)
                Time.timeScale = 1f;
        }
    }
}

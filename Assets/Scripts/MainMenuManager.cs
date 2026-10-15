using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public RectTransform titleText;
    public GameObject howToPlayPanel;
    public GameObject quitConfirmPanel;
    public TextMeshProUGUI highScoreText;
    
    public Transform cameraPivot;
    public float cameraRotationSpeed = 5f;

    private float titleAnimTimer = 0f;
    private bool titleDropped = false;
    private float startY;

    private void Start()
    {
        Time.timeScale = 1f;
        
        if (highScoreText != null)
        {
            int hs = PlayerPrefs.GetInt("HighScore_3D", 0);
            highScoreText.text = "HIGH SCORE: " + hs;
        }

        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);
    }

    private void Update()
    {
        if (cameraPivot != null)
        {
            cameraPivot.Rotate(0, cameraRotationSpeed * Time.deltaTime, 0);
        }
    }

    public void OnPlayClicked()
    {
        PlayClickSound();
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene("MainScene");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        }
    }

    public void OnHowToPlayClicked()
    {
        PlayClickSound();
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
    }

    public void OnCloseHowToPlay()
    {
        PlayClickSound();
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    public void OnQuitClicked()
    {
        PlayClickSound();
        if (quitConfirmPanel != null) quitConfirmPanel.SetActive(true);
    }

    public void OnConfirmQuit(bool confirm)
    {
        PlayClickSound();
        if (confirm)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else
        {
            if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);
        }
    }
    
    private void PlayClickSound()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButton();
    }
}

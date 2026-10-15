using UnityEngine;
using TMPro;

public class UIManager3D : MonoBehaviour
{
    public static UIManager3D Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject hudPanel;
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject confirmDialogPanel; // Added for Main Menu confirmation

    [Header("Pause UI References")]
    public UnityEngine.UI.Slider bgmSlider;
    public UnityEngine.UI.Slider sfxSlider;
    public TextMeshProUGUI pauseScoreText;
    public TextMeshProUGUI pauseRecordText;
    public UnityEngine.UI.Image[] pauseHeartIcons;
    public UnityEngine.UI.Image hudPauseIcon; // to swap play/pause
    public Sprite iconPause;
    public Sprite iconPlay;
    
    private bool isPaused = false;
    private bool isConfirming = false;
    private bool isResuming = false; // Prevents double pause during countdown

    [Header("HUD Elements")]
    public TextMeshProUGUI scoreText;
    public UnityEngine.UI.Image[] heartIcons; // Array of 3 heart images
    
    [Header("Dash UI")]
    public UnityEngine.UI.Image dashIcon;
    public UnityEngine.UI.Image dashOverlay;
    private Player3DController playerController;

    [Header("Game Over Elements")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Countdown UI")]
    public TextMeshProUGUI countdownText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Memaksa resolusi ke WXGA (1366 x 768) agar lebih ringan
        Screen.SetResolution(1366, 768, FullScreenMode.ExclusiveFullScreen);
    }

    private void Start()
    {
        if (GameManager3D.Instance != null)
        {
            GameManager3D.Instance.OnScoreChanged += UpdateScoreUI;
            GameManager3D.Instance.OnLivesChanged += UpdateLivesUI;
            GameManager3D.Instance.OnGameOver += ShowGameOverUI;
            GameManager3D.Instance.OnGameStarted += ShowHUD;
        }
        
        // Initialize sliders from AudioManager
        if (AudioManager.Instance != null)
        {
            if (bgmSlider) { bgmSlider.value = AudioManager.Instance.bgmVolume; bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume); }
            if (sfxSlider) { sfxSlider.value = AudioManager.Instance.sfxVolume; sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume); }
        }
        
        // Fix missing button listeners caused by lambda serialization issue
        if (pausePanel != null)
        {
            UnityEngine.UI.Button[] buttons = pausePanel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (var b in buttons)
            {
                if (b.name == "ResumeBtn" || b.name == "LANJUTBtn") { b.onClick.RemoveAllListeners(); b.onClick.AddListener(TogglePause); }
                else if (b.name == "MenuBtn" || b.name == "MENUBtn") { b.onClick.RemoveAllListeners(); b.onClick.AddListener(OnMainMenuButtonClicked); }
                else if (b.name == "RetryBtn" || b.name == "ULANGIBtn") { b.onClick.RemoveAllListeners(); b.onClick.AddListener(OnRestartButtonClicked); }
            }
        }
        
        if (confirmDialogPanel != null)
        {
            UnityEngine.UI.Button[] buttons = confirmDialogPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (var b in buttons)
            {
                if (b.name == "YesBtn") { b.onClick.RemoveAllListeners(); b.onClick.AddListener(ConfirmMainMenu); }
                else if (b.name == "NoBtn") { b.onClick.RemoveAllListeners(); b.onClick.AddListener(CancelConfirm); }
            }
        }
        
        ShowMainMenu();
    }

    private void OnDisable()
    {
        if (GameManager3D.Instance != null)
        {
            GameManager3D.Instance.OnScoreChanged -= UpdateScoreUI;
            GameManager3D.Instance.OnLivesChanged -= UpdateLivesUI;
            GameManager3D.Instance.OnGameOver -= ShowGameOverUI;
            GameManager3D.Instance.OnGameStarted -= ShowHUD;
        }
    }

    private void Update()
    {
        if (hudPanel != null && hudPanel.activeSelf)
        {
            if (playerController == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerController = player.GetComponent<Player3DController>();
            }

            if (playerController != null && dashOverlay != null && dashIcon != null)
            {
                float progress = playerController.DashCooldownProgress;
                dashOverlay.fillAmount = 1f - progress;
                
                if (progress >= 1f)
                    dashIcon.color = Color.white;
                else
                    dashIcon.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            
            // Check for Esc input to pause
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (GameManager3D.Instance != null && GameManager3D.Instance.CurrentState == GameManager3D.GameState.Playing)
                {
                    if (isConfirming) CancelConfirm();
                    else TogglePause();
                }
            }
            // Gamepad start button
            else if (UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.startButton.wasPressedThisFrame)
            {
                if (GameManager3D.Instance != null && GameManager3D.Instance.CurrentState == GameManager3D.GameState.Playing)
                {
                    if (isConfirming) CancelConfirm();
                    else TogglePause();
                }
            }
        }
    }

    public void OnPlayButtonClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButton();
        GameManager3D.Instance.StartGame();
    }

    public void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButton();
        GameManager3D.Instance.RestartGame();
    }

    private void UpdateScoreUI(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private void UpdateLivesUI(int lives)
    {
        if (heartIcons != null)
        {
            for (int i = 0; i < heartIcons.Length; i++)
            {
                if (heartIcons[i] != null)
                {
                    if (i < lives) heartIcons[i].color = Color.white; // Active (red heart image)
                    else heartIcons[i].color = new Color(0.3f, 0.3f, 0.3f, 1f); // Lost (grayed out)
                }
            }
        }
    }

    private void ShowMainMenu()
    {
        if(mainMenuPanel) mainMenuPanel.SetActive(true);
        if(hudPanel) hudPanel.SetActive(false);
        if(gameOverPanel) gameOverPanel.SetActive(false);
    }

    private void ShowHUD()
    {
        if(mainMenuPanel) mainMenuPanel.SetActive(false);
        if(hudPanel) hudPanel.SetActive(true);
        if(gameOverPanel) gameOverPanel.SetActive(false);
    }

    private void ShowGameOverUI()
    {
        if(hudPanel) hudPanel.SetActive(false);
        if(gameOverPanel) gameOverPanel.SetActive(true);

        // Text is now handled by GameOverUI.cs
        // if (finalScoreText != null)
        //     finalScoreText.text = "Final Score: " + GameManager3D.Instance.Score;
            
        // if (highScoreText != null)
        //     highScoreText.text = "High Score: " + GameManager3D.Instance.HighScore;
    }

    public void ShowCountdown(int count)
    {
        if (countdownText != null)
        {
            if (count > 0)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = count.ToString();
            }
            else if (count == 0)
            {
                countdownText.text = "GO!";
                Invoke("HideCountdown", 0.5f);
            }
            else
            {
                countdownText.gameObject.SetActive(false);
            }
        }
    }
    
    private void HideCountdown()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    public void OnMainMenuButtonClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButton();
        // Show confirmation dialog instead of directly going
        if (confirmDialogPanel)
        {
            isConfirming = true;
            confirmDialogPanel.SetActive(true);
        }
        else
        {
            ConfirmMainMenu();
        }
    }

    public void ConfirmMainMenu()
    {
        isConfirming = false;
        if (pausePanel) pausePanel.SetActive(false);
        if (confirmDialogPanel) confirmDialogPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (hudPanel) hudPanel.SetActive(false);

        Time.timeScale = 1f;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButton();
        
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene("MainMenu");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    public void CancelConfirm()
    {
        isConfirming = false;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButton();
        if (confirmDialogPanel) confirmDialogPanel.SetActive(false);
    }

    public void TogglePause()
    {
        if (isResuming) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButton();
        if (GameManager3D.Instance == null || GameManager3D.Instance.CurrentState != GameManager3D.GameState.Playing) return;
        
        isPaused = !isPaused;
        
        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pausePanel)
            {
                pausePanel.SetActive(true);
                // Update pause stats
                if (pauseScoreText) pauseScoreText.text = "SKOR\n<size=48>" + GameManager3D.Instance.Score + "</size>";
                if (pauseRecordText) pauseRecordText.text = "REKOR\n<size=48>" + GameManager3D.Instance.HighScore + "</size>";
                if (pauseHeartIcons != null)
                {
                    for (int i = 0; i < pauseHeartIcons.Length; i++)
                    {
                        if (pauseHeartIcons[i] != null)
                            pauseHeartIcons[i].color = (i < GameManager3D.Instance.Lives) ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
                    }
                }
            }
            if (hudPauseIcon && iconPlay) hudPauseIcon.sprite = iconPlay;
            
            DialogManager dm = GameObject.FindAnyObjectByType<DialogManager>();
            if (dm) dm.HideBubbles();
            
            // Lower BGM volume
            if (AudioManager.Instance != null) AudioManager.Instance.SetBGMDucked(true);
        }
        else
        {
            // Resume with countdown
            if (pausePanel) pausePanel.SetActive(false);
            if (hudPauseIcon && iconPause) hudPauseIcon.sprite = iconPause;
            
            // Restore BGM volume
            if (AudioManager.Instance != null) AudioManager.Instance.SetBGMDucked(false);
            
            // Instead of resuming immediately, trigger countdown 3-2-1
            Time.timeScale = 1f; // Need time running for countdown, but wait, if countdown runs, we need game to still be frozen?
            // User requested: Resume: hitung mundur singkat 3-2-1 sebelum game lanjut... lalu timeScale = 1.
            // If we use unscaled time for countdown, we can keep timeScale=0 until it finishes.
            StartCoroutine(ResumeCountdownRoutine());
        }
    }

    private System.Collections.IEnumerator ResumeCountdownRoutine()
    {
        isResuming = true;
        // keep time frozen
        Time.timeScale = 0f;
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            for (int i = 3; i > 0; i--)
            {
                countdownText.text = i.ToString();
                yield return new WaitForSecondsRealtime(0.7f);
            }
            countdownText.text = "GO!";
            yield return new WaitForSecondsRealtime(0.5f);
            countdownText.gameObject.SetActive(false);
        }
        Time.timeScale = 1f;
        isResuming = false;
    }
}

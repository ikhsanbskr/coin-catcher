using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GameOverUI : MonoBehaviour
{
    public CanvasGroup cg;
    public RectTransform windowRect;
    public TextMeshProUGUI scoreTxt;
    public TextMeshProUGUI bestTxt;
    public Button btnRetry;
    public Button btnMenu;
    public UIManager3D uiManager;

    private void Awake()
    {
        btnRetry.onClick.AddListener(OnRetry);
        btnMenu.onClick.AddListener(OnMenu);
    }

    private void OnEnable()
    {
        // Hide HUD
        if (uiManager && uiManager.hudPanel) uiManager.hudPanel.SetActive(false);

        cg.alpha = 0f;
        windowRect.localScale = Vector3.one * 0.8f;
        
        StartCoroutine(AnimateIn());
        if (GameManager3D.Instance != null)
        {
            StartCoroutine(CountScore(GameManager3D.Instance.Score));
            bestTxt.text = GameManager3D.Instance.HighScore.ToString();
        }
    }

    private IEnumerator AnimateIn()
    {
        float timer = 0f;
        float duration = 0.3f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            cg.alpha = timer / duration;
            yield return null;
        }
        cg.alpha = 1f;

        timer = 0f;
        duration = 0.4f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float val = 0.8f + (1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f)) * 0.2f;
            windowRect.localScale = Vector3.one * val;
            yield return null;
        }
        windowRect.localScale = Vector3.one;
    }

    private IEnumerator CountScore(int targetScore)
    {
        float timer = 0f;
        float duration = 1.2f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;
            int current = Mathf.RoundToInt(Mathf.Lerp(0, targetScore, t));
            scoreTxt.text = current.ToString();
            yield return null;
        }
        scoreTxt.text = targetScore.ToString();
    }

    private void Update()
    {
        // Keyboard support
        bool retryPressed = false;
        bool menuPressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame) retryPressed = true;
            if (Keyboard.current.escapeKey.wasPressedThisFrame) menuPressed = true;
        }
#else
        retryPressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
        menuPressed = Input.GetKeyDown(KeyCode.Escape);
#endif

        if (retryPressed) OnRetry();
        if (menuPressed) OnMenu();
    }

    private void OnRetry()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void OnMenu()
    {
        Time.timeScale = 1f;
        if (uiManager) uiManager.OnMainMenuButtonClicked();
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;
    
    public GameObject container;
    public Image progressBar;
    // public RectTransform rotatingCoin;
    public TextMeshProUGUI tipText;
    public TextMeshProUGUI loadingText;
    
    private string[] tips = new string[]
    {
        "Tekan Shift untuk dash!",
        "Perhatikan bayangan di tanah untuk menghindari bom!",
        "Koin bernilai 10 poin.",
        "Awas, bom membuatmu kehilangan hati!",
        "Lari tanpa henti untuk bertahan hidup."
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private bool isLoading = false;

    public void LoadScene(string sceneName)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadRoutine(sceneName));
        }
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        isLoading = true;
        if (container != null) container.SetActive(true);
        if (progressBar != null) progressBar.fillAmount = 0f;
        if (tipText != null && tips != null && tips.Length > 0) tipText.text = tips[UnityEngine.Random.Range(0, tips.Length)];
        
        float timer = 0f;
        float minTime = 1.5f; // reduced a bit so it doesn't take too long

        while (timer < minTime)
        {
            timer += Time.unscaledDeltaTime;
            
            float targetProgress = timer / minTime;

            if (progressBar != null)
            {
                progressBar.fillAmount = Mathf.MoveTowards(progressBar.fillAmount, targetProgress, Time.unscaledDeltaTime * 2f);
            }

            if (loadingText != null)
            {
                int dots = Mathf.FloorToInt(Time.unscaledTime * 3f) % 4;
                loadingText.text = "Memuat" + new string('.', dots);
            }

            yield return null;
        }

        if (progressBar != null) progressBar.fillAmount = 1f;

        // Load scene synchronously to avoid Linux async load corruption bug
        SceneManager.LoadScene(sceneName);

        isLoading = false;
        if (container != null) container.SetActive(false);
    }
}

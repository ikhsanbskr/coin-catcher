using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip coinClip;
    public AudioClip bombClip;
    public AudioClip hitClip;
    public AudioClip gameOverClip;
    public AudioClip buttonClip;
    public AudioClip bgmClip;

    public float bgmVolume { get; private set; } = 1f;
    public float sfxVolume { get; private set; } = 1f;
    private bool isBGMDucked = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (GameManager3D.Instance != null)
        {
            GameManager3D.Instance.OnGameStarted += OnGameStarted;
            GameManager3D.Instance.OnGameOver += OnGameOver;
        }

        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);

        if (sfxSource != null) sfxSource.ignoreListenerPause = true;

        // Start BGM
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            UpdateBGMVolume();
            bgmSource.Play();
        }
    }

    private void OnDestroy()
    {
        if (GameManager3D.Instance != null)
        {
            GameManager3D.Instance.OnGameStarted -= OnGameStarted;
            GameManager3D.Instance.OnGameOver -= OnGameOver;
        }
    }

    private void OnGameStarted()
    {
        isBGMDucked = false;
        UpdateBGMVolume();
    }

    private void OnGameOver()
    {
        PlaySFX(gameOverClip);
        isBGMDucked = true;
        UpdateBGMVolume();
    }

    public void PlayCoin() { PlaySFX(coinClip); }
    public void PlayBomb() { PlaySFX(bombClip); }
    public void PlayHit() { PlaySFX(hitClip); }
    public void PlayButton() { PlaySFX(buttonClip); }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }
    
    public void SetBGMVolume(float v)
    {
        bgmVolume = v;
        PlayerPrefs.SetFloat("BGMVolume", v);
        UpdateBGMVolume();
    }
    
    public void SetSFXVolume(float v)
    {
        sfxVolume = v;
        PlayerPrefs.SetFloat("SFXVolume", v);
    }
    
    public void SetBGMDucked(bool ducked)
    {
        isBGMDucked = ducked;
        UpdateBGMVolume();
    }
    
    private void UpdateBGMVolume()
    {
        if (bgmSource != null)
        {
            float target = bgmVolume * (isBGMDucked ? 0.4f : 1f);
            bgmSource.volume = target;
        }
    }
}

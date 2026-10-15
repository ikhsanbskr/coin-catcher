using UnityEngine;
using System;

public class GameManager3D : MonoBehaviour
{
    public static GameManager3D Instance { get; private set; }

    public enum GameState { MainMenu, Playing, GameOver }
    public GameState CurrentState { get; private set; }

    public int Score { get; private set; }
    public int Lives { get; private set; }
    public int HighScore { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<int> OnLivesChanged;
    public event Action OnGameOver;
    public event Action OnGameStarted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        HighScore = PlayerPrefs.GetInt("HighScore_3D", 0);
        CurrentState = GameState.MainMenu;
    }

    private int coinStreak = 0;

    private void Start()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainScene")
        {
            StartCoroutine(CountdownRoutine());
        }
    }

    private System.Collections.IEnumerator CountdownRoutine()
    {
        // Provide enough time for LoadingScreen to fade if needed, though Scene activation is instant.
        yield return new WaitForSeconds(0.5f);
        
        Score = 0;
        Lives = 3;
        coinStreak = 0;
        OnScoreChanged?.Invoke(Score);
        OnLivesChanged?.Invoke(Lives);

        if (UIManager3D.Instance != null) UIManager3D.Instance.ShowCountdown(3);
        yield return new WaitForSeconds(1f);
        if (UIManager3D.Instance != null) UIManager3D.Instance.ShowCountdown(2);
        yield return new WaitForSeconds(1f);
        if (UIManager3D.Instance != null) UIManager3D.Instance.ShowCountdown(1);
        yield return new WaitForSeconds(1f);
        if (UIManager3D.Instance != null) UIManager3D.Instance.ShowCountdown(0); // "GO!" or hide
        
        CurrentState = GameState.Playing;
        OnGameStarted?.Invoke();

        if (DialogManager.Instance != null)
        {
            if (UnityEngine.Random.value > 0.5f) DialogManager.Instance.TriggerDialog("StartArka", true);
            else DialogManager.Instance.TriggerDialog("StartMaya", true);
        }
    }

    public void StartGame()
    {
        StartCoroutine(CountdownRoutine());
    }

    public void AddScore(int amount)
    {
        if (CurrentState != GameState.Playing) return;

        Score += amount;
        coinStreak++;
        OnScoreChanged?.Invoke(Score);

        if (DialogManager.Instance != null)
        {
            if (coinStreak >= 3 && coinStreak % 3 == 0)
            {
                DialogManager.Instance.TriggerDialog("CoinStreak");
            }
            if (Score > 0 && Score % 100 == 0)
            {
                DialogManager.Instance.TriggerDialog("Score100");
            }
        }
    }

    public void TakeDamage()
    {
        if (CurrentState != GameState.Playing) return;

        Lives--;
        coinStreak = 0;
        OnLivesChanged?.Invoke(Lives);

        if (Lives <= 0)
        {
            TriggerGameOver();
        }
        else if (Lives == 1)
        {
            if (DialogManager.Instance != null) DialogManager.Instance.TriggerDialog("LowLife");
        }
        else
        {
            if (DialogManager.Instance != null) DialogManager.Instance.TriggerDialog("HitBomb");
        }
    }

    private void TriggerGameOver()
    {
        CurrentState = GameState.GameOver;
        
        bool newRecord = false;
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("HighScore_3D", HighScore);
            PlayerPrefs.Save();
            newRecord = true;
        }

        OnGameOver?.Invoke();

        if (DialogManager.Instance != null)
        {
            if (newRecord) DialogManager.Instance.TriggerDialog("NewRecord", true);
            else if (UnityEngine.Random.value > 0.5f) DialogManager.Instance.TriggerDialog("GameOverMaya", true);
            else DialogManager.Instance.TriggerDialog("GameOverArka", true);
        }
    }

    public void RestartGame()
    {
        StartGame();
    }
}

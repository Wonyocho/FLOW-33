// ReactionGameManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReactionGameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI averageScoreText;
    public TextMeshProUGUI touchToStartText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI countdownText;
    public Button backButton;

    [Header("Main Menu Stats (split)")]
    public TextMeshProUGUI avgText;
    public TextMeshProUGUI bestText;
    public TextMeshProUGUI gamesText;

    [Header("Game Buttons")]
    public Button[] gameButtons;

    [Header("Game Settings")]
    public int totalTargets = 20;
    public int maxScorePerTarget = 1000;

    [Header("Colors")]
    public Color defaultButtonColor = Color.white;
    public Color targetColor = new Color32(0xFF, 0x67, 0x00, 0xFF); // #FF6700
    [SerializeField] private int mainMenuHighlightIndex = 2; // 메인 화면에서 칠할 버튼(기본: Button2)

    // Game State
    private GameState currentState;
    private int currentTarget = 0;
    private int currentActiveButton = -1;
    private float targetStartTime;
    private int currentScore = 0;
    private readonly List<float> reactionTimes = new List<float>();
    private bool isProcessingClick = false; // 클릭 처리 중 재진입 방지

    public enum GameState { MainMenu, Countdown, Playing, GameOver }

    void Start()
    {
        if (gameButtons != null && gameButtons.Length > 0)
        {
            var img0 = GetButtonImage(gameButtons[0]);
            if (img0 != null) defaultButtonColor = img0.color;
        }

        SetState(GameState.MainMenu);
        SetupButtons();
        UpdateAverageScore();

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButton);
    }

    void Update()
    {
        if (currentState == GameState.GameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackButton();
        }
    }

    void SetupButtons()
    {
        if (gameButtons == null) return;

        for (int i = 0; i < gameButtons.Length; i++)
        {
            int buttonIndex = i;
            if (gameButtons[i] == null) continue;

            gameButtons[i].onClick.AddListener(() => OnButtonClicked(buttonIndex));
            gameButtons[i].transform.localScale = Vector3.one;
        }
    }

    void OnButtonClicked(int buttonIndex)
    {
        if (currentState == GameState.MainMenu || currentState == GameState.GameOver)
        {
            StartGame();
            return;
        }

        OnGameButtonClicked(buttonIndex);
    }

    void OnBackButton()
    {
        SetState(GameState.MainMenu);
        UpdateAverageScore();
    }

    public void StartGame()
    {
        if (currentState != GameState.MainMenu && currentState != GameState.GameOver) return;

        SetState(GameState.Countdown);
        StartCoroutine(StartGameCountdown());
    }

    IEnumerator StartGameCountdown()
    {
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            countdownText.gameObject.SetActive(true);

            SoundManager.Instance?.PlayCountdownSound();
            yield return new WaitForSeconds(1f);
        }

        countdownText.gameObject.SetActive(false);
        SoundManager.Instance?.PlayGameStart();

        SetState(GameState.Playing);

        currentTarget = 0;
        currentScore = 0;
        reactionTimes.Clear();
        isProcessingClick = false;

        ActivateRandomButton();
        UpdateScoreUI();
    }

    void ActivateRandomButton()
    {
        if (gameButtons == null || gameButtons.Length == 0) return;

        int tries = 0;
        do
        {
            currentActiveButton = Random.Range(0, gameButtons.Length);
            tries++;
        }
        while (tries < 32 &&
               (gameButtons[currentActiveButton] == null ||
                !gameButtons[currentActiveButton].gameObject.activeInHierarchy));

        if (currentActiveButton < 0 ||
            currentActiveButton >= gameButtons.Length ||
            gameButtons[currentActiveButton] == null ||
            !gameButtons[currentActiveButton].gameObject.activeInHierarchy)
        {
            Debug.LogWarning("ActivateRandomButton: No valid button found.");
            currentActiveButton = -1;
            return;
        }

        targetStartTime = Time.time;

        var animator = gameButtons[currentActiveButton].GetComponent<ButtonAnimator>();
        if (animator != null) animator.SetAsTarget();
        else                  PaintButton(currentActiveButton, targetColor);
    }

    void OnGameButtonClicked(int buttonIndex)
    {
        if (currentState != GameState.Playing) return;
        if (isProcessingClick) return;
        if (currentActiveButton == -1) return;
        if (buttonIndex < 0 || buttonIndex >= gameButtons.Length || gameButtons[buttonIndex] == null) return;

        isProcessingClick = true;
        SoundManager.Instance?.PlayButtonClick();

        if (buttonIndex == currentActiveButton)
        {
            float reactionTime = Time.time - targetStartTime;
            reactionTimes.Add(reactionTime);

            int oldActive = currentActiveButton;
            currentActiveButton = -1;
            ResetButtonColor(oldActive);

            SoundManager.Instance?.PlayCorrectSound();

            int score = Mathf.RoundToInt(maxScorePerTarget * (1f / (reactionTime + 0.1f)) * 0.1f);
            score = Mathf.Clamp(score, 10, maxScorePerTarget);
            currentScore += score;

            currentTarget++;

            if (currentTarget >= totalTargets)
            {
                EndGame();
            }
            else
            {
                ActivateRandomButton();
                UpdateScoreUI();
            }
        }
        else
        {
            int oldActive = currentActiveButton;
            currentActiveButton = -1;

            if (oldActive != -1)
                ResetButtonColor(oldActive);

            SoundManager.Instance?.PlayWrongSound();
            GameOver();
        }

        isProcessingClick = false;
    }

    void ResetButtonColor(int buttonIndex)
    {
        if (gameButtons == null || buttonIndex < 0 || buttonIndex >= gameButtons.Length) return;
        if (gameButtons[buttonIndex] == null) return;

        var animator = gameButtons[buttonIndex].GetComponent<ButtonAnimator>();
        if (animator != null) animator.ResetButton();
        else                  PaintButton(buttonIndex, defaultButtonColor);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"{currentScore}\n";
    }

    void EndGame()
    {
        float averageReactionTime = 0f;
        foreach (float t in reactionTimes) averageReactionTime += t;
        if (reactionTimes.Count > 0) averageReactionTime /= reactionTimes.Count;

        DataManager.Instance?.SaveGameResult(currentScore, averageReactionTime);
        SoundManager.Instance?.PlayGameComplete();

        SetState(GameState.GameOver);
        ShowGameOverScreenImmediate(averageReactionTime);
    }

    void ShowGameOverScreenImmediate(float avgReactionTime)
    {
        SafeSetActive(averageScoreText, true);

        if (titleText != null)        titleText.text        = $"{currentScore}";
        if (averageScoreText != null) averageScoreText.text = $"Avg Reaction: {avgReactionTime:F3}s";
        if (touchToStartText != null) touchToStartText.text = "Touch Button to play again";
    }

    void GameOver()
    {
        SetState(GameState.GameOver);
        SoundManager.Instance?.PlayGameOver();

        if (titleText != null)        titleText.text        = $"{currentScore}";
        if (averageScoreText != null) averageScoreText.text = string.Empty;
        if (touchToStartText != null) touchToStartText.text = "Touch Button to try again";

        if (currentActiveButton != -1)
        {
            ResetButtonColor(currentActiveButton);
            currentActiveButton = -1;
        }
    }

    void SetState(GameState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                SafeSetActive(titleText, true);
                SafeSetActive(touchToStartText, true);
                SafeSetActive(scoreText, false);
                SafeSetActive(countdownText, false);

                SafeSetActive(averageScoreText, false);

                SafeSetActive(avgText,  true);
                SafeSetActive(bestText, true);
                SafeSetActive(gamesText,false);

                SetButtonsActive(true);
                ResetAllButtons();
                SetTransitionMode(false);
                 HighlightMainMenuButton(mainMenuHighlightIndex);
                if (titleText != null) titleText.text = "FLOW";
                if (touchToStartText != null) touchToStartText.text = "Touch any button to start";
                SafeSetActive(backButton, false);
                break;

            case GameState.Countdown:
                SafeSetActive(titleText, false);
                SafeSetActive(touchToStartText, false);
                SafeSetActive(scoreText, false);
                SafeSetActive(countdownText, true);

                SafeSetActive(avgText,  false);
                SafeSetActive(bestText, false);
                SafeSetActive(gamesText,false);
                SafeSetActive(averageScoreText, false);

                SetButtonsActive(true);
                ResetAllButtons();
                SetTransitionMode(false);
                SafeSetActive(backButton, false);
                break;

            case GameState.Playing:
                SafeSetActive(titleText, false);
                SafeSetActive(touchToStartText, false);
                SafeSetActive(scoreText, true);
                SafeSetActive(countdownText, false);

                SafeSetActive(avgText,  false);
                SafeSetActive(bestText, false);
                SafeSetActive(gamesText,false);
                SafeSetActive(averageScoreText, false);

                SetButtonsActive(true);
                SetTransitionMode(true);
                UpdateScoreUI();
                SafeSetActive(backButton, false);
                break;

            case GameState.GameOver:
                SafeSetActive(scoreText, false);
                SetButtonsActive(true);
                SafeSetActive(titleText, true);

                SafeSetActive(averageScoreText, false);

                SafeSetActive(avgText,  false);
                SafeSetActive(bestText, false);
                SafeSetActive(gamesText,false);

                SafeSetActive(touchToStartText, true);
                SetTransitionMode(false);
                SafeSetActive(backButton, true);
                break;
        }
    }

    void SetButtonsActive(bool active)
    {
        if (gameButtons == null) return;
        foreach (Button b in gameButtons)
        {
            if (b != null)
                b.gameObject.SetActive(active);
        }
    }

    void ResetAllButtons()
    {
        if (gameButtons == null) return;

        for (int i = 0; i < gameButtons.Length; i++)
        {
            if (gameButtons[i] == null) continue;

            var animator = gameButtons[i].GetComponent<ButtonAnimator>();
            if (animator != null) animator.ResetButton();
            else                  PaintButton(i, defaultButtonColor);
        }
        currentActiveButton = -1;
    }

    void UpdateAverageScore()
    {
        bool hasSplit = (avgText != null) || (bestText != null) || (gamesText != null);

        if (DataManager.Instance != null)
        {
            float avgScore   = DataManager.Instance.GetAverageScore();
            int   gamesPlayed= DataManager.Instance.GetTotalGamesPlayed();
            int   best       = DataManager.Instance.GetBestScore();

            if (hasSplit)
            {
                if (avgText   != null) avgText.text   = (gamesPlayed == 0) ? "Average: —" : $"Average: {avgScore:F0}";
                if (bestText  != null) bestText.text  = (gamesPlayed == 0) ? "Best: —"    : $"Best: {best}";
                if (gamesText != null && gamesText.gameObject.activeInHierarchy)
                    gamesText.text = $"Games: {gamesPlayed}";
            }
            else if (averageScoreText != null)
            {
                if (gamesPlayed == 0) averageScoreText.text = "No games played yet";
                else averageScoreText.text = $"Average: {avgScore:F0} | Best: {best}\nGames: {gamesPlayed}";
            }
        }
        else
        {
            if (hasSplit)
            {
                if (avgText   != null) avgText.text   = "Average: —";
                if (bestText  != null) bestText.text  = "Best: —";
                if (gamesText != null) gamesText.text = "Games: 0";
            }
            else if (averageScoreText != null)
            {
                averageScoreText.text = "Average Score: No games played";
            }
        }
    }

    private void HighlightMainMenuButton(int idx)
    {
        if (gameButtons == null || idx < 0 || idx >= gameButtons.Length) return;
        var btn = gameButtons[idx];
        if (btn == null) return;

        btn.transition = Selectable.Transition.None;

        var animator = btn.GetComponent<ButtonAnimator>();
        if (animator != null) animator.ResetButton();

        PaintButton(idx, targetColor);
    }

    private Image GetButtonImage(int idx)
    {
        if (gameButtons == null || idx < 0 || idx >= gameButtons.Length) return null;
        return GetButtonImage(gameButtons[idx]);
    }

    private Image GetButtonImage(Button b)
    {
        if (b == null) return null;
        return b.GetComponent<Image>() ?? b.targetGraphic as Image;
    }

    private void PaintButton(int idx, Color color)
    {
        var img = GetButtonImage(idx);
        if (img != null) img.color = color;
    }

    private void SetTransitionMode(bool playing)
    {
        if (gameButtons == null) return;
        foreach (var b in gameButtons)
        {
            if (b == null) continue;
            b.transition = playing ? Selectable.Transition.None : Selectable.Transition.ColorTint;
        }
    }

    private void SafeSetActive(Behaviour comp, bool active)
    {
        if (comp != null) comp.gameObject.SetActive(active);
    }
}

// DataManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public List<int> scores = new List<int>();
    public List<float> averageReactionTimes = new List<float>();
    public int totalGamesPlayed;
    public int bestScore;
    public float bestAverageReactionTime;
    public DateTime lastPlayedDate;
    
    public GameData()
    {
        scores = new List<int>();
        averageReactionTimes = new List<float>();
        totalGamesPlayed = 0;
        bestScore = 0;
        bestAverageReactionTime = float.MaxValue;
        lastPlayedDate = DateTime.Now;
    }
}

public class DataManager : MonoBehaviour
{
    private static DataManager instance;
    public static DataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<DataManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("DataManager");
                    instance = go.AddComponent<DataManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    private GameData gameData;
    private const string SAVE_KEY = "ReactionGameData";
    private const int MAX_STORED_RECORDS = 100;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void SaveGameResult(int score, float averageReactionTime)
    {
        gameData.scores.Add(score);
        gameData.averageReactionTimes.Add(averageReactionTime);
        gameData.totalGamesPlayed++;
        gameData.lastPlayedDate = DateTime.Now;
        
        // 최고 기록 업데이트
        if (score > gameData.bestScore)
        {
            gameData.bestScore = score;
        }
        
        if (averageReactionTime < gameData.bestAverageReactionTime)
        {
            gameData.bestAverageReactionTime = averageReactionTime;
        }
        
        // 최근 100개 기록만 유지
        if (gameData.scores.Count > MAX_STORED_RECORDS)
        {
            gameData.scores.RemoveAt(0);
            gameData.averageReactionTimes.RemoveAt(0);
        }
        
        SaveGameData();
    }
    
    public float GetAverageScore()
    {
        if (gameData.scores.Count == 0) return 0f;
        
        float total = 0f;
        foreach (int score in gameData.scores)
        {
            total += score;
        }
        return total / gameData.scores.Count;
    }
    
    public float GetAverageReactionTime()
    {
        if (gameData.averageReactionTimes.Count == 0) return 0f;
        
        float total = 0f;
        foreach (float time in gameData.averageReactionTimes)
        {
            total += time;
        }
        return total / gameData.averageReactionTimes.Count;
    }
    
    public int GetBestScore()
    {
        return gameData.bestScore;
    }
    
    public float GetBestReactionTime()
    {
        return gameData.bestAverageReactionTime == float.MaxValue ? 0f : gameData.bestAverageReactionTime;
    }
    
    public int GetTotalGamesPlayed()
    {
        return gameData.totalGamesPlayed;
    }
    
    public List<int> GetRecentScores(int count = 10)
    {
        List<int> recentScores = new List<int>();
        int startIndex = Mathf.Max(0, gameData.scores.Count - count);
        
        for (int i = startIndex; i < gameData.scores.Count; i++)
        {
            recentScores.Add(gameData.scores[i]);
        }
        
        return recentScores;
    }
    
    public DateTime GetLastPlayedDate()
    {
        return gameData.lastPlayedDate;
    }
    
    public void ResetAllData()
    {
        gameData = new GameData();
        SaveGameData();
    }
    
    private void SaveGameData()
    {
        try
        {
            string jsonData = JsonUtility.ToJson(gameData);
            PlayerPrefs.SetString(SAVE_KEY, jsonData);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game data: {e.Message}");
        }
    }
    
    private void LoadGameData()
    {
        try
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string jsonData = PlayerPrefs.GetString(SAVE_KEY);
                gameData = JsonUtility.FromJson<GameData>(jsonData);
                
                // 데이터 무결성 확인
                if (gameData.scores == null)
                    gameData.scores = new List<int>();
                if (gameData.averageReactionTimes == null)
                    gameData.averageReactionTimes = new List<float>();
            }
            else
            {
                gameData = new GameData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game data: {e.Message}");
            gameData = new GameData();
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGameData();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGameData();
        }
    }
}

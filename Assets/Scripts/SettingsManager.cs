// SettingsManager.cs
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Settings UI")]
    public GameObject settingsPanel;
    public Button settingsButton;
    public Button closeSettingsButton;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;
    public Toggle vibrateToggle;
    public Button resetDataButton;
    
    [Header("Confirmation Dialog")]
    public GameObject confirmationDialog;
    public Button confirmResetButton;
    public Button cancelResetButton;
    
    // Settings Keys
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string VIBRATE_KEY = "VibrateEnabled";
    
    private bool vibrateEnabled = true;
    
    void Start()
    {
        SetupUI();
        LoadSettings();
    }
    
    void SetupUI()
    {
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
            
        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            
        if (vibrateToggle != null)
            vibrateToggle.onValueChanged.AddListener(SetVibrateEnabled);
            
        if (resetDataButton != null)
            resetDataButton.onClick.AddListener(ShowResetConfirmation);
            
        if (confirmResetButton != null)
            confirmResetButton.onClick.AddListener(ResetAllData);
            
        if (cancelResetButton != null)
            cancelResetButton.onClick.AddListener(HideResetConfirmation);
        
        // 초기 UI 상태 설정
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
    }
    
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            
            // 사운드 재생
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayButtonClick();
            }
        }
    }
    
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            
            // 사운드 재생
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayButtonClick();
            }
        }
        
        SaveSettings();
    }
    
    public void SetSFXVolume(float volume)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(volume);
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMusicVolume(volume);
        }
    }
    
    public void SetVibrateEnabled(bool enabled)
    {
        vibrateEnabled = enabled;
        
        // 진동 테스트
        if (enabled)
        {
            TriggerVibration();
        }
    }
    
    public void TriggerVibration()
    {
        if (vibrateEnabled)
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
    
    public void ShowResetConfirmation()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(true);
            
            // 사운드 재생
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayButtonClick();
            }
        }
    }
    
    public void HideResetConfirmation()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
            
            // 사운드 재생
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayButtonClick();
            }
        }
    }
    
    public void ResetAllData()
    {
        // 데이터 초기화
        if (DataManager.Instance != null)
        {
            DataManager.Instance.ResetAllData();
        }
        
        // 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButtonClick();
        }
        
        // 확인 다이얼로그 닫기
        HideResetConfirmation();
        
        // 설정 패널 닫기
        CloseSettings();
        
        // 게임 매니저에게 UI 업데이트 요청
        ReactionGameManager gameManager = FindFirstObjectByType<ReactionGameManager>();
        if (gameManager != null)
        {
            // 평균 점수 업데이트를 위해 게임 매니저의 UpdateAverageScore 메서드 호출
            gameManager.SendMessage("UpdateAverageScore", SendMessageOptions.DontRequireReceiver);
        }
    }
    
    void SaveSettings()
    {
        if (sfxVolumeSlider != null)
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolumeSlider.value);
            
        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolumeSlider.value);
            
        PlayerPrefs.SetInt(VIBRATE_KEY, vibrateEnabled ? 1 : 0);
        
        PlayerPrefs.Save();
    }
    
    void LoadSettings()
    {
        // SFX 볼륨 로드
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.7f);
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
            SetSFXVolume(sfxVolume);
        }
        
        // 음악 볼륨 로드
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.3f);
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
            SetMusicVolume(musicVolume);
        }
        
        // 진동 설정 로드
        vibrateEnabled = PlayerPrefs.GetInt(VIBRATE_KEY, 1) == 1;
        if (vibrateToggle != null)
        {
            vibrateToggle.isOn = vibrateEnabled;
        }
    }
    
    // 외부에서 호출할 수 있는 진동 메서드
    public static void Vibrate()
    {
        SettingsManager settingsManager = FindFirstObjectByType<SettingsManager>();
        if (settingsManager != null)
        {
            settingsManager.TriggerVibration();
        }
    }
}

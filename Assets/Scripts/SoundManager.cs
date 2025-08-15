// SoundManager.cs
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;
    
    [Header("Sound Effects")]
    public AudioClip buttonClickSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip countdownSound;
    public AudioClip gameStartSound;
    public AudioClip gameOverSound;
    public AudioClip gameCompleteSound;
    
    [Header("Background Music")]
    public AudioClip backgroundMusic;
    
    [Header("Settings")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;
    [Range(0f, 1f)]
    public float musicVolume = 0.3f;
    
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SoundManager>();
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        SetupAudioSources();
        PlayBackgroundMusic();
    }
    
    void SetupAudioSources()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        
        sfxSource.volume = sfxVolume;
        musicSource.volume = musicVolume;
        musicSource.loop = true;
    }
    
    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }
    
    public void PlayCorrectSound()
    {
        PlaySFX(correctSound);
    }
    
    public void PlayWrongSound()
    {
        PlaySFX(wrongSound);
    }
    
    public void PlayCountdownSound()
    {
        PlaySFX(countdownSound);
    }
    
    public void PlayGameStart()
    {
        PlaySFX(gameStartSound);
    }
    
    public void PlayGameOver()
    {
        PlaySFX(gameOverSound);
    }
    
    public void PlayGameComplete()
    {
        PlaySFX(gameCompleteSound);
    }
    
    void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    
    void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }
    
    public void ToggleSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.mute = !sfxSource.mute;
        }
    }
    
    public void ToggleMusic()
    {
        if (musicSource != null)
        {
            musicSource.mute = !musicSource.mute;
        }
    }
}

// MobileOptimizer.cs
using UnityEngine;

public class MobileOptimizer : MonoBehaviour
{
    [Header("Performance Settings")]
    public int targetFrameRate = 60;
    public bool optimizeForMobile = true;
    
    [Header("Screen Settings")]
    public bool preventScreenSleep = true;
    public ScreenOrientation forceOrientation = ScreenOrientation.Portrait;
    
    void Start()
    {
        OptimizeForMobile();
        SetScreenSettings();
    }
    
    void OptimizeForMobile()
    {
        if (!optimizeForMobile) return;
        
        // 프레임 레이트 설정
        Application.targetFrameRate = targetFrameRate;
        
        // 화면 품질 설정 (모바일 최적화)
#if UNITY_ANDROID || UNITY_IOS
        // 모바일에서 배터리 절약을 위한 설정
        QualitySettings.vSyncCount = 0;
        
        // Android 특정 최적화
#if UNITY_ANDROID
        // GPU 스킨닝 활성화 (성능 향상)
        QualitySettings.skinWeights = SkinWeights.TwoBones;
#endif
        
        // iOS 특정 최적화
#if UNITY_IOS
        // 저전력 모드에서도 부드러운 실행
        QualitySettings.antiAliasing = 0;
#endif
#endif
    }
    
    void SetScreenSettings()
    {
        // 화면 꺼짐 방지
        if (preventScreenSleep)
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        
        // 화면 방향 고정
        if (forceOrientation != ScreenOrientation.AutoRotation)
        {
            Screen.orientation = forceOrientation;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = forceOrientation == ScreenOrientation.Portrait;
            Screen.autorotateToPortraitUpsideDown = false;
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        // 게임이 백그라운드로 갈 때 처리
        if (pauseStatus)
        {
            // 성능 절약을 위해 프레임 레이트 낮춤
            Application.targetFrameRate = 30;
        }
        else
        {
            // 게임이 포그라운드로 돌아올 때 원래 프레임 레이트로 복원
            Application.targetFrameRate = targetFrameRate;
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        // 포커스 변경 시 처리
        if (!hasFocus)
        {
            Time.timeScale = 0f; // 게임 일시정지
        }
        else
        {
            Time.timeScale = 1f; // 게임 재개
        }
    }
    
    // 메모리 사용량 모니터링 (개발용)
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.M))
        {
            LogMemoryUsage();
        }
#endif
    }
    
    void LogMemoryUsage()
    {
        long memoryUsage = System.GC.GetTotalMemory(false);
        Debug.Log($"Memory Usage: {memoryUsage / 1024 / 1024} MB");
        
        // 강제 가비지 컬렉션 (개발용)
        System.GC.Collect();
    }
}

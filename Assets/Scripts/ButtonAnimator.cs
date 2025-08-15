// ButtonAnimator.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonAnimator : MonoBehaviour
{
    private Button button;
    private Vector3 originalScale;
    private Image buttonImage;
    private Color originalColor;
    private Coroutine currentAnimation;
    private Coroutine pulseAnimation;
    
    [Header("Animation Settings")]
    public float scaleMultiplier = 1.05f; // 더 작은 스케일
    public float animationDuration = 0.03f; // 매우 빠른 클릭 애니메이션
    public float glowDuration = 0.2f; // 더 빠른 펄스
    public float pulseScale = 1.02f; // 더 작은 펄스
    
    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        originalScale = transform.localScale;
        
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        
        // onClick 이벤트는 ReactionGameManager에서 처리하므로 여기서는 등록하지 않음
    }
    
    public void PlayClickAnimation()
    {
        // 기존 애니메이션 중지
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        // 버튼 클릭 애니메이션 시작
        currentAnimation = StartCoroutine(ClickAnimation());
    }
    
    void OnButtonClick()
    {
        PlayClickAnimation();
    }
    
    IEnumerator ClickAnimation()
    {
        // 확대 애니메이션
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = originalScale * scaleMultiplier;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            t = EaseOutQuad(t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        // 축소 애니메이션
        elapsedTime = 0f;
        startScale = transform.localScale;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            t = EaseInQuad(t);
            transform.localScale = Vector3.Lerp(startScale, originalScale, t);
            yield return null;
        }
        
        transform.localScale = originalScale;
        currentAnimation = null;
    }
    
    public void SetAsTarget()
    {
        // 모든 기존 애니메이션 즉시 중지
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
        
        if (pulseAnimation != null)
        {
            StopCoroutine(pulseAnimation);
            pulseAnimation = null;
        }
        
        if (buttonImage != null)
        {
            buttonImage.color = new Color32(0xFF, 0x67, 0x00, 0xFF); // #FF6700
        }
        
        // 스케일도 즉시 원래 크기로 복원
        transform.localScale = originalScale;
        
        // 펄스 효과 시작
        pulseAnimation = StartCoroutine(PulseAnimation());
    }
    
    public void ResetButton()
    {
        // 모든 애니메이션 즉시 중지
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
        
        if (pulseAnimation != null)
        {
            StopCoroutine(pulseAnimation);
            pulseAnimation = null;
        }
        
        // 즉시 원래 상태로 복원 (애니메이션 없음)
        transform.localScale = originalScale;
        
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }
    }
    
    IEnumerator ColorTransition(Color fromColor, Color toColor, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            if (buttonImage != null)
            {
                buttonImage.color = Color.Lerp(fromColor, toColor, t);
            }
            
            yield return null;
        }
        
        if (buttonImage != null)
        {
            buttonImage.color = toColor;
        }
    }
    
    IEnumerator PulseAnimation()
    {
        Vector3 smallScale = originalScale;
        Vector3 largeScale = originalScale * pulseScale;
        
        while (true)
        {
            // 확대
            float elapsedTime = 0f;
            while (elapsedTime < glowDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / glowDuration;
                t = EaseInOutSine(t);
                transform.localScale = Vector3.Lerp(smallScale, largeScale, t);
                yield return null;
            }
            
            // 축소
            elapsedTime = 0f;
            while (elapsedTime < glowDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / glowDuration;
                t = EaseInOutSine(t);
                transform.localScale = Vector3.Lerp(largeScale, smallScale, t);
                yield return null;
            }
        }
    }
    
    // Easing 함수들
    float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
    
    float EaseInQuad(float t)
    {
        return t * t;
    }
    
    float EaseInOutSine(float t)
    {
        return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
    }
    
    void OnDestroy()
    {
        // 모든 코루틴 정리
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        if (pulseAnimation != null)
        {
            StopCoroutine(pulseAnimation);
        }
    }
}

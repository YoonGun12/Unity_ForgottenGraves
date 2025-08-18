using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ria : MonoBehaviour
{
    [Header("유령 효과 설정")]
    [SerializeField] private bool enableGhostEffect = true;
    [SerializeField] private float fadeSpeed = 2f;              // 투명도 변화 속도
    [SerializeField] private float minAlpha = 0.3f;             // 최소 투명도
    [SerializeField] private float maxAlpha = 1f;               // 최대 투명도
    
    [Header("떠다니는 효과")]
    [SerializeField] private bool enableFloating = true;
    [SerializeField] private float floatSpeed = 1f;             // 떠다니는 속도
    [SerializeField] private float floatHeight = 0.2f;          // 떠다니는 높이
    
    [Header("깜빡이는 효과")]
    [SerializeField] private bool enableFlickering = false;
    [SerializeField] private float flickerInterval = 3f;        // 깜빡이는 간격
    [SerializeField] private float flickerDuration = 0.1f;      // 깜빡이는 지속 시간

    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition;
    private float fadeDirection = 1f;                           // 투명도 변화 방향
    private float floatOffset = 0f;                             // 떠다니는 오프셋
    private bool isFlickering = false;
    private bool isVisible = true;

    // 외부에서 제어할 수 있는 상태
    private bool ghostEffectActive = true;
    private bool isAppearing = false;
    private bool isDisappearing = false;

    private bool isMoving = false;
    private bool isNormalCharacterMode = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.position;
        
        if (enableFlickering)
        {
            StartCoroutine(FlickerRoutine());
        }
    }

    void Update()
    {
        // ✅ 수정: 일반 캐릭터 모드에서는 떠다니는 효과만 비활성화
        // 애니메이션 시스템은 자유롭게 작동하도록 허용
        if (isNormalCharacterMode)
        {
            // 일반 캐릭터 모드에서는 유령 효과만 비활성화
            // transform.position은 애니메이션 시스템이 자유롭게 제어
            return;
        }
        
        if (isMoving)
        {
            // 이동 중에는 떠다니는 효과만 비활성화
            // 투명도 효과는 유지 (필요시)
            if (ghostEffectActive && enableGhostEffect && !isAppearing && !isDisappearing)
            {
                UpdateFadeEffect();
            }
            return;
        }
        
        if (ghostEffectActive)
        {
            if (enableGhostEffect && !isAppearing && !isDisappearing)
            {
                UpdateFadeEffect();
            }
            
            if (enableFloating)
            {
                UpdateFloatingEffect();
            }
        }
    }

    #region 유령 효과

    /// <summary>
    /// 투명도 변화 효과
    /// </summary>
    private void UpdateFadeEffect()
    {
        if (spriteRenderer == null || isFlickering) return;

        Color color = spriteRenderer.color;
        color.a += fadeDirection * fadeSpeed * Time.deltaTime;

        if (color.a >= maxAlpha)
        {
            color.a = maxAlpha;
            fadeDirection = -1f;
        }
        else if (color.a <= minAlpha)
        {
            color.a = minAlpha;
            fadeDirection = 1f;
        }

        spriteRenderer.color = color;
    }

    /// <summary>
    /// 떠다니는 효과
    /// </summary>
    private void UpdateFloatingEffect()
    {
        floatOffset += floatSpeed * Time.deltaTime;
        float yOffset = Mathf.Sin(floatOffset) * floatHeight;
        transform.position = originalPosition + new Vector3(0, yOffset, 0);
    }

    /// <summary>
    /// 깜빡이는 효과 코루틴
    /// </summary>
    private IEnumerator FlickerRoutine()
    {
        while (enableFlickering)
        {
            yield return new WaitForSeconds(flickerInterval);
            
            if (ghostEffectActive && !isAppearing && !isDisappearing && !isMoving && !isNormalCharacterMode)
            {
                yield return StartCoroutine(Flicker());
            }
        }
    }

    /// <summary>
    /// 깜빡이는 효과
    /// </summary>
    private IEnumerator Flicker()
    {
        isFlickering = true;
        
        // 잠깐 사라지기
        SetVisible(false);
        yield return new WaitForSeconds(flickerDuration);
        
        // 다시 나타나기
        SetVisible(true);
        yield return new WaitForSeconds(flickerDuration);
        
        isFlickering = false;
    }

    #endregion

    #region 공개 메서드 (외부 제어용)
    
    /// <summary>
    /// ✅ 수정: 일반 캐릭터 모드 설정
    /// </summary>
    public void SetNormalCharacterMode(bool enabled)
    {
        isNormalCharacterMode = enabled;
        
        if (enabled)
        {
            
            // 유령 효과만 비활성화, 위치 제어는 애니메이션 시스템에 맡김
            ghostEffectActive = false;
            
            // 투명도를 1로 고정
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f;
                spriteRenderer.color = color;
            }
        }
        else
        {
            // 유령 모드로 복귀
            ghostEffectActive = true;
            originalPosition = transform.position;
        }
    }
    
    /// <summary>
    /// ✅ 수정: 이동 모드 설정
    /// </summary>
    public void SetMoving(bool moving)
    {
        isMoving = moving;
        
        if (!moving)
        {
            // 이동이 끝나면 현재 위치를 원래 위치로 설정
            originalPosition = transform.position;
        }
    }
    
    /// <summary>
    /// 유령 효과 활성화/비활성화
    /// </summary>
    public void SetGhostEffectActive(bool active)
    {
        ghostEffectActive = active;
        
        if (!active && spriteRenderer != null)
        {
            // 효과 비활성화시 완전히 보이게
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }

    /// <summary>
    /// 서서히 나타나는 효과
    /// </summary>
    public void AppearGradually(float duration = 2f)
    {
        StartCoroutine(AppearCoroutine(duration));
    }

    /// <summary>
    /// 서서히 사라지는 효과
    /// </summary>
    public void DisappearGradually(float duration = 2f)
    {
        StartCoroutine(DisappearCoroutine(duration));
    }

    /// <summary>
    /// 즉시 보이기/숨기기
    /// </summary>
    public void SetVisible(bool visible)
    {
        isVisible = visible;
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = visible ? maxAlpha : 0f;
            spriteRenderer.color = color;
        }
    }

    /// <summary>
    /// 위치 재설정 (떠다니는 효과 기준점 변경)
    /// </summary>
    public void SetOriginalPosition(Vector3 newPosition)
    {
        originalPosition = newPosition;
    }

    #endregion

    #region 코루틴

    /// <summary>
    /// 나타나는 효과 코루틴
    /// </summary>
    private IEnumerator AppearCoroutine(float duration)
    {
        isAppearing = true;
        
        if (spriteRenderer == null) yield break;
        
        Color startColor = spriteRenderer.color;
        startColor.a = 0f;
        spriteRenderer.color = startColor;
        
        Color endColor = startColor;
        endColor.a = maxAlpha;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color currentColor = Color.Lerp(startColor, endColor, elapsedTime / duration);
            spriteRenderer.color = currentColor;
            yield return null;
        }
        
        spriteRenderer.color = endColor;
        isAppearing = false;
    }

    /// <summary>
    /// 사라지는 효과 코루틴
    /// </summary>
    private IEnumerator DisappearCoroutine(float duration)
    {
        isDisappearing = true;
        
        if (spriteRenderer == null) yield break;
        
        Color startColor = spriteRenderer.color;
        Color endColor = startColor;
        endColor.a = 0f;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color currentColor = Color.Lerp(startColor, endColor, elapsedTime / duration);
            spriteRenderer.color = currentColor;
            yield return null;
        }
        
        spriteRenderer.color = endColor;
        isDisappearing = false;
    }

    #endregion

    #region 설정 메서드

    /// <summary>
    /// 투명도 범위 설정
    /// </summary>
    public void SetAlphaRange(float min, float max)
    {
        minAlpha = Mathf.Clamp01(min);
        maxAlpha = Mathf.Clamp01(max);
    }

    /// <summary>
    /// 페이드 속도 설정
    /// </summary>
    public void SetFadeSpeed(float speed)
    {
        fadeSpeed = Mathf.Max(0f, speed);
    }

    /// <summary>
    /// 떠다니는 효과 설정
    /// </summary>
    public void SetFloatingSettings(float speed, float height)
    {
        floatSpeed = speed;
        floatHeight = height;
    }

    /// <summary>
    /// 깜빡이는 효과 토글
    /// </summary>
    public void SetFlickeringEnabled(bool enabled)
    {
        enableFlickering = enabled;
        
        if (enabled && !isFlickering)
        {
            StartCoroutine(FlickerRoutine());
        }
    }

    #endregion

    #region 유틸리티

    /// <summary>
    /// 현재 상태 확인
    /// </summary>
    public bool IsVisible() => isVisible;
    public bool IsAppearing() => isAppearing;
    public bool IsDisappearing() => isDisappearing;
    public bool IsGhostEffectActive() => ghostEffectActive;
    public bool IsMoving() => isMoving;

    #endregion
}
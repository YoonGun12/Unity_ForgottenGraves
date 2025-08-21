using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 프롤로그 씬들의 공통 기능을 담당하는 베이스 컨트롤러
/// </summary>
public abstract class Prologue : MonoBehaviour
{
    [Header("기본 설정")]
    [SerializeField] protected string nextSceneName;
    [SerializeField] protected float sceneTransitionDelay = 2f;
    [SerializeField] protected int prologueCSVIndex;

    [Header("캐릭터 이동")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected Transform[] waypoints;

    [Header("페이드 효과")]
    [SerializeField] protected Image fadeImage;
    [SerializeField] protected float fadeDuration = 3f;

    [Header("캐릭터들")]
    [SerializeField] protected Transform lyle;
    [SerializeField] protected Transform granpa;
    [SerializeField] protected Transform ria;
    [SerializeField] protected Animator lyleAnimator;
    [SerializeField] protected Animator granpaAnimator;
    [SerializeField] protected Animator riaAnimator;

    protected bool isPlaying = false;
    protected int currentSequence = 0;

    #region 초기화

    protected virtual void Start()
    {
        InitializeFadeImage();
        InitializeScene();
        StartCoroutine(WaitForDatabaseAndStart());
    }

    protected virtual void InitializeFadeImage()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;
        }
    }

    /// <summary>
    /// 각 프롤로그별 특수 초기화
    /// </summary>
    protected abstract void InitializeScene();

    protected IEnumerator WaitForDatabaseAndStart()
    {
        yield return new WaitUntil(() => DialogueDatabase.Instance != null);
        yield return new WaitUntil(() => DialogueDatabase.Instance.IsLoaded);
        yield return new WaitUntil(() => DialogueManager.Instance != null);
        yield return StartCoroutine(CheckAndSetupDialogueUI());
        
        SubscribeToEvents();
        StartPrologueSequence();
    }

    protected IEnumerator CheckAndSetupDialogueUI()
    {
        DialogueUI dialogueUI = FindObjectOfType<DialogueUI>();
        if (dialogueUI == null)
        {
            Debug.LogError($"DialogueUI를 찾을 수 없습니다. {GetType().Name} 씬에 DialogueUI가 있는지 확인하세요.");
            yield break;
        }
        
        if (DialogueManager.Instance != null)
        {
            yield return new WaitForEndOfFrame();
        }
    }

    protected virtual void SubscribeToEvents()
    {
        DialogueManager.OnDialogueCompleted += OnDialogueCompleted;
        DialogueManager.OnEventFlagTriggered += OnEventFlagTriggered;
    }

    protected virtual void OnDestroy()
    {
        DialogueManager.OnDialogueCompleted -= OnDialogueCompleted;
        DialogueManager.OnEventFlagTriggered -= OnEventFlagTriggered;
    }

    #endregion

    #region 프롤로그 진행

    protected void StartPrologueSequence()
    {
        if (isPlaying) return;
        isPlaying = true;
        currentSequence = 0;
        StartCoroutine(PlayPrologueSequence());
    }

    /// <summary>
    /// 각 프롤로그별 시퀀스 구현 (오버라이드 필요)
    /// </summary>
    protected abstract IEnumerator PlayPrologueSequence();

    #endregion

    #region 캐릭터 이동 및 애니메이션

    /// <summary>
    /// 캐릭터를 특정 지점으로 이동 (격자 기반)
    /// </summary>
    protected IEnumerator MoveCharacterToPoint(Transform character, Transform targetPoint, Animator animator)
    {
        if (character == null || targetPoint == null) yield break;

        Vector2 startPos = character.position;
        Vector2 targetPos = targetPoint.position;
        
        // 수평 이동
        if (Mathf.Abs(startPos.x - targetPos.x) > 0.1f)
        {
            yield return StartCoroutine(MoveHorizontal(character, targetPos.x, animator));
        }
        
        // 수직 이동
        if (Mathf.Abs(character.position.y - targetPos.y) > 0.1f)
        {
            yield return StartCoroutine(MoveVertical(character, targetPos.y, animator));
        }
        
        character.position = targetPoint.position;
        
        if (animator != null)
        {
            animator.SetBool("isChange", false); 
            animator.SetInteger("h", 0);
            animator.SetInteger("v", 0);
        }
    }

    protected IEnumerator MoveHorizontal(Transform character, float targetX, Animator animator)
    {
        Vector2 direction = Vector2.right * Mathf.Sign(targetX - character.position.x);
        
        if (animator != null)
        {
            animator.SetInteger("h", (int)direction.x);
            animator.SetInteger("v", 0);
            animator.SetBool("isChange", true);
        }
        
        while (Mathf.Abs(character.position.x - targetX) > 0.1f)
        {
            Vector2 newPos = character.position;
            newPos.x = Mathf.MoveTowards(newPos.x, targetX, moveSpeed * Time.deltaTime);
            character.position = newPos;
            yield return null;
        }
        
        Vector2 correctedPos = character.position;
        correctedPos.x = targetX;
        character.position = correctedPos;
    }

    protected IEnumerator MoveVertical(Transform character, float targetY, Animator animator)
    {
        Vector2 direction = Vector2.up * Mathf.Sign(targetY - character.position.y);
        
        if (animator != null)
        {
            animator.SetInteger("h", 0);
            animator.SetInteger("v", (int)direction.y);
            animator.SetBool("isChange", true);
        }
        
        while (Mathf.Abs(character.position.y - targetY) > 0.1f)
        {
            Vector2 newPos = character.position;
            newPos.y = Mathf.MoveTowards(newPos.y, targetY, moveSpeed * Time.deltaTime);
            character.position = newPos;
            yield return null;
        }
        
        Vector2 correctedPos = character.position;
        correctedPos.y = targetY;
        character.position = correctedPos;
    }

    /// <summary>
    /// 캐릭터 방향 설정
    /// </summary>
    protected IEnumerator SetCharacterDirection(Animator animator, string direction)
    {
        if (animator == null) yield break;

        switch (direction.ToLower())
        {
            case "left":
                animator.SetInteger("h", -1);
                animator.SetInteger("v", 0);
                break;
            case "right":
                animator.SetInteger("h", 1);
                animator.SetInteger("v", 0);
                break;
            case "up":
                animator.SetInteger("h", 0);
                animator.SetInteger("v", 1);
                break;
            case "down":
                animator.SetInteger("h", 0);
                animator.SetInteger("v", -1);
                break;
            default:
                yield break;
        }

        animator.SetBool("isChange", true);
        yield return new WaitForSeconds(0.1f);
        animator.SetInteger("h", 0);
        animator.SetInteger("v", 0);
    }

    #endregion

    #region 특별 효과

    /// <summary>
    /// 캐릭터를 서서히 투명하게 (임종 표현용)
    /// </summary>
    protected IEnumerator FadeOutCharacter(Transform character, float duration)
    {
        if (character == null) yield break;
        
        SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;
        
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / duration);
            yield return null;
        }
        
        spriteRenderer.color = endColor;
        character.gameObject.SetActive(false);
    }

    /// <summary>
    /// 캐릭터를 서서히 나타나게
    /// </summary>
    protected IEnumerator FadeInCharacter(Transform character, float duration)
    {
        if (character == null) yield break;
        
        SpriteRenderer spriteRenderer = character.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;
        
        Color startColor = spriteRenderer.color;
        startColor.a = 0f;
        spriteRenderer.color = startColor;
        
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / duration);
            yield return null;
        }
        
        spriteRenderer.color = endColor;
    }

    #endregion

    #region 페이드 효과

    /// <summary>
    /// 페이드 인 (검은화면 → 투명)
    /// </summary>
    protected IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
    
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    
        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 페이드 아웃 (투명 → 검은화면)
    /// </summary>
    protected IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
    
        float elapsedTime = 0f;
        color.a = 0f;
        fadeImage.color = color;
    
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    
        color.a = 1f;
        fadeImage.color = color;
    }

    #endregion

    #region 사용자 입력

    protected IEnumerator WaitForUserInput()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Space) || 
                Input.GetKeyDown(KeyCode.Return) || 
                Input.GetMouseButtonDown(0))
            {
                break;
            }
            yield return null;
        }
    }

    protected IEnumerator WaitForUserInputThenFadeOut()
    {
        yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        yield return StartCoroutine(WaitForUserInput());
        yield return StartCoroutine(FadeOut());
    }

    #endregion

    #region 대화 헬퍼

    /// <summary>
    /// 대화 시작 헬퍼 메서드
    /// </summary>
    protected IEnumerator StartDialogueAndWait(int startIndex, int count, Enums.DialogueMode mode = Enums.DialogueMode.CutScene)
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueRangeByIndex(prologueCSVIndex, startIndex, count, mode);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive());
        }
    }

    #endregion

    #region 씬 전환

    /// <summary>
    /// 다음 씬으로 전환
    /// </summary>
    protected IEnumerator TransitionToNextScene()
    {
        yield return new WaitForSeconds(sceneTransitionDelay);
        SceneManager.LoadScene(nextSceneName);
    }

    #endregion

    #region 이벤트 처리

    protected virtual void OnDialogueCompleted(string setName)
    {
        // 하위 클래스에서 오버라이드 가능
    }

    protected virtual void OnEventFlagTriggered(string eventFlag)
    {
        switch (eventFlag.ToLower())
        {
            case "scene_start":
                StartCoroutine(FadeIn());
                break;
            
            case "scene_end":
                StartCoroutine(WaitForUserInputThenFadeOut());
                break;
            
            default:
                HandleCustomEventFlag(eventFlag);
                break;
        }
    }

    /// <summary>
    /// 각 프롤로그별 커스텀 이벤트 플래그 처리
    /// </summary>
    protected virtual void HandleCustomEventFlag(string eventFlag)
    {
        // 하위 클래스에서 구현
    }

    #endregion
}